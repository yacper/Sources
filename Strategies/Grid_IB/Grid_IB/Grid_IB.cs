/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		适合IB账户的网格交易策略
                Ib只有Postion，没有trade，所以需要自己记录持仓
                buy, baseline=1900, step=10, steps=10 => 第一个单子1900，最后一个单子1990(tp:2000), [1900~1990]
                sell, baseline=2000, step=10, steps=10 => 第一个单子2000，最后一个单子1910(tp:1900), [2000~1910]


                开单的同时，由于无法记录message，只能从自己的记录中找到对应的单子，不能从account的position中判断是否位置已经开单
                这种情况下，使用marketOrder开单，到位置后，判断tp。

                另外，针对ib的情况，必须考虑PlaceOrder timeout的情况，如果timeout，可能是已经成功了，所以要额外挂载事件
*********************************************************************/

//using System.CodeDom;
//using System.Collections.ObjectModel;

using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
using System.Windows;
//using System.Windows.Controls;
//using Microsoft.Maui.Graphics;
using Sparks.MVVM;
using Sparks.Trader.Api;
//using Sparks.Trader.BackTesting;
//using Sparks.Trader.Charts;
//using Sparks.Trader.Common;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
public class PositionRow : ObservableObject
{
    public int    Index              { get; set; } // 仅用于查看，不用于任何逻辑
    public double Price              { get; set; }
    public double Volume             { get; set; }
    public string OpenClientOrderId  { get; set; } // 未回复的开单orderid
    public string CloseClientOrderId { get; set; } // 未回复的平单orderid
}


[Strategy(Group = "Trends")]
public class Grid_IB : Strategy
{
#region 用户Paras

    [Parameter, DefaultValue(10)]
    public double MaxOrder { get; set; }

    [Parameter, DefaultValue(999)]
    public int Magic { get; set; }

    [Parameter, DefaultValue(1)]
    public double BaseLot { get; set; }

    [Parameter, DefaultValue(3)]
    public double Slippage { get; set; }

    [Parameter, DefaultValue(ETradeDirection.Buy)]
    public ETradeDirection Direction { get; set; }

    [Parameter, DefaultValue(1900)]
    public double BaseLine { get; set; }

    [Parameter, DefaultValue(0.5)]
    public double Tolerance { get; set; }

    [Parameter, DefaultValue(10)]
    public double Step { get; set; }

    [Parameter, DefaultValue(10)]
    public int Steps { get; set; }

    [Parameter, Stroke("#ff0000")]
    public Stroke GridStroke { get; set; }

#endregion

    protected string DescId => $"{nameof(Grid_IB)}_{Magic}";

    protected string                            PositionFile => $"{DescId}.cfg";
    public    ObservableCollection<PositionRow> PositionLog  { get; protected set; }

    public int LastSteps { get; protected set; } = 0;

    public string Summary { get; protected set; }

    protected override void OnStart()
    {
        // 检查参数
        if (Direction != ETradeDirection.Buy && Direction != ETradeDirection.Sell) { throw new ArgumentException("Direction must be Buy or Sell"); }

        if (Step <= 0) { throw new ArgumentException($"Step必须大于0:" + Step); }

        double minlot = Symbol.SymbolInfo.LotSize;
        if (BaseLot < minlot) { throw new ArgumentException($"BaseLot:{BaseLot} 必须大于{Symbol.Code}的最小LotSize:{minlot}"); }

        // 读取持仓记录
        LoadPositionLog();

        if (!CheckPositionsFit())
            //throw new Exception($"Positions Not Fit"); 
            ;


        DrawLines();

        // 挂载order事件
        TradingAccount.PendingOrders.Added += PendingOrders_Added;
   

#region UI 不能在回测中使用
        if (Runtime == ERuntime.Live | Runtime == ERuntime.Editing)
        {
            Window_ = new Window() { Title = $"{nameof(Grid_IB)}:{Symbol.Code}|{Chart.TimeFrame}", Width = 400, Height = 400 };
            //Window_.Owner = Application.Current.MainWindow;

            var view = new PositionLogView(this);

            Window_.Content = view;
            Window_.Show();
        }
#endregion
    }


        protected override void OnStop()
    {
        Window_?.Hide();
        Window_?.Close();
        Window_ = null;
        SavePositionLog();
    }


