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
public enum EPivotType
{
    Classic,
    Woodie,
    Fibonacci,
    Camarilla
}


[Indicator(Group = "Custom")]
public class Pivot : Indicator
{
    [Parameter, Display(GroupName = "Inputs"), DefaultValue(EPivotType.Classic)]
    public EPivotType Type { get; set; }


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

        double r3 = 0; // 阻力3
        double r2 = 0; // 阻力2
        double r1 = 0; // 阻力1
        double p  = 0; // 枢轴点P
        double s1 = 0; // 支撑1
        double s2 = 0; // 支撑2
        double s3 = 0; // 支撑3

        switch (Type)
        {
            case EPivotType.Classic:
            {
                p = (h + l + c) / 3;

                r3 = h + 2 * (p - l);
                r2 = p + (h - l);
                r1 = 2 * p - l;

                s1 = 2 * p - h;
                s2 = p - (h - l);
                s3 = l - 2 * (h - p);
            }
                break;
            case EPivotType.Woodie:
            {
                p = (h + l + c * 2) / 4;

                r3 = h + 2 * (p - l);
                r2 = p + (h - l);
                r1 = 2 * p - l;

                s1 = 2 * p - h;
                s2 = p - (h - l);
                s3 = l - 2 * (h - p);
            }
                break;
            case EPivotType.Fibonacci:
            {
                p = (h + l + c) / 3;

                r3 = p + (h - l) * 1;
                r2 = p + (h - l) * 0.618;
                r1 = p + (h - l) * 0.382;

                s1 = p - (h - l) * 0.382;
                s2 = p - (h - l) * 0.618;
                s3 = p - (h - l) * 1;
            }
                break;
            case EPivotType.Camarilla:
            {
                p = (h + l + c) / 3;

                r3 = c + (h - l) * 0.25;
                r2 = c + (h - l) * 0.1666;
                r1 = c + (h - l) * 0.0833;

                s1 = c - (h - l) * 0.0833;
                s2 = c - (h - l) * 0.1666;
                s3 = c - (h - l) * 0.25;
            }
                break;
        }


        // 新的一天
        if (dt.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours) != DayStart.Value.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours)) { DayStart = dt; }

        // 当天还没结束
        {
            var dayEnd = dt;

            var day = dayEnd.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours).Date;

            var fmt = $"f{Symbol.Digits}";

            // 绘制线条
            var obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(p)}", new ChartPoint(DayStart.Value, p), new ChartPoint(dayEnd, p), new Stroke(Chart.Setting.ForegroundColor), true,
                                                   Chart.Setting.DefaultFont.WithColor(Chart.Setting.ForegroundColor), $"P:{p.ToString(fmt)}",
                                                   VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(r3)}", new ChartPoint(DayStart.Value, r3), new ChartPoint(dayEnd, r3), new Stroke(Chart.Setting.BuyColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R3:{r3.ToString(fmt)}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(r2)}", new ChartPoint(DayStart.Value, r2), new ChartPoint(dayEnd, r2), new Stroke(Chart.Setting.BuyColor),
                                               true, Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R2:{r2.ToString(fmt)}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(r1)}", new ChartPoint(DayStart.Value, r1), new ChartPoint(dayEnd, r1), new Stroke(Chart.Setting.BuyColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.BuyColor), $"R1:{r1.ToString(fmt)}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);


            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(s1)}", new ChartPoint(DayStart.Value, s1), new ChartPoint(dayEnd, s1), new Stroke(Chart.Setting.SellColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S1:{s1.ToString(fmt)}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(s2)}", new ChartPoint(DayStart.Value, s2), new ChartPoint(dayEnd, s2), new Stroke(Chart.Setting.SellColor), true,
                                               Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S2:{s2.ToString(fmt)}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);

            obj = Chart.MainArea.DrawTrendLine($"{day}_{nameof(s3)}", new ChartPoint(DayStart.Value, s3), new ChartPoint(dayEnd, s3), new Stroke(Chart.Setting.SellColor),
                                               true, Chart.Setting.DefaultFont.WithColor(Chart.Setting.SellColor), $"S3:{s3.ToString(fmt)}",
                                               VerticalAlignment.Center, HorizontalAlignment.Left);
        }
    }


    protected IBars DayBars;

    protected DateTime? DayStart = null;
}
}