using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Common;
using RLib.Base;

namespace Neo.Scripts.Custom
{
[Indicator(Group = "Trends")]
public class ZZ : Indicator
{
    [Parameter, Range(1, int.MaxValue), DefaultValue(12)]
    public int Depth { get; set; }

    [Parameter, Range(1, int.MaxValue), DefaultValue(5)]
    public int Deviation { get; set; }

    //[Parameter, Range(1, int.MaxValue), DefaultValue(3)]
    //public int Depth { get; set; j}

    [Output, Stroke("#00eeee")]
    public IIndicatorDatas Result { get; set; }


    [Output, Stroke("#0000ee")]
    public IIndicatorDatas Troughs_ { get; set; }

    [Output, Stroke("#0000ee")]
    public IIndicatorDatas Peeks_ { get; set; }


    protected override void OnStart()
    {
        //Peeks_   = CreateIndicatorDatas();
        //Troughs_ = CreateIndicatorDatas();
        PointSize_ = Symbol.PointSize;
    }

    protected override void OnData(ISource source, int index)
    {
        if (index + 1 < Depth)
            return;

        var low = Bars.Lows.Min(index - Depth + 1, index);
        if (Bars.Lows[index].NearlyEqual(low))
        {
            Troughs_[index] = low;
            int i = 1;
            //for (int i = 1; i < Depth; i++)
            {
                if (!Troughs_[index - i].IsNullOrNan() && Troughs_[index - i] > low)
                    Troughs_[index - i] = double.NaN;
            }
        }
        else
            Troughs_[index] = double.NaN;

        var high = Bars.Highs.Max(index - Depth + 1, index);
        if (Bars.Highs[index].NearlyEqual(high))
        {
            Peeks_[index] = high;
            int i = 1;
            //for (int i = 1; i < Depth; i++)
            {
                if (!Peeks_[index - i].IsNullOrNan() && Peeks_[index - i] < high)
                    Peeks_[index - i] = double.NaN;
            }
        }
        else
            Peeks_[index] = double.NaN;


        if (!Troughs_[index].IsNullOrNan())
        {
            // newlow 
            if (LastIsHigh_ != null && LastIsHigh_.Value == false) { Result[Result.LastNotNanIndex] = double.NaN; }

            Result[index] = Troughs_[index];
            LastIsHigh_   = false;
        }
        else if (!Peeks_[index].IsNullOrNan())
        {
            // new high
            if (LastIsHigh_ != null && LastIsHigh_.Value == true) { Result[Result.LastNotNanIndex] = double.NaN; }

            Result[index] = Peeks_[index];
            LastIsHigh_   = true;
        }
    }

    #region Private fields

    private double PointSize_;

    private bool? LastIsHigh_ = null;

    //private IIndicatorDatas Peeks_;
    //private IIndicatorDatas Troughs_;

    #endregion
}
}