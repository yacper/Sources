// 该示例演示如何在OnStart()中获取另一个合约的Bars数据
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.MarketData;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Indicator(Group = "Samples")]
public class SampleGetBars : Indicator
{
#region 用户参数
    [Parameter, Display(Name = "Periods"), Range(1, int.MaxValue), DefaultValue(7)]
    public int Periods { get; set; }

    [Parameter, Display(Name = "Periods2"), Range(1, int.MaxValue), DefaultValue(14)]
    public int Periods2 { get; set; }


    [Output, Stroke("white")]
    public IIndicatorDataSeries Result1 { get; set; }

    [Output, Stroke("orange")]
    public IIndicatorDataSeries Result2 { get; set; }
#endregion

    protected override void OnStart()
    {
        IBar b = Bars.FirstOrDefault();

        Bars2_ = GetBars(Contract, ETimeFrame.D1, b.Time.Date);

        // 创建mva
        Mva1_ = Indicators.CreateIndicator<SMA>(Bars.Closes, Periods);
        Mva2_ = Indicators.CreateIndicator<SMA>(Bars2_.Closes, Periods2);
    }

    protected override void OnData(ISource source, int index)
    {
        if(source != Bars)
            return;

        Result1[index] = Mva1_.Result[index];

        var dt     = Bars[index].Time;
        var modDt  = TimeFrame < ETimeFrame.D1 ? dt.ModTimeFrame(ETimeFrame.D1, Symbol.TradingHours) : dt.Date;
        var index2 = Bars2_.Times.IndexOf(modDt);
        if (index2 != -1)
            Result2[index] = Mva2_.Result[index2];
    }

    private SMA Mva1_;
    private SMA Mva2_;

    protected IBars Bars2_;
}
}