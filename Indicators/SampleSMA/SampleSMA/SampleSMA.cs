using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Common;

namespace Neo.Scripts.Custom
{
	[Indicator(Group = "Trends")]
public class SampleSMA : Indicator
	{
#region 用户参数
		[Parameter, Display(Name = "Source"), DefaultValue("Closes")]
	public IDatas Source
		{
			get; set;
		}

		[Parameter, Display(Name = "Periods"), Range(1, int.MaxValue), DefaultValue(7)]
	public int Periods
		{
			get; set;
		}

		[Output, Stroke("#b667c5")]
	public IIndicatorDatas Result
		{
			get; set;
		}
#endregion

		protected override void OnStart()
		{
		}

		protected override void OnData(ISource source, int index)
		{
			if (source != Source)
				return;

			if (index + 1 < Periods ||
			Source.Count <= index)
				return;

			int startIndex = index - Periods + 1;
			int endIndex = startIndex + Periods - 1;
			Result[index] = Source.Avg(startIndex, endIndex);
		}
	}
}
