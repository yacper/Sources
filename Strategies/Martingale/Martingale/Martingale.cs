/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
*********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Common;

namespace Neo.Scripts.Custom
{
	[Strategy(Group = "Reversion")]
public class Martingale : Strategy
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


		protected override void OnData(ISource source, int index)
		{

		}
	}
}
