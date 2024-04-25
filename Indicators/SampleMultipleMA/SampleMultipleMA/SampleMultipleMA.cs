using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Api.Indicators;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Indicator(Group = "Trends")]
public class SampleMultipleMA : Indicator
{
#region 用户参数
    [Parameter, Display(Name = "Source")]
    public IDatas Source { get; set; }

    [Parameter, Display(Name = "MaType"), DefaultValue(EMaType.Simple)]
    public EMaType MaType { get; set; }

    [Parameter, Display(Name = "Periods1"), Range(1, int.MaxValue), DefaultValue(20)]
    public int Periods1 { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(60)]
    public int Periods2 { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(120)]
    public int Periods3 { get; set; }


    [Output, Display(Name = "Result1"), Stroke("green")]
    public IIndicatorDatas Result1 { get; set; }

    [Output, Stroke("blue")]
    public IIndicatorDatas Result2 { get; set; }

    [Output, Stroke("red")]
    public IIndicatorDatas Result3 { get; set; }
#endregion


    protected override void OnStart()
    {
        Ma1_ = Indicators.MovingAverage(MaType, Source, Periods1);
        Ma2_ = Indicators.MovingAverage(MaType, Source, Periods2);
        Ma3_ = Indicators.MovingAverage(MaType, Source, Periods3);
    }

    protected override void OnData(ISource source, int index)
    {
        Result1[index] = Ma1_.Result[index];
        Result2[index] = Ma2_.Result[index];
        Result3[index] = Ma3_.Result[index];
    }

    private IMovingAverage Ma1_;
    private IMovingAverage Ma2_;
    private IMovingAverage Ma3_;
}
}