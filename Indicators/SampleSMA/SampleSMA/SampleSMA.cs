using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
	/// <summary>
/// IsOverlay=true, 这是个主图指标，输出数据将绘制在主图上
/// Group = "Trends", 指标分组
/// </summary>
	[Indicator(IsOverlay=true, Group = "Samples")]
public class SampleSMA : Indicator
	{
#region 用户参数
    /// <summary>
    /// 指标公式的数据源
    /// DefaultValue("Closes"), 默认值数据源为IBars的Closes
    /// </summary>
    [Parameter, Display(Name = "Source", GroupName = "Calc"), DefaultValue("Closes")]
    public IDatas Source { get; set; } 

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
    public IIndicatorDatas Result { get;  set; }

#endregion
    /// <summary>
    /// 当指标附加到图表时，将调用此事件方法。它用于初始化您计划在指标中使用的任何变量。您还可以定义和引用其他指标，以使用其他指标的公式创建单个指标
    /// </summary>
    protected override void OnStart()
    {
    }

    /// <summary>
    /// 在每次传入数据时都会调用此方法。在此方法中，您可以编写处理传入数据的逻辑，以计算指标应显示的下一条绘制线
    /// </summary>
    /// <param name="source"></param>
    /// <param name="index"></param>
    protected override void OnData(ISource source, int index)
    {
        /// 如果当前数据源不是我们指定的数据源，则返回
        if (source != Source)
            return;

        /// 如果当前索引小于第一个有意义的指标值的索引，则返回
        if (index < First)
            return;

        int startIndex = index - Periods + 1;

        //此代码将指标计算的结果分配给我们之前定义的 Result 参数。
        Result[index] = Source.Avg(startIndex, index);
    }

    /// <summary>
    /// 当指标停止或者从图表中移除时，将调用此方法。在此方法中，您可以清理任何资源，例如事件订阅或缓存的数据
    /// </summary>
    protected override void OnStop()
    {

    }

    /// <summary>
    /// 第一个有意义的指标值的索引
    /// </summary>
    protected int First => Source.FirstNotNanIndex == -1 ? int.MaxValue : Source.FirstNotNanIndex + Periods - 1;
	}
}
