/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		适合IB账户的网格交易策略
                Ib只有Postion，没有trade，所以需要自己记录持仓
*********************************************************************/

using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
public class PositionRow
{
    public uint   Index       { get; set; }
    public double Price       { get; set; }
    public double Volume      { get; set; }
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

    [Parameter, DefaultValue(10)]
    public double Step { get; set; }

    [Parameter, DefaultValue(10)]
    public int Steps { get; set; }

    [Parameter, Stroke("#ff0000")]
    public Stroke GridStroke { get; set; }

#endregion

    protected string DescId=>$"{nameof(Grid_IB)}_{Magic}";

    protected string                            PositionFile => $"{DescId}.cfg";
    public ObservableCollection<PositionRow> PositionLog  { get; protected set; }

    public int LastSteps { get; protected set; } = 0;

    protected override void OnStart()
    {
            Info("hello");

        // 检查参数
        if (Direction != ETradeDirection.Buy && Direction != ETradeDirection.Sell) { throw new ArgumentException("Direction must be Buy or Sell"); }

        if (Step <= 0) { throw new ArgumentException($"Step必须大于0:" + Step); }

        double minlot = Symbol.SymbolInfo.LotSize;
        if (BaseLot < minlot) { throw new ArgumentException($"BaseLot:{BaseLot} 必须大于{Symbol.Code}的最小LotSize:{minlot}"); }

        // 读取持仓记录
        LoadPositionLog();

        DrawLines();

        Window_       = new Window() { Title = $"{nameof(Grid_IB)}:{Symbol.Code}|{Chart.TimeFrame}", Width = 400, Height = 400 };
        //Window_.Owner = Application.Current.MainWindow;

        //DataGrid dg = new DataGrid();
        //dg.ItemsSource  = PositionLog;

        var view = new PositionLogView(this);

        Window_.Content = view;
        Window_.Show();
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

        IBar last = source[index] as IBar;

        // 只判断开仓条件，平仓条件在takeprofit中
        if (Direction == ETradeDirection.Buy)
        {
            // 超越边界
            if (last.Close >= BaseLine + Step * Steps ||
                last.Close <= BaseLine - Step)
                return;

            int steps = (int)((last.Close - BaseLine) / Step);
            // Print("•>[RGridEA.mq4:110]: steps: ", steps);
            if (steps != LastSteps)
            {
                Info($"Steps: {GetPriceLine(LastSteps)}[{LastSteps}] -> {GetPriceLine(steps)}[{steps}]");
                TryClosePosition(LastSteps);
                LastSteps = steps;
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
            if (last.Close <= BaseLine - Step * Steps ||
                last.Close >= BaseLine + Step)
                return;

            int steps = (int)((BaseLine - last.Close) / Step);
            if (steps != LastSteps)
            {
                Info($"Steps: {GetPriceLine(LastSteps)}[{LastSteps}] -> {GetPriceLine(steps)}[{steps}]");
                TryClosePosition(LastSteps);
                LastSteps = steps;
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
        if (steps >= Steps || steps <0)
            return;

        double price = GetPriceLine(steps);
        // Print("•>[RGridEA.mq4:171]: price: ", price);
        if (!HasOrderByPriceLine(price)) // 该位置还未开单
        {
            if (CheckCanOpenOrder()) // 可以新开单
            {
                ExecuteLimitOrder(Symbol.Contract, Direction, GetPriceLine(steps), BaseLot);
            }
        }
    }
    void TryClosePosition(int steps) // 在nstep处，尝试平单
    {
        // 不能超越steps
        if (steps >= Steps || steps <0)
            return;

        double price = GetPriceLine(steps);
        // Print("•>[RGridEA.mq4:171]: price: ", price);
        if (HasPositionByPriceLine(price)) // 该位置有持仓
        {
            ClosePosition(Symbol.Contract, Direction, PositionLog.FirstOrDefault(p=>p.Price.NearlyEqual(GetPriceLine(steps))).Volume);
        }
    }



    bool HasOrderByPriceLine(double price)
    {
        return TradingAccount.PendingOrders.Any(p => p.Price.NearlyEqual(price) && p.Direction == Direction);
        //return PositionLog.Any(p => p.Price.NearlyEqual(price));
    }
    bool HasPositionByPriceLine(double price)
    {
        return PositionLog.Any(p => p.Price.NearlyEqual(price) && p.Volume>0);
    }



    void DrawLines()
    {
        double step;
        if (Direction == ETradeDirection.Buy)
            step = Step;
        else
            step = -Step;

        for (int i = 0; i != Steps; ++i)
        {
            double price = BaseLine + step * i;
            var    name  = $"Line_{price}";
            var line = Chart.MainArea.DrawHorizontalLine(name, price, GridStroke);
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
            PositionLog = PositionFile.FileToJsonObj<ObservableCollection<PositionRow>>();
        }
        catch  { }

        if (PositionLog == null)
            PositionLog = new ObservableCollection<PositionRow>();
    }

    internal void SavePositionLog() { PositionLog.ToJsonFile(PositionFile); }

    // 检查订单数
    protected bool CheckCanOpenOrder()
    {
        return PositionLog.Count(p=>!p.Volume.NearlyEqual(0)) < MaxOrder;
    }

    private Window Window_;

#region trading

    protected void ExecuteLimitOrder(SymbolContract contract, ETradeDirection dir, double price, double quantity)
    {
        var oi = new LimitOrderReq(contract, dir, price, quantity)
        {
        };

        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful)
            {
                if (e.Trade != null)
                {
                    MyAlert($"{DescId} Open", e.Trade.ToString());
                    if (e.Trade.Direction == ETradeDirection.Buy)
                        LongTrade_ = e.Trade;
                    else
                        ShortTrade_ = e.Trade;
                }
            }

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

    protected void ClosePosition(SymbolContract contract, ETradeDirection dir, double quantity)
    {
        var oi = new MarketOrderReq(contract, dir.Reverse(), quantity)
        {
            //CloseTradeId = t.Id,
            OpenClose    = EOpenClose.Close
        };
        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful)
            {
                MyAlert($"${DescId} Close", $"Close {contract.Code} {quantity}@Market");
            }

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


    protected void MyAlert(string title, string msg)
    {
        Alert(title, msg, new AlertAction[]
        {
            new PopupAlertAction(), new EmailAlertAction("469710114@qq.com")
        });
    }

    protected string Label => LongName + Id;

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