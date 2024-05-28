/*
由Michael Chalek在20世纪80年代开发的Dual Thrust策略是一个非常经典的趋势跟踪策略，是常年排在国外前10大流行策略之一，
曾被《Future Trust》杂志评为最赚钱的策略之一。Dual Thrust系统策略十分简单，思路简明，但正所谓大道至简，该策略适用于股票、期货、外汇等多类型市场，
如果配合上良好的资金管理和策略择时，可以为投资者带来长期稳定的收益。

Dual Thrust的核心思想是以今日开盘价加减一定比例的N周期内的价格振幅（Range），确定上下轨，上下轨的设定是交易策略的核心部分，
在计算时共用到：最高价、最低价、收盘价、开盘价四个参数。当价格超过上轨时，如果持有空仓，先平再开多；如果没有仓位，直接开多。
当价格跌破下轨时，如果持有多仓，则先平仓，再开空仓；如果没有仓位，直接开空仓。

Dual Thrust对于多头和空头的触发条件，考虑了"非对称"的幅度，做多和做空参考的Range可以选择不同的周期数，也可以通过参数K1和K2来确定。
K1 和 K2一般根据自己经验以及回测结果进行优化。

具体计算过程如下：
N日High的最高价HH, N日Close的最低价LC
N日Close的最高价HC，N日Low的最低价LL

    Range = Max(HH-LC,HC-LL)

上轨(upperLine )= Open + K1*Range
下轨(lowerLine )= Open - K2*Range

突破上轨做多，跌破下轨翻空。

2.模型体现的基本思想
首先，HH到LL这个范围表示过去N天中价格的移动区间。HC和LC表示的是过去N天内价格的波动情况。如果HC和LC都在中间位置，
那么表示价格在N天内的高低波动只是一种偶然情况。其余时间波动都很小，而且市场呈现出不明朗的趋势。在HH和LL范围固定的情况下，突破需要的点数相对较少。
如果过去N天内价格都在上涨或下跌，呈现单边趋势，那么则表现为HH和LL的范围较大，HC和LC都趋近于箱体的上/下沿，
则表现为较大的buyrange 和 sellrange，于是乎，价格产生突破的话需要的点数也就更多一些。这样的设计，体现了突破策略的一个基本思路，即“大行情大突破，小行情小突破”。

3. 策略逻辑
(1)当价格向上突破上轨时，如果当时持有空仓，则先平仓，再开多仓；如果没有仓位，则直接开多仓；
(2)当价格向下突破下轨时，如果当时持有多仓，泽县平川，再开空仓；如果没有仓位，则直接开空仓；
第一步：设置参数N、k1、k2
第二步：计算HH、LC、HC、LL
第三步：计算range
第四步：设定做多和做空信号

4.策略在不同行情中的表现情况：
（1）行情连续走强。例如，前日大涨，昨日大涨，今日也大涨。这时，策略会一直向上突破，死死扣住持有的多仓。
（2）行情由强转弱，进入平台期。例如，前日大涨，昨日小涨，今日微涨或微跌。这时，策略仍然会一直持有多仓，不产生任何有效突破。
（3）当行情由弱转强，比如前日小涨，昨日中涨，今日大涨时，策略将会针对之前持有的仓位采取进一步措施。如果之前持有的是反向仓位，则会考虑反手交易；
而如果之前持有的是同向仓位，则会考虑一直抱住。

当K1<<K2时，多头相对容易被触发；当K1>K2时，空头相对容易被触发,当K1>K2时，空头相对容易被触发。
因此，投资者在使用该策略时，一方面可以参考历史数据测试的最优参数，另一方面，则可以根据自己对后势的判断，或从其他大周期的技术指标入手，阶段性地动态调整K1和K2的值。

5.模型的缺陷
由于没有止损止盈模块，模型的表现实际上并不是十分稳定。模型是一个永远在市的策略，碰到信号以后就反手。
但是，进一步设定固定止损位后，发现模型的盈利状况反而下降了。我进一步调试止损位，发现只有当止损位大于1000跳，也就是对应橡胶的5000个点时，
策略的盈利才不会受到止损位的影响。也就是说，最大回撤高达保证金账户的300%，也就是说，如果全部资金投入这样的策略的话，已经爆仓无数次了。
策略风险极大。所以，看上去很美的策略并不一定真的敢用。
1000跳的波动已经远超涨跌停板的限制，这也就意味着在某一时间段内，策略可能连续持有巨额损失仓位达几天之久，因此原先的突破信号与当前行情没有任何关联，
说明这样的收益成分很可能包含了一些运气的因素。

此外，策略中缺乏止盈部分，因此也不会有任何收益被锁定。当然，止盈和止损必须要结合起来看，如果没有止盈只有止损，那么该策略就很可能一直在止损出场而没有盈利了。

6.模型的优点与改进
1）Dual Thrust策略用一种非常简单的逻辑，融入了顺势操作的原理和一定的止损功能，在不同的行情中表现出不一样的变化，设计者的思维独特令人叹服。
2）该策略如果可以转变成日内策略，或许会有一定的效果改善，相信大家可以去挖掘出一种有效的方法。
3）突破区域的设置和参数的运用，似乎缺少一定的客观证据，因此对于未来的收益能力仍然存在较大的质疑。

https://www.myquant.cn/docs/python_strategyies/424
https://zhuanlan.zhihu.com/p/625967507?utm_id=0
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
    public class DualTrust : Strategy
    {
        #region 用户Paras

        // 周期参数
        [Parameter, Display(GroupName = "Inputs"), Range(1, int.MaxValue), DefaultValue(20)]
        public int N { get; set; }

        [Parameter, Display(GroupName = "Inputs"), Range(0, double.MaxValue), DefaultValue(0.2)]
        public double K1 { get; set; }

        [Parameter, Display(GroupName = "Inputs"), Range(0, double.MaxValue), DefaultValue(0.2)]
        public double K2 { get; set; }


        [Parameter, Display(GroupName = "Inputs"), Range(1, int.MaxValue), DefaultValue(1)]
        public double Quantity { get; set; }

        [Parameter, Display(GroupName = "Inputs"), Fill("#332962ff")]
        public Fill FillBackground { get; set; }

        [Output, Display(Name = "上轨", GroupName = "Style"), Stroke("#2962FF")]
        public IIndicatorDataSeries ULine { get; set; }

        [Output, Display(Name = "下轨", GroupName = "Style"), Stroke("#2962FF")]
        public IIndicatorDataSeries LLine { get; set; }
        #endregion

        protected override void OnStart()
        {
            Fill(ULine, LLine, FillBackground);
        }

        protected override void OnData(ISource source, int index)
        {
            // ITicks也会触发，过滤掉
            if (source != Bars)
                return;

            if(index < N)
                return;

            var HH = (source as IBars).Highs.Max(N);
            var LC = (source as IBars).Closes.Min(N);
            var HC = (source as IBars).Closes.Max(N);
            var LL = (source as IBars).Lows.Min(N);

            var range = Math.Max(HH - LC, HC - LL);


            // 设置输出
            ULine[index] = (source as IBars)[index].Open + K1 * range;
            LLine[index] = (source as IBars)[index].Open - K2 * range;

            // 历史数据没有结束，不执行逻辑
            if (!IsHistoryOver)
                return;

            // 正在下单中，不执行逻辑
            if (OrderSending)
                return;

            // 突破上轨，买入
            if ((source as IBars).Closes.CrossOver(ULine))
            {
                // 关闭sell头寸
                Close(ETradeDirection.Sell);

                if (LongTrade_ == null && LongOrder_ == null)
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity, Label);
            }
            // 突破上轨，卖出
            else if ((source as IBars).Closes.CrossDown(LLine))
            {
                // 关闭buy头寸
                Close(ETradeDirection.Buy);

                if (ShortTrade_ == null && ShortOrder_ == null)
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Quantity, Label);
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

        // 提示，弹窗
        protected void MyAlert(string title, string msg) { Alert(title, msg, AlertAction.Popup()); }

        protected string Label => LongName + Id;

        protected ITrade LongTrade_;
        protected IOrder LongOrder_;
        protected bool LongSending_;
        protected bool LongClosing_;

        protected ITrade ShortTrade_;
        protected IOrder ShortOrder_;
        protected bool ShortSending_;
        protected bool ShortClosing_;

        protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
    }
}