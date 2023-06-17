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
[Indicator(Group = "Trends")]
public class SampleIcon : Indicator
{
#region 用户参数
   
#endregion

    protected override void OnStart()
    {
		
    }

    protected override void OnData(ISource source, int index)
    {
        if (index != source.Count-1)
            return;
        
        IBar b = (source as IBars)[index] ;

        Color  c = Colors.Blue;
        Stroke s = new Stroke(){Color = c};
        Fill   f = new Fill(){Color = c};
        Chart.MainArea.DrawIcon("down", EChartIconType.DownTriangle, b.Time, b.High, s, f);
        Chart.MainArea.DrawIcon("up", EChartIconType.UpTriangle, b.Time, b.Low, s, f);
        Chart.MainArea.DrawIcon("left", EChartIconType.LeftTriangle, b.Time, (b.Low+b.High)/2, s, f);
        Chart.MainArea.DrawIcon("right", EChartIconType.RightTriangle, b.Time, (b.Low+b.High)/2, s, f);

    }

}
}