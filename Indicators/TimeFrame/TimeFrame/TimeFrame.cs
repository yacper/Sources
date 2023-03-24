using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Api.Alert;
using Neo.Api.Attributes;
using Neo.Api.Charts;
using Neo.Api.MarketData;
using Neo.Api.Providers;
using Neo.Api.Scripts;
using Neo.Api.Symbols;
using Neo.Common.Scripts;
using RLib.Base;
using RLib.Graphics;

namespace Neo.Scripts.Custom
{
[Indicator(Group = "Utils")]
public class TimeFrame : Indicator
{
#region 用户参数
    [Parameter, Display(Name = "TimeBegin"), DefaultValue("2:00:00")]
    public TimeSpan TimeBegin { get; set; }
  
    [Parameter, Display(Name = "TimeEnd"), DefaultValue("9:00:00")]
    public TimeSpan TimeEnd { get; set; }

    [Parameter, Display(Name = "LineStroke"), Stroke("#ffa0a4ad", DashPattern = "1,2")]
    public Stroke LineStroke { get; set; }
#endregion

    protected override void OnStart()
    {
    }

    protected override void OnData(ISource source, int index)
    {
        var t = (source[index] as IBar).Time.TimeOfDay;

        if (t == TimeBegin || t==TimeEnd)
        {
            Chart.MainArea.DrawVerticalLine(t.ToString(), (source[index] as IBar).Time, LineStroke);
        }
    }
}
}