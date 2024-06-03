// 演示使用talib计算一个简单的sma
// 可以通过nuget引入Atypical.TechnicalAnalysis.Functions， 它实现了TALib的Functions，注意，要加上预览版
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.MarketData;
using Sparks.Trader.Scripts;
using TechnicalAnalysis.Functions;

namespace Sparks.Scripts.Custom
{
[Indicator(Group = "Custom")]
public class SampleReferenceTalib : Indicator
{
#region 用户参数
   /// <summary>
    /// 指标公式的数据源
    /// DefaultValue("Closes"), 默认值数据源为IBars的Closes
    /// </summary>
    [Parameter, Display(Name = "Source", GroupName = "Calc"), DefaultValue("Closes")]
    public IDataSeries Source { get; set; } 

    /// <summary>
    /// 指标中使用的周期数
    /// Range(1, int.MaxValue) 1到int.MaxValue的周期范围
    /// DefaultValue(14), 默认周期为14
    /// </summary>
    [Parameter, Display(Name = "Periods", GroupName = "Calc"), Range(1, int.MaxValue), Step(1), DefaultValue(14)]
    public int Periods { get; set; }

    /// <summary>
    /// 指标输出结果绘制线
    /// </summary>
    [Output, Display(Name = "Result", GroupName = "Out"), Stroke("#b667c5")]
    public IIndicatorDataSeries Result { get;  set; }
#endregion

    protected override void OnStart()
    {
    }

    protected override void OnData(ISource source, int index)
    {
        /// 如果当前数据源不是我们指定的数据源，则返回
        if (source != Source)
            return;

        /// 如果当前索引小于第一个有意义的指标值的索引，则返回
        if (index < First)
            return;

        // 只计算当前索引的指标值
        SmaResult actualResult = TAMath.Sma(
                                            index,
                                            index,
                                            (source as DataSeries).ToArray(), Periods);
        Result[index] = actualResult.Real[0];
    }
/// <summary>
    /// 第一个有意义的指标值的索引
    /// </summary>
    protected int First => Source.FirstNotNanIndex == -1 ? int.MaxValue : Source.FirstNotNanIndex + Periods - 1;
}
}