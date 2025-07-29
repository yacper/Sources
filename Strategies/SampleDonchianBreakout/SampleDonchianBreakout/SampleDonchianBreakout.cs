/// Dochian Channel的简单突破策略，处理订单时，针对两种类型的交易通道做了不同的处理：
///     对于像外汇平台这种有Trade的，直接通过Trade操作，在回测中，也是这种情况
///     对于IB这种没有Trade的，通过Order配合Position操作
/// 
// 为了防止盘中均线多次互相穿越，主要逻辑在bar新开时执行，研判上一个bar结束时的均线情况
// bar新开时，判断上一个bar的快速均线是否上穿慢速均线，如果是，平空单，开多单。 如果上一个bar的快速均线下穿慢速均线，平多单，开空单

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Api.Indicators;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
    [Strategy(Group = "Samples")]
    public class SampleDonchianBreakout : Strategy
    {
        #region 用户Paras
        // 周期参数
        [Parameter, Display(GroupName = "Inputs"), Range(1, int.MaxValue), DefaultValue(20)]
        public int Periods { get; set; }

        // 每笔交易的数量
        [Parameter, Display(GroupName = "Inputs"), Range(1, int.MaxValue), DefaultValue(1)]
        public double Quantity { get; set; }

        [Parameter, Display(GroupName = "Inputs"), Fill("#332962ff")]
        public Fill FillBackground { get; set; }

        [Output, Display(Name = "上轨", GroupName = "Style"), Stroke("#2962FF")]
        public IIndicatorDataSeries ULine { get; set; }

        [Output, Display(Name = "中轨", GroupName = "Style"), Stroke("#88FF6D00")]
        public IIndicatorDataSeries MLine { get; set; }

        [Output, Display(Name = "下轨", GroupName = "Style"), Stroke("#2962FF")]
        public IIndicatorDataSeries LLine { get; set; }

        #endregion

        protected override void OnStart()
        {
            // 创建Donchian Channel指标
            Dc_ = Indicators.DC(Periods);

            Fill(ULine, LLine, FillBackground);
        }

        protected override void OnData(ISource source, int index)
        {
            // ITicks也会触发，过滤掉
            if (source != Bars)
                return;

            // 设置输出
            ULine[index] = Dc_.ULine[index];
            LLine[index] = Dc_.LLine[index];
            MLine[index] = Dc_.MLine[index];


            // 历史数据没有结束，不执行逻辑
            if (!IsHistoryOver)
                return;

            // 正在下单中，不执行逻辑
            if (OrderSending)
                return;

            // 突破上轨，买入
            if ((source as IBars).Closes.CrossOver(ULine))
            {
                // 关闭sell头寸
                Close(ETradeDirection.Sell);

                if (LongTrade_ == null && LongOrder_ == null)
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity, Label);
            }
            // 突破上轨，卖出
            else if ((source as IBars).Closes.CrossDown(LLine))
            {
                // 关闭buy头寸
                Close(ETradeDirection.Buy);

                if (ShortTrade_ == null && ShortOrder_ == null)
                    ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Sell, Quantity, Label);
            }
        }


        protected void ExecuteMarketOrder(Contract contract, ETradeDirection dir, double quantity, string label = null)
        {
            // 下市价单
            var ret = PlaceMarketOrder(contract, dir, quantity, label: label, callback: (e) =>
            {
                if (e.IsSuccessful)
                {
                    // 下单成功，提示
                    MyAlert("Open", e.ToString());

                    // 外汇平台
                    if (e.Trade != null)
                    {
                        if (e.Trade.Direction == ETradeDirection.Buy)
                            LongTrade_ = e.Trade;
                        else
                            ShortTrade_ = e.Trade;
                    }
                    else // IB之类的通用Postion, 没有trade细单
                    {
                        if (e.Order.Direction == ETradeDirection.Buy)
                            LongOrder_ = e.Order;
                        else
                            ShortOrder_ = e.Order;
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

        // 关闭买卖方向上的头寸
        protected void Close(ETradeDirection dir)
        {
            if (dir == ETradeDirection.Buy)
            {
                if (LongTrade_ != null)
                    CloseTrade(LongTrade_);
                else if (LongOrder_ != null)
                    ClosePositionByOrder(LongOrder_);
            }
            else if (dir == ETradeDirection.Sell)
            {
                if (ShortTrade_ != null)
                    CloseTrade(ShortTrade_);
                else if (ShortOrder_ != null)
                    ClosePositionByOrder(ShortOrder_);
            }
        }

        protected void CloseTrade(ITrade t)
        {
            // 关闭单子
            var ret = CloseTrade(t, callback: (e) =>
            {
                if (e.IsSuccessful)
                {
                    // 成功提示
                    MyAlert("close", e.ToString());

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

        // 关闭由Order产生的Postion
        protected void ClosePositionByOrder(IOrder t)
        {
            IPosition p = TradingAccount.Positions.FirstOrDefault(p => p.Contract == t.Contract);
            if (p == null)
                return;
            var ret = ClosePosition(p, t.FilledQuantity, e =>
            {
                if (e.IsSuccessful)
                {
                    MyAlert("close", e.ToString());

                    if (t.Direction == ETradeDirection.Buy)
                        LongOrder_ = null;
                    else
                        ShortOrder_ = null;
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

        // 提示，弹窗
        protected void MyAlert(string title, string msg) { Alert(title, msg, AlertAction.Popup()); }

        protected string Label => LongName + Id;

        protected IDC Dc_;

        protected ITrade LongTrade_;
        protected IOrder LongOrder_;
        protected bool LongSending_;
        protected bool LongClosing_;

        protected ITrade ShortTrade_;
        protected IOrder ShortOrder_;
        protected bool ShortSending_;
        protected bool ShortClosing_;

        protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
    }
}