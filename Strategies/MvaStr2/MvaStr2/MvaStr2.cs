/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
				均线多头排列开仓
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Api;
using Sparks.Common;

namespace Sparks.Scripts.Custom
{
[Strategy(Group = "Trends")]
public class MvaStr2 : Strategy
{
#region 用户Paras

    [Parameter, Range(1, int.MaxValue), DefaultValue(20)]
    public int Ma1Periods { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(60)]
    public int Ma2Periods { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(89)]
    public int Ma3Periods { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(120)]
    public int Ma4Periods { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(200)]
    public int Ma5Periods { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(1)]
    public double Quantity { get; set; }

    [Output, Stroke("green")]
    public IIndicatorDatas Ma1Result { get; set; }

    [Output, Stroke("blue")]
    public IIndicatorDatas Ma2Result { get; set; }

    [Output, Stroke("pink")]
    public IIndicatorDatas Ma3Result { get; set; }

    [Output, Stroke("aliceblue")]
    public IIndicatorDatas Ma4Result { get; set; }

    [Output, Stroke("purple")]
    public IIndicatorDatas Ma5Result { get; set; }


#endregion

    protected override void OnStart()
    {
        MA1_ = CreateScript<MVA>(Bars.Closes, Ma1Periods);
        MA2_ = CreateScript<MVA>(Bars.Closes, Ma2Periods);
        MA3_ = CreateScript<MVA>(Bars.Closes, Ma3Periods);
        MA4_ = CreateScript<MVA>(Bars.Closes, Ma4Periods);
        MA5_ = CreateScript<MVA>(Bars.Closes, Ma5Periods);

        MAs = new() { MA1_, MA2_, MA3_, MA4_, MA5_ };

        VOL_ = CreateIndicator<VOL>(60);

    }


    protected override void OnData(ISource source, int index)
    {
        Ma1Result[index] = MA1_.Result[index];
        Ma2Result[index] = MA2_.Result[index];
        Ma3Result[index] = MA3_.Result[index];
        Ma4Result[index] = MA4_.Result[index];
        Ma5Result[index] = MA5_.Result[index];

      
        if (!IsHistoryOver)
            return;

        if (OrderSending)
            return;

        LongTrade_  = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Buy && p.Comment == Label);
        ShortTrade_ = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Sell && p.Comment == Label);

        if (LongTrade_ == null && ShortTrade_ == null)
        {
            if (MAs.All(p => p.Result.IsRising) &&   // 全部均线向上
                    MA1_.Result[index]>MA2_.Result[index]
                    //&&
                    //VOL_.Result[index] >VOL_.MaResult[index]*1.2    // 1.2倍量
                    //MA2_.Result[index]>MA3_.Result[index]&&
                    //MA3_.Result[index]>MA4_.Result[index]&&
                    //MA4_.Result[index]>MA5_.Result[index]
                    ) 
            {
                if (LongTrade_ == null)
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity, Label);
            }
            else if (MAs.All(p => p.Result.IsDeclining) &&
                    MA1_.Result[index]<MA2_.Result[index]
                    //&&
                    //VOL_.Result[index] >VOL_.MaResult[index]*1.2  // 1.2倍量
                    //MA2_.Result[index]<MA3_.Result[index]&&
                    //MA3_.Result[index]<MA4_.Result[index]&&
                    //MA4_.Result[index]<MA5_.Result[index]
                    )
            {
                if (ShortTrade_ == null)
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Quantity, Label);
            }
        }
        else
        {
            if (LongTrade_ != null && MA1_.Result.CrossDown(MA2_.Result)) { CloseTrade(LongTrade_); }
            else if (ShortTrade_ != null && MA1_.Result.CrossOver(MA2_.Result)) { CloseTrade(ShortTrade_); }
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


    protected void MyAlert(string title, string msg)
    {
        Alert(title, msg, new AlertAction[] { new PopupAlertAction(), new EmailAlertAction("469710114@qq.com") });
    }

    protected string Label => LongName + Id;


    protected ITrade LongTrade_;
    protected bool   LongSending_;
    protected bool   LongClosing_;
    protected ITrade ShortTrade_;
    protected bool   ShortSending_;
    protected bool   ShortClosing_;

    protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;

    protected MVA       MA1_;
    protected MVA       MA2_;
    protected MVA       MA3_;
    protected MVA       MA4_;
    protected MVA       MA5_;
    protected List<MVA> MAs;
    protected VOL       VOL_;
}
}