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
public class SamplePinBar : Indicator
{
#region 用户参数
    [Parameter, Stroke("#b667c5")]
    public Stroke Stroke { get; set; }
    [Parameter, Fill("#b667c5")]
    public Fill Fill { get; set; }
 
    public IIndicatorDatas Result { get; set; }
#endregion

    protected override void OnStart()
    {
        Result = CreateIndicatorDatas();
        Atr_   = CreateIndicator<ATR>(14);
        Mva5_  = CreateIndicator<MVA>(Bars.Closes, 5);
        Mva10_ = CreateIndicator<MVA>(Bars.Closes, 10);
        Dc_    = CreateIndicator<DC>(20);

    }

    protected override void OnData(ISource source, int index)
    {
        //1. 长影线是实体的2倍以上
        // 2. 短影线很短或几乎没有
        // 3, 振幅基本达到atr（太短了意义不大）
        // 4. 逆之前的趋势（暂时用ma5和ma10比较来表示）
        // 5. 位置接近前高或前低（提高准确性）

        if (index < 20)
            return;

        IBar b = (source as IBars)[index];

        //1.
        if (b.LongShadow / b.Solid < 2)
            return;

        //2. 
        if (b.LongShadow / b.ShortShadow < 2)
            return;

        //3.
        if (b.Range / Atr_.ATRLine[index] < 0.8)
            return;

        // 4. 
        if (Mva5_.Result[index] >= Mva10_.Result[index]) // 上涨趋势
        {
            if (!b.LongShadow.NearlyEqual(b.TopShadow))
                return;
        }
        else
        {// 下跌趋势
            if (!b.LongShadow.NearlyEqual(b.BottomShadow))
                return;
        }

        //5.
        if (b.LongShadow.NearlyEqual(b.TopShadow))
        {
            Result[index] = -1;
            Chart.MainArea.DrawIcon($"{Id}_{index}", EChartIconType.DownTriangle, b.Time, b.High, Stroke, Fill);
        }
        else
        {
            Result[index] = 1;
            Chart.MainArea.DrawIcon($"{Id}_{index}", EChartIconType.UpTriangle, b.Time, b.Low, Stroke, Fill);
        }

    }

    protected ATR Atr_;
    protected MVA Mva5_;
    protected MVA Mva10_;
    protected DC  Dc_;
}
}