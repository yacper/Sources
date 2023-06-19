/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		死扛策略 主要用于永远有价值的产品（商品），在其价值低位，死扛做多。不盈利不退出。
				策略基本要求，不能有隔夜费，不然很容易被隔夜费磨死。
				开仓：
					30分钟rsi低于30，并且当前开仓
                加仓：
                    在不利方向继续移动一个加仓区间则加仓。
				平仓：	
					rsi高于70，并且当前存在的头寸处于盈利状态。非盈利头寸-之前open的，不平。
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
using RLib.Graphics;

namespace Neo.Scripts.Custom
{
[Strategy(Group = "Trends")]
public class Mountain : Strategy
{
    public enum EAllowDirection
    {
        Buy  = 2,
        Sell = 4,
    }

#region 用户Paras
    [Parameter, Description("交易方向"), DefaultValue(EAllowDirection.Buy)]
    public EAllowDirection Direction { get; set; }       // 

    [Parameter, Description("Rsi周期"), Range(1, int.MaxValue), DefaultValue(14)]
    public int RsiPeriods { get; set; }

    [Parameter, Description("Rsi High Level"), Range(1, 100), DefaultValue(70)]
    public int RsiHighLevel { get; set; }

    [Parameter, Description("Rsi Low Level"), Range(1, 100), DefaultValue(30)]
    public int RsiLowLevel { get; set; }


    [Parameter, Range(1, int.MaxValue), DefaultValue(1)]
    public double Lots { get; set; }

    [Parameter, Description("加仓区间"), Range(0.000001, double.MaxValue), DefaultValue(1)]
    public double IncreaseGap { get; set; }

    [Parameter, Description("最小开仓区间，与上一个非盈利头寸的最小距离"), Range(0.000001, double.MaxValue), DefaultValue(1)]
    public double MinOpenGap { get; set; }


#endregion

    protected override void OnStart()
    {
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
            if (Direction == EAllowDirection.Buy)
            {  
                if (Rsi_.Result.LastValue < 30 && Pinbar_.Result[index].NearlyEqual(1))
                {
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Lots, Label);
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
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Lots, Label);
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
    protected RSI    Rsi_;}
}