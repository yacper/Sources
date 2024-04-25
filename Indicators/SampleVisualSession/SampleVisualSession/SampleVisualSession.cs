using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Indicator("VisualSession", Desc = "时间区间标识", Group = "Samples")]
public class SampleVisualSession : Indicator
{
    [Parameter, Display(GroupName = "Inputs"), DefaultValue("2:00")]
    public TimeSpan Begin { get; set; }

    [Parameter, Display(GroupName = "Inputs"), DefaultValue("6:00")]
    public TimeSpan End { get; set; }

    [Parameter, Display(Name = "Background", GroupName = "Style"), Fill("#882962ff")]
    public Fill Background { get; set; }

    protected override void OnStart()
    {
        if (Begin.TotalDays > 1 || End.TotalDays > 1)
            throw new Exception($"Begin:{Begin} or End:{End} should <= 1 day");

        if (Chart.TimeFrame > ETimeFrame.D1)
            throw new Exception($"TimeFrame:{Chart.TimeFrame} should <= ETimeFrame.D1");
    }

    protected override void OnData(ISource source, int index)
    {
        IBar b = Bars[index];

        if (IsInterDay)
        {
            if (b.Time.TimeOfDay >= Begin || b.Time.TimeOfDay <= End)
                SetBackground(index, Background);
        }
        else
        {
            if (b.Time.TimeOfDay >= Begin && b.Time.TimeOfDay <= End)
                SetBackground(index, Background);
        }
    }

    protected bool IsInterDay => Begin > End; // 是否跨日
}
}