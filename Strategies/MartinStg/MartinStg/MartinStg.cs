/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:
*********************************************************************/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.MarketData;
using Sparks.Trader.Scripts;
using Sparks.Trader.Symbols;
using Sparks.Trader.Trading;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
[Strategy(Group = "Trends")]
public class MartinStg : Strategy
{
    #region 用户Paras

    [Parameter, Description("初始数量"), DefaultValue(1)]
    public double StartingQuantity { get; set; }

    [Parameter, Description("加仓乘数"), Range(0.1, 200), DefaultValue(1.5)]
    public double Multiplier { get; set; }

    [Parameter, Description("加仓区间"), Range(0.0000001, int.MaxValue), DefaultValue(10)]
    public double OpenRange { get; set; }

    [Parameter, Description("收货区间"), Range(0.0000001, int.MaxValue), DefaultValue(10)]
    public double TpRange { get; set; }

    [Parameter, Description("实例Id"), Range(0, int.MaxValue), DefaultValue(1399)]
    public int MagicNumber { get; set; }

    [Parameter]
    public string Email { get; set; }

    [Parameter]
    public List<string> EmailCC{ get; set; }

    #endregion

    protected override void OnStart() { }

    protected override void OnData(ISource source, int index)
    {
        if (!IsHistoryOver)
            return;

        if (OrderSending)
            return;

        int lastIndex = 0;
        var trades    = TradingAccount.Trades.Where(p => p.Code == Symbol.Code && p.Comment.StartsWith(MagicNumber.ToString())).ToList();
        if (trades.Any())
        {
            var lastTrade = trades.OrderBy(p => p.OpenTime).Last();
            try { lastIndex = TradeLabel.Parse(lastTrade.Comment).Index; }
            catch (Exception e) { Error(e); }

            var plrange = trades.Sum(p => p.PLPips * Symbol.PointSize);

            // takeprofit 所有
            if (plrange >= TpRange)
                trades.ForEach(p => CloseTrade(p));
            else if (plrange < 0 && Math.Abs(lastTrade.OpenPrice - Bars.Closes.LastValue) >= OpenRange)
            {
                ExecuteMarketOrder(Symbol.Contract, lastTrade.Direction, StartingQuantity * Multiplier, Label(lastIndex + 1));
            }
        }
        else // 随意开一个买卖单
        {
            ExecuteMarketOrder(Symbol.Contract, DateTime.Now.Ticks % 2 == 0 ? ETradeDirection.Buy : ETradeDirection.Sell, StartingQuantity,
                               Label(lastIndex + 1));
        }
    }


    protected void ExecuteMarketOrder(Contract contract, ETradeDirection dir, double quantity, string label = null)
    {
        var oi = new MarketOrderReq(contract, dir, Symbol.NormalizeLots(quantity), ETIF.GTC)
        {
            Label = label,
        };

        var ret = PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful)
            {
                if (e.Trade != null) { MyAlert("Open", e.Trade.ToString()); }
            }
            else
                Error(e);

            Openning_ = false;
        });

        if (ret.IsExecuting) { Openning_ = true; }
    }

    protected void CloseTrade(ITrade t)
    {
        if (t == null)
            return;

        var oi = new MarketOrderReq(t.Symbol.Contract, t.Direction.Reverse(), t.Lots)
        {
            CloseTradeId = t.Id,
            OpenClose    = EOpenClose.Close
        };
        var ret = PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful) { MyAlert("close", t.ToString()); }

            Closing_ = false;
        });

        if (ret.IsExecuting) { Closing_ = true; }
    }

    protected void MyAlert(string title, string msg)
    {
        Alert(title, msg, new AlertAction[]
        {
            new PopupAlertAction(), 
            //new EmailAlertAction("469710114@qq.com")
            new EmailAlertAction(Email, EmailCC)
        });
    }

    protected string Label(int index) => new TradeLabel() { MagicNumber = MagicNumber, LongName = LongName, Index = index }.ToString();

    protected bool Openning_;
    protected bool Closing_;
    protected bool OrderSending => Openning_ | Closing_;
}

public class TradeLabel
{
    public override string ToString() => $"{MagicNumber}-{LongName}-{Index}";

    public static TradeLabel Parse(string s)
    {
        try
        {
            var ss = s.Split('-');
            return new TradeLabel()
            {
                MagicNumber = Convert.ToInt32(ss[0]),
                LongName    = ss[1],
                Index       = Convert.ToInt32(ss[2])
            };
        }
        catch{ }

        return null;
    }

    public int    MagicNumber { get; set; }
    public string LongName    { get; set; }
    public int    Index       { get; set; }
}
}