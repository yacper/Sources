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

namespace Neo
{
[Strategy(Group = "Trends")]
public class SampleMaCross : Strategy
{
#region 用户Paras

    [Parameter, DefaultValue("Hello world!")]
    public string Message { get; set; }

#endregion

    protected override void OnInit()
    {
        Logger.Info(Message);
    }

    protected override void OnTick(ITick tick, bool realtime)
    {

    }

    protected override void OnBar(IBar bar, bool realtime)
    {

    }

    protected override void OnHistoryOver() { } // history over
}
}