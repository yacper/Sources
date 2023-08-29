/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		趋势回调开仓
            1. 确认主趋势
            2. 在回调时，通过rsi以及pinbar，重要支撑阻力位置开仓
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Common;
using RLib.Base;

namespace Neo.Scripts.Custom
{
public enum ETrendType
{
    Unknown   = 0x1 << 1,
    UpTrend   = 0x1 << 2,
    DownTrend = 0x1 << 3,
}

[Strategy(Group = "Trends")]
public class TrendPullback : Strategy
{
#region 用户Paras
    [Parameter, Description("趋势类型"), DefaultValue(ETrendType.Unknown)]
    public ETrendType TrendType { get; set; }       // 

    [Parameter, Description("趋势是否由用户指定"), DefaultValue(true)]
    public bool IsTrendUserDefined { get; set; }       // 否，则系统判断趋势


    [Parameter, Description("Rsi周期"), Range(1, int.MaxValue), DefaultValue(14)]
    public int RsiPeriods { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(20)]
    public int DcPeriods { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(1)]
    public double Quantity { get; set; }


    [Output, Stroke("#b667c5")]
    public IOutputIndicatorDatas DcUpper { get; set; }

    [Output, Stroke("#b667c5")]
    public IOutputIndicatorDatas DcLower { get; set; }
#endregion

    protected override void OnStart()
    {
        Pinbar_ = CreateIndicator<Pinbar>();
        Rsi_    = CreateIndicator<RSI>(Bars.Closes, RsiPeriods);

    }


    protected override void OnData(ISource source, int index)
    {
        if (!IsHistoryOver)
            return;

        IBar b = (source as IBars)[index];

        //// 更新dc
        //if (b.LastTime.TimeOfDay.IsWithin(DcStartTime, DcEndTime))
        //{
        //    DcUpper[index] = (source as IBars).Highs.Max(index - DcPeriods + 1, index);
        //    DcLower[index] = (source as IBars).Lows.Min(index - DcPeriods + 1, index);

        //    TodayFinished = false;
        //}

        // 开仓时间, 未开仓
        if (!Opened && !OrderSending)
        {
            if (!IsTrendUserDefined)
                DetectTrend();

            if (TrendType == ETrendType.UpTrend)
            {   /* 上升趋势
                     开仓条件：  1.Rsi小于30
                                 2. 出现pinbar
                */
                if (Rsi_.Result.LastValue < 30 && Pinbar_.Result[index].NearlyEqual(1))
                {
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity, Label);
                }
            }
            else if (TrendType == ETrendType.DownTrend)
            {
                /* 下降趋势
                     开仓条件：  1.Rsi大于70
                                 2. 出现pinbar
                */
                if (Rsi_.Result.LastValue > 70 && Pinbar_.Result[index].NearlyEqual(-1))
                {
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Quantity, Label);
                }
            }
        }

        // 正常平仓时间
        if (Opened && !OrderSending)
        //if (Opened && !OrderSending && b.LastTime.TimeOfDay.IsWithin(DcEndTime, LastExitTime))
        {
            // 一旦开仓，更新dc，用于止盈或止损
            //if (b.LastTime.TimeOfDay.IsWithin(DcStartTime, DcEndTime))
            {
                DcUpper[index] = (source as IBars).Highs.Max(index - DcPeriods + 1, index);
                DcLower[index] = (source as IBars).Lows.Min(index - DcPeriods + 1, index);
            }

            if (LongTrade_ != null) // long
            {/* 平仓条件
                    rsi大于70，
                        1. 出现pinbar
                        2. 跌破dc downChannel
              */
                if ((Rsi_.Result.LastValue > 70 && Pinbar_.Result[index].NearlyEqual(-1)) ||
                    b.Close < DcLower.LastValue)
                {
                    CloseTrade(LongTrade_);
                }
            }
            else if (ShortTrade_ != null) // short
            {/* 平仓条件
                    rsi小于30，
                        1. 出现pinbar
                        2. 跌破dc downChannel
              */
                if ((Rsi_.Result.LastValue < 30 && Pinbar_.Result[index].NearlyEqual(1)) ||
                    b.Close > DcUpper.LastValue)
                {
                    CloseTrade(ShortTrade_);
                }
            }
        }

        //// 超时平仓
        //if (Opened && b.LastTime.TimeOfDay >= LastExitTime)
        //{
        //    CloseTrade();
        //}

    }


    protected void DetectTrend()
    {

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
                if (t.Direction == ETradeDirection.Buy)
                    LongTrade_ = null;
                else
                    ShortTrade_ = null;
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


    protected ITrade LongTrade_;
    protected bool   LongSending_;
    protected bool   LongClosing_;

    protected ITrade ShortTrade_;
    protected bool   ShortSending_;
    protected bool   ShortClosing_;

    protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
    protected bool Opened       => LongTrade_ != null || ShortTrade_ != null;



    protected Pinbar Pinbar_;
    protected RSI    Rsi_;
}
}