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
using Neo.Api.MarketData;
using Neo.Api.Providers;
using Neo.Api.Scripts;
using Neo.Api.Symbols;
using Neo.Common.Scripts;
using RLib.Base;
using RLib.Graphics;

namespace Neo
{
[Indicator(Group = "Trends")]
public class SampleSMA : Indicator
{
#region 用户参数
    [Parameter, Display(Name = "Source"), DefaultValue("Closes")]
    public IDatas Source { get; set; }
  
    [Parameter, Display(Name = "Periods"), Range(1, int.MaxValue), DefaultValue(7)]
    public int Periods { get; set; }

    [Output, Stroke("#b667c5")]
    public IIndicatorDatas Result { get; set; }
#endregion

    protected override void OnInit()
    {
    }

    public override void Calculate(int period, bool realtime)
    {
        if (period + 1 < Periods ||
            Source.Count <= period)
            return;

        int startIndex = period - Periods + 1;
        int endIndex   = startIndex + Periods - 1;
        Result[period] = Source.Avg(startIndex, endIndex);
    }
}
}