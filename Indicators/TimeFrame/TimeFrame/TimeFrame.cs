using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Api;
using Sparks.Common;

namespace Sparks.Scripts.Custom
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
        Datas_     = CreateIndicatorDatas();
    }

    protected IIndicatorDatas Datas_;

    protected override void OnData(ISource source, int index)
    {
        if (source[index] is ITick tick)
        {
            Info($"OnTick:{tick}");
        }
        else if (source[index] is IBar bar)
        {
            Info($"OnBar:{bar}");

            // 在实时阶段，可以通过source.IsLastOpen判断是否新开一个bar
            if (IsHistoryOver && source.IsLastOpen)
            {
                Info("new bar open");
            }
        }
        else if (source[index] is double value)
        {
            Info($"OnData:{value}");
            // 在实时阶段，可以通过source.IsLastOpen判断是否新开一个data
            if (IsHistoryOver && source.IsLastOpen)
            {
                Info("new data open");
            }
        }


        //var t = (source[index] as IBar).Time.TimeOfDay;

        //if (t == TimeBegin || t==TimeEnd)
        //{
        //    Chart.MainArea.DrawVerticalLine((source[index] as IBar).Time.ToString(), (source[index] as IBar).Time, LineStroke);
        //}
    }
}
}