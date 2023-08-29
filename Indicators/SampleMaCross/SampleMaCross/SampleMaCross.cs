/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Common;
using RLib.Base;

namespace Neo
{
	[Strategy(Group = "Trends")]
public class SampleMaCross : Strategy
	{
#region 用户Paras

		[Parameter, DefaultValue("Hello world!")]
	public string Message
		{
			get; set;
		}

#endregion

		protected override void OnStart()
		{
			Info(Message);
		}

	}
}
