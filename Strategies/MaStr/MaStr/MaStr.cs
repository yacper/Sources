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
using System.Security.Cryptography;
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
public class MvaStr : Strategy
	{
#region 用户Paras
 
		[Parameter, Range(5, 6), DefaultValue(5)]
	public int QuickPeriods
		{
			get; set;
		}
 
		[Parameter, Range(10, 11), DefaultValue(20)]
	public int SlowPeriods
		{
			get; set;
		}
 
		[Parameter, Range(1, 2), DefaultValue(1)]
	public double Quantity
		{
			get; set;
		}
 
		[Output, Stroke("green")]
	public IIndicatorDatas QuickMaResult
		{
			get; set;
		}
 
		[Output, Stroke("blue")]
	public IIndicatorDatas SlowMaResult
		{
			get; set;
		}
#endregion
 
		protected override void OnStart()
		{
			Info("OnStart");
 
			QuickMa_ = CreateScript<MVA>(Bars.Closes, QuickPeriods);
			QuickMaResult.Stroke = new Stroke()
			{
				Color = Colors.Yellow
			};
			SlowMa_ = CreateScript<MVA>(Bars.Closes, SlowPeriods);
		}
 
		protected override void OnData(ISource source, int index)
		{
			//Info($"{this.LongName} OnData {source.ToString()}[{index}]:{source[index]}");
 
			if(QuickPeriods >= SlowPeriods)
				return;
			
			QuickMaResult[index] = QuickMa_.Result[index];
			SlowMaResult[index] = SlowMa_.Result[index];
 
			var longPosition = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Buy);
			var shortPosition = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Sell);
 
			if (QuickMa_.Result.CrossOver(SlowMa_.Result) && longPosition == null)
			{
				if (shortPosition != null)
					CloseTrade(shortPosition);
				ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity);
			}
			else if (QuickMa_.Result.CrossDown(SlowMa_.Result) && shortPosition == null)
			{
				if (longPosition != null)
					CloseTrade(longPosition);
				ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Quantity);
			}
		}
 
 
		protected void ExecuteMarketOrder(SymbolContract contract, ETradeDirection dir, double quantity, string label = null)
		{
			var oi = new MarketOrderInfo(contract, dir, quantity)
			{
				Label = label
			};
 
			this.TradingAccount.PlaceOrder(oi);
		}
 
		protected void CloseTrade(ITrade t)
		{
			var oi = new MarketOrderInfo(t.Symbol.Contract, t.Direction.Reverse(), t.Quantity)
			{
				CloseTradeId = t.Id,
			OpenClose = EOpenClose.Close
			};
 
			this.TradingAccount.PlaceOrder(oi);
		}
 
 
		protected MVA QuickMa_;
		protected MVA SlowMa_;
	}
}