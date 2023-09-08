/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
*********************************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
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
        var oi = new MarketOrderReq(Symbol.Contract, dir, quantity)
        {
            Label = label
        };
 
        this.TradingAccount.PlaceOrder(oi, callback);
    }

    protected void PlaceLimitOrder(ETradeDirection dir , double quantity, double price, string label = null)
    {
        var orderInfo = new LimitOrderReq(Symbol.Contract, dir, price, quantity, ETIF.GTC)
            { Label = label };

        // 使用chart当前symbol对应的provider的主账户
        TradeOperation operation = TradingAccount.PlaceOrder(orderInfo, tradeResult =>
        {// 执行完毕回调
            if (tradeResult.IsSuccessful)
                Info($"限价单发送成功 {tradeResult}");
            else
                Info($"限价单发送失败 {tradeResult}");
        });

        if (operation.IsExecuting)
        {
            Info("Operation Is Executing");
        }

    }

    protected void CloseTrade(ITrade t, Action<TradeResult> callback = null)
    {
        var oi = new MarketOrderReq(t.Symbol.Contract, t.Direction.Reverse(), t.Lots)
        {
            CloseTradeId = t.Id,
            OpenClose    = EOpenClose.Close
        };

        this.TradingAccount.PlaceOrder(oi, callback);
    }

}
}