    protected override void OnData(ISource source, int index)
    {
        if (!IsHistoryOver)
            return;

        IBar   last      = source[index] as IBar;
        double lastPrice = last.Close;

        int steps = GetSteps(lastPrice);

        if (steps != LastSteps)
        {
            Info($"Steps change({lastPrice}): {GetPriceLine(LastSteps)}[{LastSteps}] -> {GetPriceLine(steps)}[{steps}]");
            LastSteps = steps;
        }

        // 如果之前平仓失败，防御性平仓
        TryClosePosition(GetPriceLine(steps - 1));

        //之前开仓可能失败，防御性开仓
        TryOpenOrder(steps, lastPrice);

        UpdateSummary();
        // 
        //if (Direction == ETradeDirection.Buy)
        //{
        //    // 超越边界
        //    // last.Close >= BaseLine + Step * Steps ||     // 上边界
        //    if (lastPrice <= BaseLine - Step)  // 下边界
        //        return;


        //    //// 平仓条件
        //    //if (steps > LastSteps)
        //    //{// 向上移动
        //    //    TryClosePosition(LastSteps);
        //    //}


        //    // 开单条件
        //    if (steps >= 0 && steps < Steps)
        //        TryOpenOrder(steps);
        //}
        //else
        //{
        //    // 超越边界
        //    if (//last.Close <= BaseLine - Step * Steps ||
        //        lastPrice >= BaseLine + Step // 上边界
        //        )
        //        return;

        //    //// 平仓条件
        //    //if(steps < LastSteps)
        //    //{
        //    //    TryClosePosition(LastSteps);
        //    //}

        //    LastSteps = steps;

        //    // 开单条件
        //    if (steps >= 0 && steps < Steps)
        //        TryOpenOrder(steps);
        // }
    }

    protected void UpdateSummary() { Summary = $"Positions:{PositionLog.Sum(p => p.Volume)}[{PositionLog.Count}] LastSteps:{LastSteps}"; }

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

    int GetSteps(double price)
    {
        if (Direction == ETradeDirection.Buy) { return (int)((price - BaseLine) / Step); }
        else { return (int)((BaseLine - price) / Step); }
    }


    void TryOpenOrder(int steps, double price) // 在nstep处，尝试开单
    {
        // 不能超越steps
        if (steps >= Steps || steps < 0)
            return;

        // 单子正在发送
        if (LongSending_ | ShortSending_)
            return;

        // 已有单子，只是timeout了，等待回复
        if (HasOrderByPriceLine(price, true))
            return;


        // 检查是否在线的误差范围内，超过太多就不开单
        double priceLine = GetPriceLine(steps);
        if (Direction == ETradeDirection.Buy)
        {
            if (price > priceLine && price - priceLine > Tolerance)
                return;
        }
        else
        {
            if (price < priceLine && priceLine - price < Tolerance)
                return;
        }

        if (!HasPositionByPriceLine(priceLine)) // 该位置还未开单
        {
            if (CheckCanOpenOrder()) // 可以新开单
            {
                EntryOrder(Symbol.Contract, Direction, price, BaseLot, steps);
            }
        }
    }

    internal void TryClosePosition(double priceLine) // 在价格线处平仓
    {
        // 单子正在发送
        if (LongClosing_ | ShortClosing_)
            return;

        // 已有单子，只是timeout了，等待回复
        if (HasOrderByPriceLine(priceLine, false))
            return;

        int steps = GetSteps(priceLine);
        if (HasPositionByPriceLine(priceLine)) // 该位置有持仓
        {
            ClosePosition(Symbol.Contract, Direction, PositionLog.FirstOrDefault(p => p.Price.NearlyEqual(GetPriceLine(steps))).Volume, priceLine);
        }
    }


    bool HasOrderByPriceLine(double price, bool open = true)
    {
        //return TradingAccount.PendingOrders.Any(p => p.Price.NearlyEqual(price) && p.Direction == Direction);
        if(open)
            return PositionLog.Any(p => p.Price.NearlyEqual(price) && !p.OpenClientOrderId.IsNullOrEmpty());
        else
            return PositionLog.Any(p => p.Price.NearlyEqual(price) && !p.CloseClientOrderId.IsNullOrEmpty());
    }

    bool HasPositionByPriceLine(double price) { return PositionLog.Any(p => p.Price.NearlyEqual(price) && p.Volume > 0); }


