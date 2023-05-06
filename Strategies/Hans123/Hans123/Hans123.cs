/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Api.Alert;
using Neo.Api.Attributes;
using Neo.Api.MarketData;
using Neo.Api.Scripts;
using Neo.Api.Symbols;
using Neo.Api.Trading;
using Neo.Common.Scripts;
using Neo.Common.Symbols;
using RLib.Base;
using RLib.Base.Utils;
using RLib.Graphics;

namespace Neo.Scripts.Custom
{
[Strategy(Group = "Trends")]
public class Hans123 : Strategy
{
#region 用户Paras

    [Parameter, Range(1, int.MaxValue), DefaultValue(20)]
    public int DcPeriods { get; set; }

    [Parameter, DefaultValue("20:0:0")]
    public TimeSpan DcStartTime { get; set; }

    [Parameter, DefaultValue("20:30:0")]
    public TimeSpan DcEndTime { get; set; }

    [Parameter, Display(Name = "LastOpenTime"), DefaultValue("22:30:0")]
    public TimeSpan LastOpenTime { get; set; } // 超过这个时间，不再开仓

    [Parameter, Display(Name = "LastExitTime"), DefaultValue("9:0:0")]
    public TimeSpan LastExitTime { get; set; }


    [Output(EOutputType.Line), Stroke("#b667c5")]
    public IIndicatorDatas DcUpper { get; set; }

    [Output(EOutputType.Line), Stroke("#b667c5")]
    public IIndicatorDatas DcLower { get; set; }

    [Output(EOutputType.Line), Stroke("#ff0000")]
    public IIndicatorDatas DcStopUpper { get; set; }

    [Output(EOutputType.Line), Stroke("#ff0000")]
    public IIndicatorDatas DcStopLower { get; set; }


    [Parameter, Range(1, int.MaxValue), DefaultValue(1)]
    public double Quantity { get; set; }

#endregion

    protected override void OnStart()
    {
        Pinbar_ = CreateIndicator<Pinbar>();

        LongTrade_   = null;
        LongSending_ = false;
        LongClosing_ = false;

        ShortTrade_   = null;
        ShortSending_ = false;
        ShortClosing_ = false;

        TodayFinished = false;
    }

    protected override void OnData(ISource source, int index)
    {
        if (!IsHistoryOver)
            return;

        if (index + 1 < DcPeriods)
            return;

        IBar b = (source as IBars)[index];

        // 更新dc
        if (b.LastTime.TimeOfDay.IsWithin(DcStartTime, DcEndTime))
        {
            DcUpper[index] = (source as IBars).Highs.Max(index - DcPeriods + 1, index);
            DcLower[index] = (source as IBars).Lows.Min(index - DcPeriods + 1, index);

            TodayFinished = false;
        }

        // 开仓时间, 未开仓, 当天也未开仓过
        if (!TodayFinished && !Opened && !OrderSending&& b.LastTime.TimeOfDay.IsWithin(DcEndTime, LastOpenTime))
        {
            DcUpper[index] = DcUpper[index-1];
            DcLower[index] = DcLower[index -1] ;


            {
                if (b.Close > DcUp)
                {
                    // long
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity, Label);
                    DcStopUpper[index] = DcUpper[index];
                    DcStopLower[index] = DcLower[index];

                }
                else if (b.Close < DcLow)
                {
                    // short
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Quantity, Label);
                    DcStopUpper[index] = DcUpper[index];
                    DcStopLower[index] = DcLower[index];
                }

            }

        }

        // 正常平仓时间
        if (Opened && !OrderSending)
        //if (Opened && !OrderSending && b.LastTime.TimeOfDay.IsWithin(DcEndTime, LastExitTime))
        {

            if (LongTrade_ != null) // long
            {
                if (b.Close < Math.Max(DcLow, DcStopLower.LastValue) ) // 止损退出
                {
                    CloseTrade(LongTrade_);
                }
            }
            else if (ShortTrade_ != null) // short
            {
                if (b.Close > Math.Min(DcUp, DcStopUpper.LastValue)) // 止损退出
                {
                    CloseTrade(ShortTrade_);
                }
            }

            DcStopUpper[index] = (source as IBars).Highs.Max(index - DcPeriods + 1, index);
            DcStopLower[index] = (source as IBars).Lows.Min(index - DcPeriods + 1, index);
        }

        //// 超时平仓
        //if (Opened && b.LastTime.TimeOfDay >= LastExitTime)
        //{
        //    CloseTrade();
        //}
    }


    protected void ExecuteMarketOrder(SymbolContract contract, ETradeDirection dir, double quantity, string label = null)
    {
        var oi = new MarketOrderInfo(contract, dir, quantity)
        {
            Label = label
        };

        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful)
            {
                if (e.Trade != null)
                {
                    MyAlert("Open Sucess", e.Trade.ToString());
                    if (e.Trade.Direction == ETradeDirection.Buy)
                        LongTrade_ = e.Trade;
                    else
                        ShortTrade_ = e.Trade;
                }
            }
            else { MyAlert("Open Error", e.Trade.ToString()); }

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

    protected void CloseTrade(ITrade t)
    {
        var oi = new MarketOrderInfo(t.Symbol.Contract, t.Direction.Reverse(), t.Quantity)
        {
            CloseTradeId = t.Id,
            OpenClose    = EOpenClose.Close
        };
        var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
        {
            if (e.IsSuccessful)
            {
                MyAlert("close", t.ToString());
                if (t.Direction == ETradeDirection.Buy)
                    LongTrade_ = null;
                else
                    ShortTrade_ = null;

                TodayFinished = true;
            }

            if (t.Direction == ETradeDirection.Buy)
                LongClosing_ = false;
            else
                ShortClosing_ = false;
        });

        if (ret.IsExecuting)
        {
            if (t.Direction == ETradeDirection.Buy)
                LongClosing_ = true;
            else
                ShortClosing_ = true;
        }
    }


    protected void CloseTrade()
    {
        if (Opened && !OrderSending)
        {
            if (LongTrade_ != null)
                CloseTrade(LongTrade_);
            else if (ShortTrade_ != null)
                CloseTrade(ShortTrade_);
        }
    }

    protected void MyAlert(string title, string msg) { Alert(title, msg, new AlertAction[] { new PopupAlertAction(), new EmailAlertAction("469710114@qq.com") }); }

    protected string Label => LongName + Id;

    protected double DcUp  => DcUpper.LastValue;
    protected double DcLow => DcLower.LastValue;

    protected ITrade LongTrade_;
    protected bool   LongSending_;
    protected bool   LongClosing_;

    protected ITrade ShortTrade_;
    protected bool   ShortSending_;
    protected bool   ShortClosing_;

    protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
    protected bool Opened       => LongTrade_ != null || ShortTrade_ != null;

    protected bool TodayFinished = false; // 一天只开一单

    protected Pinbar Pinbar_;
}
}