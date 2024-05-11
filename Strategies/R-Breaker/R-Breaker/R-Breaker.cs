/*
R Breaker 是一种 日内 回转交易策略，属于短线交易。日内回转交易是指当天买入或卖出标的后于当日再卖出或买入标的。
日内回转交易通过标的短期波动盈利，低买高卖，时间短、投机性强，适合短线投资者。
R-Breaker策略结合了趋势和反转两种交易方式，所以交易机会相对较多，比较适合日内1分钟K线或者5分钟K线级别的数据。

策略于1994年公开发布，起初专用于对冲，后来延展到波段。
连续15年荣登《Futures Truth Magazine》Top10赚钱策略。2013/04在 《Futures Truth Magazine》杂志S&P 排名第七。


策略背景

在外汇交易系统中，枢轴点 (Pivot Points) 交易方法是一种经典的交易策略。

Pivot Points是一个非常单纯的阻力支撑体系，根据昨日的最高价、最低价和收盘价，计算出七个价位，包括一个枢轴点、三个阻力位和三个支撑位。

阻力线和支撑线是技术分析中经常使用的工具之一，并且支撑线和压力线的作用是可以互相转化的。从交易的角度上来看，Pivot Point好比是作战地图，
给投资者指出了盘中应该关注的支撑和阻力价位，而至于具体的战术配合，Pivot Point并没有具体地规定，完全取决于投资者自身的交易策略。
投资者可以根据盘中价格和枢轴点、支撑位和阻力位的相关走势灵活地制定策略，甚至可以根据关键点位进行加减仓的头寸管理。


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

https://blog.csdn.net/FrankieHello/article/details/100864269
https://www.vnpy.com/forum/topic/1597-jie-mi-bing-qiang-hua-ri-nei-jing-dian-ce-lue-r-breaker
https://www.myquant.cn/docs/python_strategyies/425
https://blog.csdn.net/The_Time_Runner/article/details/89048613
*/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.MarketData;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Strategy(Group = "Custom")]
public class R_Breaker : Strategy
{
#region 用户Paras
    [Parameter, Display(GroupName = "Inputs"), Range(0, double.MaxValue), DefaultValue(0.35)]
    public double Setup_coef { get; set; }

    [Parameter, Display(GroupName = "Inputs"), Range(0, double.MaxValue), DefaultValue(0.25)]
    public double Break_coef { get; set; }

    [Parameter, Display(GroupName = "Inputs"), Range(0, double.MaxValue), DefaultValue(1.07)]
    public double Enter_coef1 { get; set; }

    [Parameter, Display(GroupName = "Inputs"), Range(0, double.MaxValue), DefaultValue(0.07)]
    public double Enter_coef2 { get; set; }

    // 每笔交易的数量
    [Parameter, Display(GroupName = "Inputs"), Range(1, int.MaxValue), DefaultValue(1)]
    public double Quantity { get; set; }

    // Abs止损区间
    [Parameter, Display(GroupName = "Inputs"), Range(0, double.MaxValue), DefaultValue(1)]
    public double StopLossRange { get; set; }

#endregion

    protected override void OnStart()
    {
        if (TimeFrame >= ETimeFrame.D1)
        {
            Warn("该指标应该作用于小于D1的时间框架下");
            Stop();
            return;
        }

        // 获取日线数据
        IBar b = Bars.FirstOrDefault();
        DayBars = GetBars(Contract, ETimeFrame.D1, b.Time.Date.AddDays(-5));
    }

    protected override void OnData(ISource source, int index)
    {
        if (source != Bars)
            return;

        IBar bar = Bars[index];

        // 前一天的数据
        var dt    = bar.Time;
        var modDt = dt.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours);
        int idx   = DayBars.FindIndex(p => p.Time.Date == modDt.Date);
        if (idx == -1)
            return;
        var preBar = DayBars[idx - 1];
        if (preBar == null)
            return;

        if (DayStart == null)
        {
            DayStart = dt;
            return;
        }

        var h = preBar.High;
        var l = preBar.Low;
        var c = preBar.Close;

         
        var sSetup = h + Setup_coef*(c-l); // 阻力2: 观察卖出价
        var bSetup = l - Setup_coef*(h-c); // 支撑2: 观察买入价

        var sEnter = Enter_coef1 / 2 * (h + l) - Enter_coef2 * l; // 阻力1: 反转卖出价
        var bEnter = Enter_coef1 / 2 * (h + l) - Enter_coef2 * h; // 支撑1: 反转买入价

        var bBreak = sSetup + Break_coef*(sSetup - bSetup); // 阻力3: 突破买入价
        var sBreak = bSetup - Break_coef*(sSetup - bSetup); // 支撑3: 突破卖出价


