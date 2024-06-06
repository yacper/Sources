using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
[Strategy(Group = "Custom")]
public class Grid : Strategy
{
#region 用户Paras

    [Parameter, DefaultValue(10)]
    public double MaxOrder { get; set; }

    [Parameter, Display(Description = "每日订单最多"), DefaultValue(4)]
    public double DayMaxOrder { get; set; }

    // 在外汇平台有效
    [Parameter, DefaultValue(999)]
    public int Magic { get; set; }

    [Parameter, DefaultValue(1)]
    public double BaseQuantity { get; set; }

    [Parameter, DefaultValue(ETradeDirection.Buy)]
    public ETradeDirection Direction { get; set; }

    [Parameter, DefaultValue(1900)]
    public double BaseLine { get; set; }

    [Parameter, DefaultValue(10)]
    public double Step { get; set; }

    [Parameter, DefaultValue(10)]
    public int Steps { get; set; }

    [Parameter, Stroke("#ff0000")]
    public Stroke GridStroke { get; set; }

#endregion

    protected override void OnStart()
    {
        // 检查参数
        if (Direction != ETradeDirection.Buy && Direction != ETradeDirection.Sell) { throw new ArgumentException("Direction must be Buy or Sell"); }

        if (Step <= 0) { throw new ArgumentException($"Step必须大于0:" + Step); }

        if (Steps <= 0) { throw new ArgumentException($"Steps必须大于0:" + Steps); }

        double minlot = Symbol.SymbolInfo.LotSize;
        if (BaseQuantity < minlot) { throw new ArgumentException($"BaseLot:{BaseQuantity} 必须大于{Symbol.Code}的最小LotSize:{minlot}"); }

        // 读取持仓记录
        //LoadPositionLog();

        DrawLines();
    }

    protected override void OnData(ISource source, int index)
    {
        if (!IsHistoryOver)
            return;

        IBar b    = source[index] as IBar;
        var  last = b.Close;

        // 只判断开仓条件，平仓条件在takeprofit中
        if (Direction == ETradeDirection.Buy)
        {
            // 超越边界
            if (last >= BaseLine + Step * Steps ||
                last <= BaseLine - Step)
                return;

            int steps = (int)((last - BaseLine) / Step);
            if (steps != LastStepNumber)
            {
                //Print(StringFormat("Step: %s[%d] -> %s[%d]", DoubleToString(GetPriceLine(LastSteps), Digits) , LastSteps, DoubleToString(GetPriceLine(steps), Digits), steps));
                Info($"Step: {GetPriceLine(LastStepNumber)}[{LastStepNumber}] -> {GetPriceLine(steps)}[{steps}]");
                LastStepNumber = steps;
            }

            if (steps < 0)
                return;

            // 在当前价格两边各尝试开单
            TryOpenOrder(steps);
            TryOpenOrder(steps + 1);
        }
        else
        {
            // 超越边界
            if (last <= BaseLine - Step * Steps ||
                last >= BaseLine + Step)
                return;

            int steps = (int)((BaseLine - last) / Step);
            if (steps != LastStepNumber)
            {
                Info($"Step: {GetPriceLine(LastStepNumber)}[{LastStepNumber}] -> {GetPriceLine(steps)}[{steps}]");
                LastStepNumber = steps;
            }

            if (steps < 0)
                return;

            TryOpenOrder(steps);
            TryOpenOrder(steps + 1);
        }
    }

    double GetPriceLine(int steps)
    {
        if (Direction == ETradeDirection.Buy)
        {
            double price = BaseLine + Step * steps;
            return Math.Round(price, Symbol.Digits);
        }
        else
        {
            double price = BaseLine - Step * steps;
            return Math.Round(price, Symbol.Digits);
        }
    }

    void TryOpenOrder(int steps) // 在nstep处，尝试开单
    {
        // 不能超越steps
        if (steps >= Steps || steps < 0)
            return;

        // 单子正在发送
        if (SendingOrders_.Contains(steps))
            return;

        double price = GetPriceLine(steps);
        if (!HasPositionByPriceLine(price)) // 该位置还未开单
        {
            if (CheckCanOpenOrder()) // 可以新开单
            {
                EntryOrder(Contract, Direction, BaseQuantity, price, steps);
            }
        }
    }