    void DrawLines()
    {
        double step;
        if (Direction == ETradeDirection.Buy)
            step = Step;
        else
            step = -Step;

        for (int i = 0; i <= Steps; ++i)
        {
            double price = BaseLine + step * i;
            var    name  = $"Line_{price}";
            var    line  = Chart.MainArea.DrawHorizontalLine(name, price, GridStroke);
            line.IsLocked = true;
        }
    }

    void ClearLines()
    {
        //for (int i = 0; i != Lines_.size(); ++i)
        //{
        //    LabeledLine* line = Lines_.get(i);
        //    line.remove();
        //    delete line;
        //}

        //Lines_.clear();
    }

    internal void LoadPositionLog()
    {
        try
        {
            //var poses = PositionFile.FileToJsonObj<List<PositionRow>>();
            //PositionLog = new ObservableCollection<PositionRow>();

            //// 由于baseline可能被修改，所以要更新对应的index
            //foreach (var pos in poses)
            //{
            //    PositionLog.Add(new PositionRow()
            //    {
            //        Index  = GetSteps(pos.Price),
            //        Price  = pos.Price,
            //        Volume = pos.Volume
            //    });
            //}
            PositionLog = PositionFile.FileToJsonObj<ObservableCollection<PositionRow>>();
        }
        catch { }

        if (PositionLog == null)
            PositionLog = new ObservableCollection<PositionRow>();
    }

    internal void SavePositionLog() { PositionLog.ToJsonFile(PositionFile); }

    // 检查订单数
    protected bool CheckCanOpenOrder() { return PositionLog.Count(p => !p.Volume.NearlyEqual(0)) < MaxOrder; }

    private Window Window_;

#region trading

