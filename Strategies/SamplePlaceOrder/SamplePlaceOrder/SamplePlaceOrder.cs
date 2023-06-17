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
[Strategy(Group="Samples")]
public class SamplePlaceOrder : Strategy
{
    protected override void OnStart()
    {
		PlaceMarketOrder(ETradeDirection.Buy, 1, "sampleMarketOrder", (tr) =>
        {
            if (tr.Trade != null)
            {
                Info($"MarketOrder成交:{tr.Trade}");

                CloseTrade(tr.Trade, (tr) =>
                {
                    if(tr.IsSuccessful)
                        Info("关闭交易成功");
                    else
                        Info("关闭交易失败");

                });
            }
        });
		
        //PlaceLimitOrder(ETradeDirection.Buy, 1, 1800, nameof(SamplePlaceOrder));
    }

    protected void PlaceMarketOrder(ETradeDirection dir, double quantity, string label = null, Action<TradeResult> callback = null)
    {
        var oi = new MarketOrderInfo(Symbol.Contract, dir, quantity)
        {
            Label = label
        };
 
        this.TradingAccount.PlaceOrder(oi, callback);
    }

    protected void PlaceLimitOrder(ETradeDirection dir , double quantity, double price, string label = null)
    {
        LimitOrderInfo orderInfo = new LimitOrderInfo(Symbol.Contract, dir, price, quantity, ETIF.GTC)
            { Label = label };

        // 使用chart当前symbol对应的provider的主账户
        TradeOperation operation = TradingAccount.PlaceOrder(orderInfo, tradeResult =>
        {// 执行完毕回调
            if (tradeResult.IsSuccessful)
                Logger.Info($"限价单发送成功 {tradeResult}");
            else
                Logger.Info($"限价单发送失败 {tradeResult}");
        });

        if (operation.IsExecuting)
        {
            Logger.Info("Operation Is Executing");
        }

    }

    protected void CloseTrade(ITrade t, Action<TradeResult> callback = null)
    {
        var oi = new MarketOrderInfo(t.Symbol.Contract, t.Direction.Reverse(), t.Quantity)
        {
            CloseTradeId = t.Id,
            OpenClose    = EOpenClose.Close
        };

        this.TradingAccount.PlaceOrder(oi, callback);
    }

}
}