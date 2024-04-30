/*
R Breaker 是一种 日内 回转交易策略，属于短线交易。日内回转交易是指当天买入或卖出标的后于当日再卖出或买入标的。
日内回转交易通过标的短期波动盈利，低买高卖，时间短、投机性强，适合短线投资者。

R Breaker 主要分为分为反转和趋势两部分。空仓时进行趋势跟随，持仓时等待反转信号反向开仓。

反转和趋势突破的价位点根据前一交易日的收盘价、最高价和最低价数据计算得出，分别为：突破买入价、观察卖出价、反转卖出价、反转买入价、观察买入价和突破卖出价。
计算方法如下：

指标计算方法：

中心价位 P = （H + C + L）/3
突破买入价 = H + 2P -2L
观察卖出价 = P + H - L
反转卖出价 = 2P - L
反转买入价 = 2P - H
观察买入价 = P - (H - L)
突破卖出价 = L - 2(H - P)
（H: 最高价, C: 收盘价, L: 最低价）

触发条件
空仓时：突破策略
    空仓时，当盘中价格>突破买入价，则认为上涨的趋势还会继续，开仓做多；
    空仓时，当盘中价格<突破卖出价，则认为下跌的趋势还会继续，开仓做空。

持仓时：反转策略
    持多单时：当日内最高价>观察卖出价后，盘中价格回落，跌破反转卖出价构成的支撑线时，采取反转策略，即做空；
    持空单时：当日内最低价<观察买入价后，盘中价格反弹，超过反转买入价构成的阻力线时，采取反转策略，即做多。


背后逻辑解析

首先看一下这6个价格与前一日价格之间的关系。

反转卖出价和反转买入价
根据公式推导，发现这两个价格和前一日最高最低价没有确定的大小关系。

观察卖出价和观察买入价。
用观察卖出价 - 前一交易日最高价发现，(H+P-L)-H = P - L >0,说明观察卖出价>前一交易日最高价；同理可证，观察买入价<前一交易日最低价。

突破买入价和突破卖出价
突破买入价>观察卖出价>前一交易日最高价，可以说明突破买入价>>前一交易日最高价。做差后可以发现，突破买入价 - 前一交易日最高价 = 2[(C-L)+(H-L)]/3。

用K线形态表示：

前一交易日K线越长，下影线越长，突破买入价越高。
前一交易日K线越长，上影线越长，突破卖入价越高。

这样一来就可以解释R Breaker背后的逻辑了。
当今日的价格突破前一交易日的最高点，形态上来看会是上涨趋势，具备一定的开多仓条件，但还不够。若前一交易日的下影线越长，说明多空方博弈激烈，多方力量强大。因此可以设置更高的突破买入价，一旦突破说明多方力量稳稳地占据了上风，那么就有理由相信未来会继续上涨。同理可解释突破卖出价背后的逻辑。

持有多仓时，若标的价格持续走高，则在当天收盘之前平仓获利离场。若价格不升反降，跌破观察卖出价时，此时价格仍处于前一交易日最高价之上，继续观望。若继续下跌，直到跌破反转卖出价时，平仓止损。

持有空仓时，若标的价格持续走低，则在当天收盘之前平仓获利离场。若价格不降反升，升至观察买入价时，此时价格仍处于前一交易日最低价之下，继续观望。若继续上涨，直到升至反转买入价时，平仓止损。

策略逻辑

第一步：根据收盘价、最高价和最低价数据计算六个价位。
第二步：如果是空仓条件下，如果价格超过突破买入价，则开多仓；如果价格跌破突破卖出价，则开空仓。
第三步：在持仓条件下:
    持多单时，当最高价超过观察卖出价，盘中价格进一步跌破反转卖出价，反手做空；
    持空单时，当最低价低于观察买入价，盘中价格进一步超过反转买入价，反手做多。
第四步：接近收盘时，全部平仓。

https://www.myquant.cn/docs/python_strategyies/425
*/
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
    [Strategy(Group = "Custom")]
    public class R_Breaker : Strategy
    {
        #region 用户Paras

        // 每笔交易的数量
        [Parameter, Display(GroupName = "Inputs"), Range(1, int.MaxValue), DefaultValue(1)]
        public double Quantity { get; set; }

        #endregion

        protected override void OnStart()
        {


        }

        protected override void OnData(ISource source, int index)
		{
			// ITicks也会触发，过滤掉
			if (source != Bars)
				return;
			

            IBar bar = Bars[index];

            IBar preBar = Bars[index-1];
            var o = preBar.Open;
            var h = preBar.High;
            var l = preBar.Low;
            var c = preBar.Close;

            var p = (h + c + l) / 3;  // 中心价位 P = （H + C + L）/3

            var bBreak = h + 2 * (p - l);  // 突破买入价
            var sSetup = p + (h - l);  // 观察卖出价
            var sEnter = 2 * p - l;  // 反转卖出价
            var bEnter = 2 * p - h;  // 反转买入价
            var bSetup = p - (h - l);  // 观察买入价
            var sBreak = l - 2 * (h - p);  // 突破卖出价

            // 历史数据没有结束，不执行逻辑
            if (!IsHistoryOver)
                return;

            // 正在下单中，不执行逻辑
            if (OrderSending)
                return;

    // 突破策略:
    if (LongOrder_ == null && LongTrade_==null && ShortOrder_==null&&ShortTrade_==null)  // 空仓条件下
    {

        if (bar.Close > bBreak)
        {
            // 在空仓的情况下，如果盘中价格超过突破买入价，则采取趋势策略，即在该点位开仓做多
            ExecuteMarketOrder(Contract, ETradeDirection.Buy, Quantity, Label);
            Info("空仓,盘中价格超过突破买入价: 开仓做多");
        }
        else if(bar.Close < sBreak)
        {
            // 在空仓的情况下，如果盘中价格跌破突破卖出价，则采取趋势策略，即在该点位开仓做空
            ExecuteMarketOrder(Contract, ETradeDirection.Sell, Quantity, Label);
            Info("空仓,盘中价格超过突破买入价: 开仓做多");
        }
    }
    // 设置止损条件
    else{
      // 有持仓时
        // 开仓价与当前行情价之差大于止损点则止损
        if (HasPositionOrTrade(ETradeDirection.Buy) and context.open_position_price - bars[0].close >= STOP_LOSS_PRICE) or \
                (position_short and bars[0].close - context.open_position_price >= STOP_LOSS_PRICE):
            print('达到止损点，全部平仓')
            order_close_all()  # 平仓

        // 反转策略:
        if position_long:  # 多仓条件下
            if data.high.iloc[1] > sSetup and bars[0].close < sEnter:
                # 多头持仓,当日内最高价超过观察卖出价后，
                # 盘中价格出现回落，且进一步跌破反转卖出价构成的支撑线时，
                # 采取反转策略，即在该点位反手做空
                order_close_all()  # 平仓
                order_volume(symbol=context.mainContract, volume=10, side=OrderSide_Sell,
                             order_type=OrderType_Market, position_effect=PositionEffect_Open)  # 做空
                print("多头持仓,当日内最高价超过观察卖出价后跌破反转卖出价: 反手做空")
                context.open_position_price = bars[0].close

        elif position_short:  # 空头持仓
            if data.low.iloc[1] < bSetup and bars[0].close > bEnter:
                # 空头持仓，当日内最低价低于观察买入价后，
                # 盘中价格出现反弹，且进一步超过反转买入价构成的阻力线时，
                # 采取反转策略，即在该点位反手做多
                order_close_all()  # 平仓
                order_volume(symbol=context.mainContract, volume=10, side=OrderSide_Buy,
                             order_type=OrderType_Market, position_effect=PositionEffect_Open)  # 做多
                print("空头持仓,当日最低价低于观察买入价后超过反转买入价: 反手做多")
                context.open_position_price = bars[0].close


		}
}


        protected void ExecuteMarketOrder(Contract contract, ETradeDirection dir, double quantity, string label = null)
        {
            // 下市价单
            var ret = PlaceMarketOrder(contract, dir, quantity, label: label, callback: (e) =>
            {
                if (e.IsSuccessful)
                {
                    // 下单成功，提示
                    MyAlert("Open", e.ToString());

                    // 外汇平台
                    if (e.Trade != null)
                    {
                        if (e.Trade.Direction == ETradeDirection.Buy)
                            LongTrade_ = e.Trade;
                        else
                            ShortTrade_ = e.Trade;
                    }
                    else // IB之类的通用Postion, 没有trade细单
                    {
                        if (e.Order.Direction == ETradeDirection.Buy)
                            LongOrder_ = e.Order;
                        else
                            ShortOrder_ = e.Order;
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

        // 关闭买卖方向上的头寸
        protected void Close(ETradeDirection dir)
        {
            if (dir == ETradeDirection.Buy)
            {
                if (LongTrade_ != null)
                    CloseTrade(LongTrade_);
                else if (LongOrder_ != null)
                    ClosePositionByOrder(LongOrder_);
            }
            else if (dir == ETradeDirection.Sell)
            {
                if (ShortTrade_ != null)
                    CloseTrade(ShortTrade_);
                else if (ShortOrder_ != null)
                    ClosePositionByOrder(ShortOrder_);
            }
        }

        protected void CloseTrade(ITrade t)
        {
            // 关闭单子
            var ret = CloseTrade(t, callback: (e) =>
            {
                if (e.IsSuccessful)
                {
                    // 成功提示
                    MyAlert("close", e.ToString());

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

        // 关闭由Order产生的Postion
        protected void ClosePositionByOrder(IOrder t)
        {
            IPosition p = TradingAccount.Positions.FirstOrDefault(p => p.Contract == t.Contract);
            if (p == null)
                return;
            var ret = ClosePosition(p, t.FilledQuantity, e =>
            {
                if (e.IsSuccessful)
                {
                    MyAlert("close", e.ToString());

                    if (t.Direction == ETradeDirection.Buy)
                        LongOrder_ = null;
                    else
                        ShortOrder_ = null;
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

        protected bool HasPositionOrTrade(ETradeDirection dir) => dir == ETradeDirection.Buy ? LongTrade_ != null || LongOrder_ != null : ShortTrade_ != null || ShortOrder_ != null;

        // 提示，弹窗
        protected void MyAlert(string title, string msg) { Alert(title, msg, AlertAction.Popup()); }

        protected string Label => LongName + Id;



        protected ITrade LongTrade_;
        protected IOrder LongOrder_;
        protected bool LongSending_;
        protected bool LongClosing_;
        protected double LongOpenPrice_ => LongTrade_?.OpenPrice ?? LongOrder_?.AvgFillPrice ?? 0;

        protected ITrade ShortTrade_;
        protected IOrder ShortOrder_;
        protected bool ShortSending_;
        protected bool ShortClosing_;
        protected double ShortOpenPrice_ => ShortTrade_?.OpenPrice ?? ShortOrder_?.AvgFillPrice ?? 0;

        protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
    }
}