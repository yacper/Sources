/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Api.Attributes;
using Neo.Api.MarketData;
using Neo.Api.Scripts;
using Neo.Api.Symbols;
using Neo.Api.Trading;
using Neo.Common.Scripts;
using Neo.Common.Symbols;
using RLib.Base;
using RLib.Graphics;

namespace Neo.Scripts.Custom
{
[Strategy(Group = "Trends")]
public class Grid : Strategy
{
#region 用户Paras
    [Parameter, Display(Name = "上边界"), ]
    public double Upper { get; set; }
    [Parameter, Display(Name = "中线"), ]
    public double Center { get; set; }
    [Parameter, Display(Name = "下边界"), ]
    public double Lower { get; set; }

    [Parameter, Display(Name = "交易方向"),  DefaultValue(ETradeDirection.Bothway)]
    public ETradeDirection AllowDirection { get; set; }

    [Parameter, Display(Name = "间隔")]
    public double Gap { get; set; } 

    [Parameter, Stroke("#ff0000")]
    public Stroke GridStroke { get; set; } 


#endregion
    protected override void OnStart()
    {
        if (Upper <= Center || Lower >= Center)
        {
            Error("错误的参数");
            Stop();
        }

        //Chart.MainArea.DrawHorizontalLine()

    }

    protected override void OnData(ISource source, int index)
    {

    }
}
}