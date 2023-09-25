/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
*********************************************************************/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Strategy(Group = "Trends")]
public class Martin : Strategy
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


#endregion

    protected override void OnStart()
    {
    }


    protected override void OnData(ISource source, int index)
    {
        if (!IsHistoryOver)
            return;

        if (OrderSending)
            return;

        Trades = TradingAccount.Trades.Where(p => p.Code == Symbol.Code && p.Comment == Label)
                               .ToList();

        // 随意开一个买卖单
        if (!Trades.Any())
        {
            ExecuteMarketOrder(Symbol.Contract, DateTime.Now.Ticks % 2 == 0 ? ETradeDirection.Buy : ETradeDirection.Sell, StartingQuantity,
                               Label);
            return;
        }

        var plrange = Trades.Sum(p => p.PLPips * Symbol.PointSize);
        var last    = Trades.OrderBy(p => p.OpenTime).Last();
        LastIndex = last.Comment;

            // tp 所有
        if (plrange >= TpRange)
            Trades.ForEach(p => CloseTrade(p));
        else if (plrange < 0 && Math.Abs(last.OpenPrice - Bars.Closes.LastValue) >= OpenRange)
        {
            ExecuteMarketOrder(Symbol.Contract, last.Direction, StartingQuantity * Multiplier, Label(LastIndex));
        }
    }


    protected void ExecuteMarketOrder(SymbolContract contract, ETradeDirection dir, double quantity, string label = null)
    {
        var oi = new MarketOrderReq(contract, dir, quantity)
        {
            Label = label
        };

        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful)
            {
                if (e.Trade != null)
                {
                    MyAlert("Open", e.Trade.ToString());
                }
            }

            Openning_ = false;
        });

        if (ret.IsExecuting)
        {
            Openning_ = true;
        }
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
        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful)
            {
                MyAlert("close", t.ToString());
            }

            Closing_ = false;
        });

        if (ret.IsExecuting)
        {
            Closing_ = true;
        }
    }

    protected void MyAlert(string title, string msg)
    {
        Alert(title, msg, new AlertAction[]
        {
            new PopupAlertAction(), new EmailAlertAction("469710114@qq.com")
        });
    }

    protected string Label(int index) => $"[{MagicNumber}]{LongName}-{index}";
    protected int    LastIndex = 1;

    protected List<ITrade> Trades;
    protected bool   Openning_;
    protected bool   Closing_;
    protected bool   OrderSending => Openning_ | Closing_;

}
}