    // 开单
    protected void EntryOrder(Contract contract, ETradeDirection dir, double quantity, double price, int steps)
    {
        Warn($"EntryOrder [{steps}] {dir} {contract.Code} {quantity}@{price}...");

        IBar b    = Bars.Last();
        var  last = Bars.Closes.LastValue;

        var            tp  = dir == ETradeDirection.Buy ? price + Step : price - Step;
        TradeOperation ret = null;
        if ((dir == ETradeDirection.Buy && last >= price) ||
            (dir == ETradeDirection.Sell && price >= last)
           )
        {
            ret = PlaceLimitOrder(contract, dir, quantity, price, takeProfit: tp, callback: (e) =>
            {
                var priceLine      = GetPriceLine(steps);
                var requestOrderId = e.RequestIds.FirstOrDefault();

                if (e.IsSuccessful)
                {
                    var orderId = e.Order?.Id;

                    var msg = $"EntryOrder[{orderId}] [{steps}] {Direction} {contract.Code} {quantity}@{price} Succeeded.";
                    Warn(msg);
                    //MyAlert($"${DescId} EntryOrder Succeeded", msg);
                }
                else //失败
                {
                    var msg = $"EntryOrder[{requestOrderId}] [{steps}] {Direction} {contract.Code} {quantity}@{price} Failed: {e.ToString()}";
                    Error(msg);
                    //MyAlert($"${DescId} EntryOrder Failed", msg);

                    // entry失败，如果是timeout，可能是已经成功了，所以要检查一下
                    if (e.ErrorCode == ETradeErrorCode.Timeout) { }
                    else // 开仓彻底失败
                    {
                    }
                }

                SendingOrders_.Remove(steps);
            });
        }
        else
        {
            ret = PlaceStopOrder(contract, dir, quantity, price, takeProfit: tp, callback: (e) =>
            {
                var priceLine      = GetPriceLine(steps);
                var requestOrderId = e.RequestIds.FirstOrDefault();

                if (e.IsSuccessful)
                {
                    var orderId = e.Order?.Id;

                    var msg = $"EntryOrder[{orderId}] [{steps}] {Direction} {contract.Code} {quantity}@{price} Succeeded.";
                    Warn(msg);
                    //MyAlert($"${DescId} EntryOrder Succeeded", msg);
                }
                else //失败
                {
                    var msg = $"EntryOrder[{requestOrderId}] [{steps}] {Direction} {contract.Code} {quantity}@{price} Failed: {e.ToString()}";
                    Error(msg);
                    //MyAlert($"${DescId} EntryOrder Failed", msg);

                    // entry失败，如果是timeout，可能是已经成功了，所以要检查一下
                    if (e.ErrorCode == ETradeErrorCode.Timeout) { }
                    else // 开仓彻底失败
                    {
                    }
                }

                SendingOrders_.Remove(steps);
            });
        }

        if (ret.IsExecuting)
        {
            SendingOrders_.Add(steps);

            var d = TimeZoneInfo.ConvertTime(b.Time, TimeZoneInfo.Local, Symbol.TradingHours.TimeZoneInfo).Date;
            if (DayOrderNumber.ContainsKey(d)) { DayOrderNumber[d]++; }
            else { DayOrderNumber.Add(d, 1); }
        }
    }

    // 检查订单数
    protected bool CheckCanOpenOrder()
    {
        // 检查每日订单数
        var d = TimeZoneInfo.ConvertTime(Bars.Last().Time, TimeZoneInfo.Local, Symbol.TradingHours.TimeZoneInfo).Date;
        if (DayOrderNumber.ContainsKey(d))
        {
            if (DayOrderNumber[d] >= DayMaxOrder)
            {
                if (!DayMaxOrderReached)
                {
                    Warn($"DayMaxOrder[{DayMaxOrder}] reached.");
                    DayMaxOrderReached = true;
                }

                return false;
            }
        }

        if (AllOrders() >= MaxOrder)
        {
            if (!MaxOrderReached)
            {
                Warn($"MaxOrder[{MaxOrder}] reached.");
                MaxOrderReached = true;
            }

            return false;
        }

        DayMaxOrderReached = false;
        MaxOrderReached    = false;

        return true;
    }

    string MakeComment(double price)
    {
        //string comment = StringFormat("%s(%i)%s_%s", EAName, Param().getMagic(), EnumToString(Param().getDirection()), DoubleToStr(price, Digits));
        var    fmt     = $"f{Symbol.Digits}";
        string comment = $"{this.GetType().Name}({Magic}){Direction}_{price.ToString(fmt)}";
        return comment;
    }

    bool HasPositionByPriceLine(double price)
    {
        // 只有外汇平台才有comment，判断comment是种更好的方式
        string comment = MakeComment(price);

        var tp = Direction == ETradeDirection.Buy ? price + Step : price - Step;
        tp = Math.Round(tp, Symbol.Digits);

        // 判断子单takeprofit是否存在
        return TradingAccount.PendingOrders.Where(p => p.IsChildOrder).Any(p => p.Contract == Contract &&
                                                                                p.Direction == Direction.Reverse() &&
                                                                                p.Price.NearlyEqual(tp));
    }

    // 当前存在的所有订单
    int AllOrders()
    {
        int n = 0;
        for (int i = 0; i != Steps; i++)
        {
            if (HasPositionByPriceLine(GetPriceLine(i)))
                n++;
        }

        return n;
    }

    // 绘制网格线
    void DrawLines()
    {
        double step = Direction == ETradeDirection.Buy ? Step : -Step;
        for (int i = 0; i != Steps; ++i)
        {
            double price = Math.Round(BaseLine + step * i, Symbol.Digits);
            var    fmt   = $"f{Symbol.Digits}";
            ChartArea.DrawHorizontalLine($"Line_{price.ToString(fmt)}", price, GridStroke, true, null, $"[{i}]{price.ToString(fmt)}", VerticalAlignment.Center, HorizontalAlignment.Left);
        }
    }

    public    int          LastStepNumber { get; protected set; } = 1;
    protected HashSet<int> SendingOrders_ = new HashSet<int>();

    public bool MaxOrderReached    { get; protected set; } = false;
    public bool DayMaxOrderReached { get; protected set; } = false;

    public Dictionary<DateTime, int> DayOrderNumber { get; protected set; } = new Dictionary<DateTime, int>(); // 每日订单数
}
}