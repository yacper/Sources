// 枢轴点指标
// 枢轴点的用法有多种，该指标主要计算了日线级别的关键点位，然后在
// https://zhuanlan.zhihu.com/p/164500867
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.MarketData;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Indicator(Group = "Custom")]
public class Pivot : Indicator
{
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

    protected override void OnStop() { DayStart = null; }

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

        // 枢轴点P
        var p = (h + l + c) / 3;

        // 阻力3: 突破买入价
        var trendBuy = h + 2 * (p - l);
        trendBuy = Math.Round(trendBuy, Symbol.Digits);
        // 阻力2: 观察卖出价
        var sellCheck = p + (h - l);
        sellCheck = Math.Round(sellCheck, Symbol.Digits);
        // 阻力3: 反转卖出价
        var revSell = 2 * p - l;
        revSell = Math.Round(revSell, Symbol.Digits);


        // 支撑1: 反转买入价
        var revBuy = 2 * p - h;
        revBuy = Math.Round(revBuy, Symbol.Digits);
        // 支撑2: 观察买入价
        var buyCheck = p - (h - l);
        buyCheck = Math.Round(buyCheck, Symbol.Digits);
        // 支撑3: 突破卖出价
        var trendSell = l - 2 * (h - p);
        trendSell = Math.Round(trendSell, Symbol.Digits);


        // 新的一天
        if (dt.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours) != DayStart.Value.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours))
        {
            DayStart = dt;
        }

        // 当天还没结束
        {
            var dayEnd = dt;

            var day = dayEnd.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours).Date;

            // 绘制线条
            var obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(p)}", new ChartPoint(DayStart.Value, p), new ChartPoint(dayEnd, p), new Stroke(Chart.Setting.ForegroundColor), true,
                                                   Chart.Setting.DefaultFont.WithColor(Chart.Setting.ForegroundColor), $"P:{p.ToString("0.00")}",
                                                   VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(trendBuy)}", new ChartPoint(DayStart.Value, trendBuy), new ChartPoint(dayEnd, trendBuy), new Stroke(Chart.Setting.BuyColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R3:{trendBuy.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(sellCheck)}", new ChartPoint(DayStart.Value, sellCheck), new ChartPoint(dayEnd, sellCheck), new Stroke(Chart.Setting.BuyColor),
                                               true, Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R2:{sellCheck.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(revSell)}", new ChartPoint(DayStart.Value, revSell), new ChartPoint(dayEnd, revSell), new Stroke(Chart.Setting.BuyColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R1:{revSell.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);


            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(revBuy)}", new ChartPoint(DayStart.Value, revBuy), new ChartPoint(dayEnd, revBuy), new Stroke(Chart.Setting.SellColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S1:{revBuy.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(buyCheck)}", new ChartPoint(DayStart.Value, buyCheck), new ChartPoint(dayEnd, buyCheck), new Stroke(Chart.Setting.SellColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S2:{buyCheck.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(trendSell)}", new ChartPoint(DayStart.Value, trendSell), new ChartPoint(dayEnd, trendSell), new Stroke(Chart.Setting.SellColor),
                                               true, Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S3:{trendSell.ToString("0.00")}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);
        }
    }


    protected IBars DayBars;

    protected DateTime? DayStart = null;
}
}