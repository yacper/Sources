using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
public enum ETestPlaceOrderType
{
    /// <summary>
    /// 市价开单，可选TakeProfit和StopLoss，大部分平台支持
    /// </summary>
    MarketOpen,

    /// <summary>
    /// 限价开单，可选TakeProfit和StopLoss，大部分平台支持
    /// </summary>
    LimitOpen,

    /// <summary>
    /// stop开单，可选TakeProfit和StopLoss，大部分平台支持
    /// </summary>
    StopOpen,

    /// <summary>
    /// 取消订单， 需提供CancelOrderId
    /// </summary>
    CancelOrder,

    /// <summary>
    /// 关闭Trade, 需提供CloseTradeId，外汇平台支持
    /// </summary>
    CloseTrade,

    /// <summary>
    /// 关闭头寸，需提供ClosePositionContract，IB支持
    /// </summary>
    ClosePosition,
}

[Strategy(Group = "Samples")]
public class SamplePlaceOrder : Strategy
{
#region 用户参数

    /// <summary>
    ///  测试类型
    /// </summary>
    [Parameter, DefaultValue(ETestPlaceOrderType.MarketOpen)]
    public ETestPlaceOrderType TestPlaceOrderType { get; set; }

    /// <summary>
    /// 买卖方向
    /// </summary>
    [Parameter, DefaultValue(ETradeDirection.Buy)]
    public ETradeDirection Direction { get; set; }

    /// <summary>
    /// 订单价格，如果是市价单，此参数无效
    /// </summary>
    [Parameter]
    public double Price { get; set; }

    /// <summary>
    /// 订单数量
    /// </summary>
    [Parameter, DefaultValue(1)]
    public int Quantity { get; set; }

    /// <summary>
    /// 订单有效期
    /// </summary>
    [Parameter, DefaultValue(ETIF.GTC)]
    public ETIF Tif { get; set; }

    /// <summary>
    /// 止盈价格，部分平台支持
    /// </summary>
    [Parameter]
    public double? TakeProfit { get; set; }

    /// <summary>
    /// 止损价格，部分平台支持
    /// </summary>
    [Parameter]
    public double? StopLoss { get; set; }


    /// <summary>
    /// 订单标签，用于标识订单，外汇平台支持
    /// </summary>
    [Parameter, DefaultValue("Sample Place Order")]
    public string Comment { get; set; }

    /// <summary>
    /// 要取消的订单的ID
    /// </summary>
    [Parameter]
    public string CancelOrderId { get; set; }

    /// <summary>
    /// 要关闭的Trade的ID
    /// </summary>
    [Parameter]
    public string CloseTradeId { get; set; }

    /// <summary>
    /// 要关闭的Position的合约
    /// </summary>
    [Parameter]
    public Contract? ClosePositionContract { get; set; }

#endregion

    protected override void OnStart()
    {
        if (Runtime != ERuntime.Live &&
            Runtime != ERuntime.Editing)
        {
            Error("此脚本只能在实盘和编辑模式下运行");
            Stop();
            return;
        }

        this.TradingAccount.PendingOrders.Created += (sender, order) => { Info($"订单创建 {order}"); };

        this.TradingAccount.PendingOrders.Filled += (sender, order) => { Info($"订单Filled {order}"); };

        this.TradingAccount.PendingOrders.Cancelled += (sender, order) => { Info($"订单取消 {order}"); };

        this.TradingAccount.PendingOrders.Modified += (sender, order) => { Info($"订单修改 {order}"); };

        this.TradingAccount.Trades.Opened   += (sender, trade) => { Info($"Trade打开 {trade}"); };
        this.TradingAccount.Trades.Modified += (sender, trade) => { Info($"Trade打开 {trade}"); };
        this.TradingAccount.Trades.Closed   += (sender, trade) => { Info($"Trade打开 {trade}"); };

        this.TradingAccount.HistoryOrders.Added += (sender, orders) => { Info($"历史订单添加 {orders.Select(p => p.ToString()).Join(',')}"); };


        switch (TestPlaceOrderType)
        {
            case ETestPlaceOrderType.MarketOpen:
            {
                // 使用chart当前symbol对应的provider的主账户
                var tr = PlaceMarketOrder(Contract,
                                          Direction, Quantity, EOpenClose.Open, Tif,
                                          TakeProfit, StopLoss, Comment,
                                          (result =>
                                              {
                                                  // 执行完毕回调
                                                  if (result.IsSuccessful)
                                                      Info($"市价单发送成功");
                                                  else
                                                      Info($"市价单发送失败 {result}");
                                              }));
            }
                break;
            case ETestPlaceOrderType.LimitOpen:
            {
                var tr = PlaceLimitOrder(Contract,
                                         Direction, Price, Quantity, EOpenClose.Open, Tif,
                                         TakeProfit, StopLoss, Comment,
                                         (result =>
                                             {
                                                 // 执行完毕回调
                                                 if (result.IsSuccessful)
                                                     Info($"限价单发送成功");
                                                 else
                                                     Info($"限价单发送失败 {result}");
                                             }));
            }
                break;
            case ETestPlaceOrderType.StopOpen:
            {
                var tr = PlaceStopOrder(Contract,
                                        Direction, Price, Quantity, EOpenClose.Open, Tif,
                                        TakeProfit, StopLoss, Comment,
                                        (result =>
                                            {
                                                // 执行完毕回调
                                                if (result.IsSuccessful)
                                                    Info($"Stop单发送成功");
                                                else
                                                    Info($"Stop单发送失败 {result}");
                                            }));
            }
                break;
            case ETestPlaceOrderType.CancelOrder:
            {
                var order = TradingAccount.PendingOrders.FirstOrDefault(p => p.Id == CancelOrderId);
                if (order == null)
                {
                    Info($"找不到订单 {CancelOrderId}");
                    return;
                }

                var tr = CancelOrder(order, (result =>
                                                {
                                                    // 执行完毕回调
                                                    if (result.IsSuccessful)
                                                        Info($"{order}订单取消成功");
                                                    else
                                                        Info($"{order} 订单取消失败 {result}");
                                                }));
            }
                ;
                break;

            case ETestPlaceOrderType.CloseTrade:
            {
                var trade = this.TradingAccount.Trades.FirstOrDefault(p => p.Id == CloseTradeId);
                if (trade == null)
                {
                    Info($"找不到Trade {CloseTradeId}");
                    return;
                }

                var tr = CloseTrade(trade, callback: (result =>
                                                         {
                                                             // 执行完毕回调
                                                             if (result.IsSuccessful)
                                                                 Info($"{trade} 关闭成功");
                                                             else
                                                                 Info($"{trade} 关闭失败 {result}");
                                                         }));
            }
                ;
                break;

            case ETestPlaceOrderType.ClosePosition:
            {
                var position = this.TradingAccount.Positions.FirstOrDefault(p => p.Contract == ClosePositionContract);
                if (position == null)
                {
                    Info($"{ClosePositionContract} 没有头寸");
                    return;
                }

                var tr = ClosePosition(position, callback: (result =>
                                                               {
                                                                   // 执行完毕回调
                                                                   if (result.IsSuccessful)
                                                                       Info($"{position} 关闭成功");
                                                                   else
                                                                       Info($"{position} 关闭失败 {result}");
                                                               }));
            }
                ;
                break;
        }
    }
}
}