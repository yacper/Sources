/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		
*********************************************************************/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Api;
using Sparks.Common;

namespace Sparks
{
	[Strategy(Group = "Trends")]
public class MvaStr : Strategy
	{
#region 用户Paras

		[Parameter, Range(1, 100), DefaultValue(5)]
	public int QuickPeriods
		{
			get; set;
		}

		[Parameter, Range(50, 200), DefaultValue(20)]
	public int SlowPeriods
		{
			get; set;
		}

		[Parameter,  DefaultValue(1)]
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
			if (QuickPeriods >= SlowPeriods)
			{
				Error("Bad Args");
				Stop();
				return;
			}

			QuickMa_ = CreateScript<MVA>(Bars.Closes, QuickPeriods);
			QuickMaResult.Stroke = new Stroke()
			{
				Color = Colors.Yellow
			};
			SlowMa_ = CreateScript<MVA>(Bars.Closes, SlowPeriods);

			//LongTrade_ = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Buy && p.Comment == Label);
			//ShortTrade_ = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Sell && p.Comment == Label);
		}

		protected override void OnData(ISource source, int index)
		{
			//Info($"{this.LongName} OnData {source.ToString()}[{index}]:{source[index]}");

			QuickMaResult[index] = QuickMa_.Result[index];
			SlowMaResult[index] = SlowMa_.Result[index];

			if (!IsHistoryOver)
				return;

			if (OrderSending)
				return;

			LongTrade_ = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Buy && p.Comment == Label);
			ShortTrade_ = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Sell && p.Comment == Label);

			if (QuickMa_.Result.CrossOver(SlowMa_.Result))
			{
				if (ShortTrade_ != null)
					CloseTrade(ShortTrade_);
				if (LongTrade_ == null)
					ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity, Label);
			}
			else if (QuickMa_.Result.CrossDown(SlowMa_.Result))
			{
				if (LongTrade_ != null)
					CloseTrade(LongTrade_);
				if (ShortTrade_ == null)
					ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Quantity, Label);
			}
		}


		protected void ExecuteMarketOrder(SymbolContract contract, ETradeDirection dir, double quantity, string label = null)
		{
			var oi = new MarketOrderReq(contract, dir, quantity)
			{
				Label = label
			};

			var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
				{
					if (e.IsSuccessful)
					{
						if (e.Trade != null)
						{
							MyAlert("Open", e.Trade.ToString());
							if (e.Trade.Direction == ETradeDirection.Buy)
								LongTrade_ = e.Trade;
							else
								ShortTrade_ = e.Trade;
						}
					}

					if (dir == ETradeDirection.Buy)
						LongSending_ = false;
					else
						ShortSending_ = false;
				});

			if (ret.IsExecuting)
			{
				if (dir == ETradeDirection.Buy)
					LongSending_ = true;
				else
					ShortSending_ = true;
			}
		}

		protected void CloseTrade(ITrade t)
		{
			var oi = new MarketOrderReq(t.Symbol.Contract, t.Direction.Reverse(), t.Lots)
			{
				CloseTradeId = t.Id,
			OpenClose = EOpenClose.Close
			};
			var ret = this.TradingAccount.PlaceOrder(oi, (e) =>
				{
					if (e.IsSuccessful)
					{
						MyAlert("close", t.ToString());
						if (t.Direction == ETradeDirection.Buy)
							LongTrade_ = null;
						else
							ShortTrade_ = null;
					}

					if (t.Direction == ETradeDirection.Buy)
						LongClosing_ = false;
					else
						ShortClosing_ = false;
				});

			if (ret.IsExecuting)
			{
				if (t.Direction == ETradeDirection.Buy)
					LongClosing_ = true;
				else
					ShortClosing_ = true;
			}
		}


		protected void MyAlert(string title, string msg)
		{
			Alert(title, msg, new AlertAction[]
				{
					new PopupAlertAction(), new EmailAlertAction("469710114@qq.com")
				});
		}

		protected string Label => LongName + Id;

		protected MVA QuickMa_;
		protected MVA SlowMa_;

		protected ITrade LongTrade_;
		protected bool LongSending_;
		protected bool LongClosing_;
		protected ITrade ShortTrade_;
		protected bool ShortSending_;
		protected bool ShortClosing_;

		protected bool OrderSending =>LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
	}
}
