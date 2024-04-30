using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Indicator(Group = "Samples", IsOverlay = false)]
public class SampleAtr : Indicator
{
#region 用户Paras

    [Parameter, Display(Name = "Periods", GroupName = "Inputs"), Range(1, Int32.MaxValue), DefaultValue(7)]
    public int Periods { get; set; }

    [Output, PlotType(EPlotType.Columns),Display(GroupName = "Style")]
    public IIndicatorDataSeries TR { get; set; }

    [Output,Display(GroupName = "Style"), Stroke("#9ACCFF")]
    public IIndicatorDataSeries Result { get; set; }

#endregion

    protected override void OnStart()
    {
        TRFirst  = 1;
        ATRFirst = 1 + Periods;
    }


    protected override void OnData(ISource source, int index)
    {
        if (index >= TRFirst)
        {
            TR[index] = GetTR_(index);
            TR.SetColor(index, Chart.Setting.GetColor(Bars[index].Direction).WithAlpha(0.7f));
        }

        if (index >= ATRFirst)
            Result[index] = TR.Avg(index - Periods + 1, index);
    }

    protected double GetTR_(int period) // 获取TR
    {
        double hl = Math.Abs(Bars.Highs[period] - Bars.Lows[period]);
        double hc = Math.Abs(Bars.Highs[period] - Bars.Closes[period - 1]);
        double lc = Math.Abs(Bars.Lows[period] - Bars.Closes[period - 1]);

        double tr = hl;
        if (tr < hc)
            tr = hc;

        if (tr < lc)
            tr = lc;

        return tr;
    }


    protected int TRFirst;
    protected int ATRFirst;
}}