        // 新的一天
        if (dt.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours) != DayStart.Value.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours)) { DayStart = dt; }

        // 绘制关键点
        {
            var dayEnd = dt;

            var day = dayEnd.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours).Date;

            var obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(bBreak)}", new ChartPoint(DayStart.Value, bBreak), new ChartPoint(dayEnd, bBreak), new Stroke(Chart.Setting.BuyColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R3:{bBreak.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(sSetup)}", new ChartPoint(DayStart.Value, sSetup), new ChartPoint(dayEnd, sSetup), new Stroke(Chart.Setting.BuyColor),
                                               true, Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R2:{sSetup.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(sEnter)}", new ChartPoint(DayStart.Value, sEnter), new ChartPoint(dayEnd, sEnter), new Stroke(Chart.Setting.BuyColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R1:{sEnter.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);


            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(bEnter)}", new ChartPoint(DayStart.Value, bEnter), new ChartPoint(dayEnd, bEnter), new Stroke(Chart.Setting.SellColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S1:{bEnter.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(bSetup)}", new ChartPoint(DayStart.Value, bSetup), new ChartPoint(dayEnd, bSetup), new Stroke(Chart.Setting.SellColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S2:{bSetup.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(sBreak)}", new ChartPoint(DayStart.Value, sBreak), new ChartPoint(dayEnd, sBreak), new Stroke(Chart.Setting.SellColor),
                                               true, Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S3:{sBreak.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);
        }


        return;

        // 历史数据没有结束，不执行逻辑
        if (!IsHistoryOver)
            return;


        // 正在下单中，不执行逻辑
        if (OrderSending)
            return;

        // 突破策略:
        if (LongOrder_ == null && LongTrade_ == null && ShortOrder_ == null && ShortTrade_ == null) // 空仓条件下
        {
            if (bar.Close > bBreak)
            {
                // 在空仓的情况下，如果盘中价格超过突破买入价，则采取趋势策略，即在该点位开仓做多
                ExecuteMarketOrder(Contract, ETradeDirection.Buy, Quantity, Label);
                Info("空仓,盘中价格超过突破买入价: 开仓做多");
            }
            else if (bar.Close < sBreak)
            {
                // 在空仓的情况下，如果盘中价格跌破突破卖出价，则采取趋势策略，即在该点位开仓做空
                ExecuteMarketOrder(Contract, ETradeDirection.Sell, Quantity, Label);
                Info("空仓,盘中价格超过突破买入价: 开仓做多");
            }
        }
        // 设置止损条件
        else
        {
            // 有持仓时
            // 开仓价与当前行情价之差大于止损点则止损
            if ((HasPositionOrTrade(ETradeDirection.Buy) && LongOpenPrice_ - bar.Close >= StopLossRange) ||
                (HasPositionOrTrade(ETradeDirection.Sell) && bar.Close - ShortOpenPrice_ >= StopLossRange)
               )
            {
                Info("达到止损点，全部平仓");
                CloseAll(); // 平仓

                // 正在下单中，不执行逻辑
                if (OrderSending)
                    return;
            }

            // 反转策略:
            if (HasPositionOrTrade(ETradeDirection.Buy)) // 多仓条件下
            {
                if (bar.High > sSetup && bar.Close < sEnter)
                {
                    // 多头持仓,当日内最高价超过观察卖出价后，
                    // 盘中价格出现回落，且进一步跌破反转卖出价构成的支撑线时，
                    // 采取反转策略，即在该点位反手做空
                    CloseAll(); // 平仓

                    // 做空
                    ExecuteMarketOrder(Contract, ETradeDirection.Sell, Quantity, Label);

                    Info("多头持仓,当日内最高价超过观察卖出价后跌破反转卖出价: 反手做空");
                }
            }
            else if (HasPositionOrTrade(ETradeDirection.Sell)) // 空头持仓
            {
                if (bar.Low < bSetup && bar.Close > bEnter)
                {
                    // 空头持仓，当日内最低价低于观察买入价后，
                    // 盘中价格出现反弹，且进一步超过反转买入价构成的阻力线时，
                    // 采取反转策略，即在该点位反手做多
                    CloseAll(); // 平仓

                    // 做空
                    ExecuteMarketOrder(Contract, ETradeDirection.Buy, Quantity, Label);

                    Info("空头持仓,当日最低价低于观察买入价后超过反转买入价: 反手做多");
                }
            }
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

    protected void CloseAll()
    {
        Close(ETradeDirection.Buy);
        Close(ETradeDirection.Sell);
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
    protected bool   LongSending_;
    protected bool   LongClosing_;
    protected double LongOpenPrice_ => LongTrade_?.OpenPrice ?? LongOrder_?.AvgFillPrice ?? 0;

    protected ITrade ShortTrade_;
    protected IOrder ShortOrder_;
    protected bool   ShortSending_;
    protected bool   ShortClosing_;
    protected double ShortOpenPrice_ => ShortTrade_?.OpenPrice ?? ShortOrder_?.AvgFillPrice ?? 0;

    protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;


    protected IBars DayBars;

    protected DateTime? DayStart = null;
}
}