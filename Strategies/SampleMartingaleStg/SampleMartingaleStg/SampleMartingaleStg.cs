// 该策略可以在模拟账户上运行，回测暂时不支持子订单（takeprofit/stoploss）

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Strategy(Group = "Samples")]
public class SampleMartingaleStg : Strategy
{
#region 用户Paras

    [Parameter, Display(Name = "Initial Quantity (Lots)", GroupName = "Volume"), DefaultValue(1)]
    public double InitialQuantity { get; set; }

    [Parameter, Display(Name = "Stop Loss Pips", GroupName = "Protection"), DefaultValue(40), Range(1, int.MaxValue)]
    public int StopLossPips { get; set; }

    [Parameter, Display(Name = "Take Profit Pips", GroupName = "Protection"), DefaultValue(40), Range(1, int.MaxValue)]
    public int TakeProfitPips { get; set; }

#endregion

    protected override void OnStart()
    {
        TradingAccount.PendingOrders.Filled += PendingOrders_Filled;

        ExecuteOrder(InitialQuantity, GetRandomTradeDirection());
    }

    private void PendingOrders_Filled(object sender, IOrder e)
    {
        if (Order_ == null)
            return;

        if (e == Order_.TakeProfitOrder) { ExecuteOrder(InitialQuantity, GetRandomTradeDirection()); }
        else if (e == Order_.StopLossOrder) { ExecuteOrder(e.TotalQuantity * 2, e.Direction); }
    }

    private void ExecuteOrder(double quantity, ETradeDirection direction)
    {
        double basePrice  = Symbol.Last;
        double takeProfit = 0;
        double stopLoss   = 0;

        if (direction == ETradeDirection.Buy)
        {
            basePrice  = Symbol.Ask;
            takeProfit = basePrice + TakeProfitPips * Symbol.PointSize;
            stopLoss   = basePrice - StopLossPips * Symbol.PointSize;
        }
        else if (direction == ETradeDirection.Sell)
        {
            basePrice  = Symbol.Bid;
            takeProfit = basePrice - TakeProfitPips * Symbol.PointSize;
            stopLoss   = basePrice + StopLossPips * Symbol.PointSize;
        }

        PlaceMarketOrder(Contract,
                         direction, quantity, EOpenClose.Open,
                         ETIF.GTC, takeProfit, stopLoss, "SampleMartingaleStg", result =>
                         {
                             if (result.IsSuccessful) { Order_ = result.Order; }
                             else { Error("Failed to place order: " + result); }
                         });
    }


    private ETradeDirection GetRandomTradeDirection() { return random.Next(2) == 0 ? ETradeDirection.Buy : ETradeDirection.Sell; }


    protected override void OnData(ISource source, int index) { }

    private   Random random = new Random();
    protected IOrder Order_;
}
}