    protected void EntryOrder(SymbolContract contract, ETradeDirection dir, double price, double quantity, int steps)
    {
        Warn($"EntryOrder {contract.Code} {quantity}@{price}[{steps}]...");
        //var oi = new LimitOrderReq(contract, dir, price, quantity)
        var oi = new MarketOrderReq(contract, dir, quantity)
        {
        };

        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            var priceLine = GetPriceLine(steps);
            var row       = PositionLog.FirstOrDefault(p => p.Price.NearlyEqual(priceLine));

            if (e.IsSuccessful)
            {
                // 由于使用marketOrder，成功就意味着开仓了
                if (row == null)
                {
                    row = new PositionRow() { Index = steps, Price = priceLine, Volume = quantity };
                    PositionLog.Add(row);
                }
                else
                {
                    row.Index  =  steps;
                    row.Price  =  priceLine;
                    row.Volume += quantity;
                }

                if (e.Order != null) { }
                //if (e.Trade != null)
                //{
                //    MyAlert($"{DescId} Open", e.Trade.ToString());
                //    if (e.Trade.Direction == ETradeDirection.Buy)
                //        LongTrade_ = e.Trade;
                //    else
                //        ShortTrade_ = e.Trade;
                //}

                var msg = $"EntryOrder {contract.Code} {quantity}@{price}[{steps}] Succeeded.";
                Warn(msg);
                MyAlert($"${DescId} EntryOrder Succeeded", msg);
            }
            else //失败
            {
                var requestOrderId = e.RequestIds.FirstOrDefault();
                var msg       = $"EntryOrder[{requestOrderId}] {contract.Code} {quantity}@{price}[{steps}] Failed: {e.ToString()}";
                Error(msg);
                MyAlert($"${DescId} EntryOrder Failed", msg);


                // entry失败，如果是timeout，可能是已经成功了，所以要检查一下
                if (e.ErrorCode == ETradeErrorCode.Timeout)
                {
                    if (row == null)
                    {
                        row = new PositionRow() { Index = steps, Price = priceLine, Volume = 0 };   // 开仓失败，volume记录为0
                        PositionLog.Add(row);
                    }
                    else
                    {
                        row.Index  =  steps;
                        //row.Volume += quantity;
                    }

                    // 记录open order ic
                    row.OpenClientOrderId = requestOrderId;
                }
            }

            UpdateSummary();
            SavePositionLog();

            if (dir == ETradeDirection.Buy)
                LongSending_ = false;
            else
                ShortSending_ = false;
        });

        if (ret.IsExecuting)
        {
            if (dir == ETradeDirection.Buy)
                LongSending_ = true;
            else
                ShortSending_ = true;
        }
    }

    protected void ClosePosition(SymbolContract contract, ETradeDirection dir, double quantity, double priceLine)
    {
        Warn($"ClosePosition {contract.Code} {quantity}@{priceLine}[{GetSteps(priceLine)}]...");
        var oi = new MarketOrderReq(contract, dir.Reverse(), quantity)
        {
            //CloseTradeId = t.Id,
            OpenClose = EOpenClose.Close
        };
        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            var row   = PositionLog.FirstOrDefault(p => p.Price.NearlyEqual(priceLine));
            var index = GetSteps(priceLine);
            if (e.IsSuccessful)
            {
                if (row != null)
                {
                    row.Index  =  GetSteps(priceLine);
                    row.Volume -= quantity;
                    if (row.Volume.NearlyEqual(0) && row.OpenClientOrderId.IsNullOrEmpty())
                        PositionLog.Remove(row);
                }

                var msg = $"ClosePosition {contract.Code} {quantity}@{priceLine}[{GetSteps(priceLine)}] Succeeded.";
                Warn(msg);
                MyAlert($"${DescId} Close Succeeded", msg);
            }
            else //失败
            {
                var requestOrderId = e.RequestIds.FirstOrDefault();
                var msg = $"ClosePosition[{requestOrderId}] {contract.Code} {quantity}@{priceLine}[{GetSteps(priceLine)}] Failed: {e.ToString()}.";
                Error(msg);
                MyAlert($"${DescId} Close Failed", msg);

                // close失败，如果是timeout，可能是已经close了，所以要检查一下
                if (e.ErrorCode == ETradeErrorCode.Timeout)
                {
                    {
                        row.Index = index;
                    }

                    // 记录open order ic
                    row.CloseClientOrderId = requestOrderId;
                }
            }

            UpdateSummary();
            SavePositionLog();

            if (dir == ETradeDirection.Buy)
                LongClosing_ = false;
            else
                ShortClosing_ = false;
        });

        if (ret.IsExecuting)
        {
            if (dir == ETradeDirection.Buy)
                LongClosing_ = true;
            else
                ShortClosing_ = true;
        }
    }

    // 检查positionLog记录是否同tradingAccount一致
    protected bool CheckPositionsFit()
    {
        IPosition p            = TradingAccount.Positions.FirstOrDefault(p => p.Symbol == Symbol);
        double    sysPosVolume = p?.Lots ?? 0;
        double    logPosVolume = PositionLog.Sum(p => p.Volume);
        if (!logPosVolume.NearlyEqual(sysPosVolume))
        {
            var msg = $"当前记录的Position({logPosVolume}) != 系统Position({sysPosVolume})";
            Error(msg);
            return false;
        }

        return true;
    }

    private void PendingOrders_Added(object sender, IEnumerable<IOrder> e)
    {
        // 有未回复的订单
        if (!HasTimeoutOrder())
            return;

        foreach (var o in e)
        {
            {// timeout 开仓单
                var row = PositionLog.FirstOrDefault(p => p.OpenClientOrderId == o.LocalId);
                if (row != null)
                {
                    row.Volume            += o.OriginalLots;
                    row.OpenClientOrderId =  null;
                    Warn($"Time out Open order Excecuted:{o}");
                }
            }
            {// timeout 平仓单
                var row = PositionLog.FirstOrDefault(p => p.CloseClientOrderId == o.LocalId);
                if (row != null)
                {
                    row.Volume            -= o.OriginalLots;
                    row.CloseClientOrderId =  null;

                    if (row.Volume.NearlyEqual(0) && row.OpenClientOrderId.IsNullOrEmpty())
                        PositionLog.Remove(row);

                    Warn($"Time out close order Excecuted:{o}");
                }
            }
        }
    }

    // 还有未回复的订单（ib tws api特有的问题，回复有的时候特别慢）
    protected bool HasTimeoutOrder()
    {
        return PositionLog.Any(p => !p.OpenClientOrderId.IsNullOrEmpty()  || !p.CloseClientOrderId.IsNullOrEmpty());
    }


    protected void MyAlert(string title, string msg )
    {
        return;
        Alert(title, msg, new AlertAction[]
        {
            new PopupAlertAction(), new EmailAlertAction("469710114@qq.com")
        });
    }

    //    protected string Label => LongName + Id;
    protected bool PositionFit => CheckPositionsFit();

    protected ITrade LongTrade_;
    protected bool   LongSending_;
    protected bool   LongClosing_;
    protected ITrade ShortTrade_;
    protected bool   ShortSending_;
    protected bool   ShortClosing_;

    protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;

#endregion
}
}