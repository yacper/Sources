﻿/********************************************************************
    created:	2017/4/14 15:14:23
    author:		rush
    email:		黄金20:00 1分钟动量跟随
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
	[Strategy(Group = "Trends")]
	public class RushMinute : Strategy
	{
		public enum ERushType
		{
			ENoRush,
		ERushUp,  // 向上急拉
		ERushDown // 向下急拉
		}


#region 用户Paras

		[Parameter, Description("交易方向"), DefaultValue(ETradeDirection.Bothway)]
		public ETradeDirection Direction
		{
			get; set;
		}

		[Parameter, Description("手数"), Range(1, int.MaxValue), DefaultValue(1)]
		public double Lots
		{
			get; set;
		}


		[Parameter, Description("Rsi周期"), Range(1, int.MaxValue), DefaultValue(14)]
		public int RsiPeriods
		{
			get; set;
		}

		[Parameter, Range(1, int.MaxValue), DefaultValue(20)]
	public int DcPeriods
		{
			get; set;
		}

		[Parameter, Range(1, int.MaxValue), DefaultValue(14)]
	public int AtrPeriods
		{
			get; set;
		}

		[Parameter, DefaultValue("19:30:0")]
	public TimeSpan DcStartTime
		{
			get; set;
		}

		[Parameter, DefaultValue("20:00:0")]
	public TimeSpan DcEndTime
		{
			get; set;
		}

		[Parameter, DefaultValue("20:00:0")]
	public TimeSpan StartOpenTime
		{
			get; set;
		}

		[Parameter, DefaultValue("23:00:0")]
	public TimeSpan EndOpenTime
		{
			get; set;
		}



		[Output, Stroke("#b667c5")]
	public IIndicatorDataSeries DcUpper
		{
			get; set;
		}

		[Output, Stroke("#b667c5")]
	public IIndicatorDataSeries DcLower
		{
			get; set;
		}

#endregion

		protected override void OnStart()
		{
			Pinbar_ = CreateIndicator<Pinbar>();
			Rsi_ = CreateIndicator<RSI>(Bars.Closes, RsiPeriods);
			Atr_ = CreateIndicator<ATR>(AtrPeriods);
		}


		protected override void OnData(ISource source, int index)
		{
			if (!IsHistoryOver)
				return;

			IBar b = (source as IBars)[index];

			// 更新dc
			if (b.LastTime.TimeOfDay.IsWithin(DcStartTime, DcEndTime))
			{
				DcUpper[index] = (source as IBars).Highs.Max(index - DcPeriods + 1, index);
				DcLower[index] = (source as IBars).Lows.Min(index - DcPeriods + 1, index);

				TodayFinished = false;
			}

			// 开仓时间, 未开仓
			if (!Opened && !OrderSending && b.LastTime.TimeOfDay.IsWithin(StartOpenTime, EndOpenTime) && !TodayFinished)
			{
				var rushType = DetectRushType(source, index);
				if (rushType == ERushType.ERushUp)
				{
					/* 上升趋势
                    开仓条件：  1.Rsi小于30
                                2. 出现pinbar
                */
					//if (Rsi_.Result.LastValue < 30 && Pinbar_.Result[index].NearlyEqual(1))
					{
						ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Lots, Label);
						OpenBar = (source as IBars)[index -1];
						TodayFinished = true;
					}
				}
				else if (rushType == ERushType.ERushDown)
				{
					/* 下降趋势
                     开仓条件：  1.Rsi大于70
                                 2. 出现pinbar
                */
					//if (Rsi_.Result.LastValue > 70 && Pinbar_.Result[index].NearlyEqual(-1))
					{
						ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Lots, Label);
						OpenBar = (source as IBars)[index -1];
						TodayFinished = true;
					}
				}
			}

			// 正常平仓时间
			if (Opened && !OrderSending)
			//if (Opened && !OrderSending && b.LastTime.TimeOfDay.IsWithin(DcEndTime, LastExitTime))
			{
				// 一旦开仓，更新dc，用于止盈或止损
				//if (b.LastTime.TimeOfDay.IsWithin(DcStartTime, DcEndTime))
				{
					DcUpper[index] = (source as IBars).Highs.Max(index - DcPeriods + 1, index);
					DcLower[index] = (source as IBars).Lows.Min(index - DcPeriods + 1, index);
				}

				if (LongTrade_ != null) // long
				{
					/* 平仓条件
                        
                        
                                    //rsi大于70，
                                    //    1. 出现pinbar
                                    //    2. 跌破dc downChannel
                              */
					//if ((Rsi_.Result.LastValue > 70 && Pinbar_.Result[index].NearlyEqual(-1)) ||
					//    b.Close < DcLower.LastValue)

					// 止盈平仓
					if ((b.Close - LongTrade_.OpenPrice) > OpenBar.Change)
					{
						CloseTrade(LongTrade_);
					}

					// 止损
					else if (LongTrade_.OpenPrice - b.Close > OpenBar.Change)
					{
						CloseTrade(LongTrade_);
					}

				}
				else if (ShortTrade_ != null) // short
				{
					/* 平仓条件
                                    rsi小于30，
                                        1. 出现pinbar
                                        2. 跌破dc downChannel
                              */
					//if ((Rsi_.Result.LastValue < 30 && Pinbar_.Result[index].NearlyEqual(1)) ||
					//    b.Close > DcUpper.LastValue)
					// 止盈平仓
					if ((b.Close - ShortTrade_.OpenPrice) < OpenBar.Change)
					{
						CloseTrade(ShortTrade_);
					}

					// 止损平仓
					else if ((ShortTrade_.OpenPrice - b.Close) < OpenBar.Change)
					{
						CloseTrade(ShortTrade_);
					}

				}
			}

			//// 超时平仓
			//if (Opened && b.LastTime.TimeOfDay >= LastExitTime)
			//{
			//    CloseTrade();
			//}

		}


		protected ERushType DetectRushType(ISource source, int index)
		{
			if (!(source is IBars))
				return ERushType.ENoRush;

			// bar新开时检测
			if (!(source as IBars).IsLastOpen)
				return ERushType.ENoRush;

			var atr = Atr_.ATRLine.Last(2);

			IBar b = source[index -1] as IBar;

			if (b.Change > atr * 2)  // 超过2倍
			{
				return ERushType.ERushUp;

			}
			else if (b.Change < - atr * 2)
			{
				return ERushType.ERushDown;
			}

			return ERushType.ENoRush;
		}



		protected void ExecuteMarketOrder(Contract contract, ETradeDirection dir, double quantity, string label = null)
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
							MyAlert("Open Sucess", e.Trade.ToString());
							if (e.Trade.Direction == ETradeDirection.Buy)
								LongTrade_ = e.Trade;
							else
								ShortTrade_ = e.Trade;
						}
					}
					else
					{
						MyAlert("Open Error", e.Trade.ToString());
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


		protected void CloseTrade()
		{
			if (Opened && !OrderSending)
			{
				if (LongTrade_ != null)
					CloseTrade(LongTrade_);
				else if (ShortTrade_ != null)
					CloseTrade(ShortTrade_);
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


		protected ITrade LongTrade_;
		protected bool LongSending_;
		protected bool LongClosing_;

		protected ITrade ShortTrade_;
		protected bool ShortSending_;
		protected bool ShortClosing_;

		protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
		protected bool Opened => LongTrade_ != null || ShortTrade_ != null;

		protected bool TodayFinished = false; // 今日结束
		protected IBar OpenBar = null;    // 开仓bar


		protected Pinbar Pinbar_;
		protected RSI Rsi_;
		protected ATR Atr_;
	}

}
