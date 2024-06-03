using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
[Strategy(Group = "Samples")]
public class SampleDoubleMa : Strategy
{
#region 用户Paras
    // 快速均线周期参数
    [Parameter, Range(1, 100), DefaultValue(5)]
    public int QuickPeriods { get; set; }

    // 慢速均线周期参数
    [Parameter, Range(20, 150), DefaultValue(20)]
    public int SlowPeriods { get; set; }

    // 每笔交易的数量
    [Parameter, DefaultValue(1)]
    public double Quantity { get; set; }

    // 快速均线输出
    [Output, Stroke("green")]
    public IIndicatorDataSeries QuickMaResult { get; set; }

    // 慢速速均线输出
    [Output, Stroke("blue")]
    public IIndicatorDataSeries SlowMaResult { get; set; }

#endregion

    protected override void OnStart()
    {
        // 参数有效性检查，如果快速均线周期大于等于慢速均线周期，停止策略
        if (QuickPeriods >= SlowPeriods)
        {
            // 输出错误信息
            Error($"Bad Args, QuickPeriods:{QuickPeriods} >= SlowPeriods:{SlowPeriods}");
            // 停止策略
            Stop();
            return;
        }

        IBar b = Bars.FirstOrDefault();
        Bars2_ = GetBars(Contract, ETimeFrame.W1, b.Time.Date);

        // 创建快速均线指标
        QuickMa_ = Indicators.CreateIndicator<SMA>(Bars.Closes, QuickPeriods);

        // 创建慢速均线指标
        SlowMa_ = Indicators.CreateIndicator<SMA>(Bars.Closes, SlowPeriods);
    }

    protected IBars Bars2_;

    protected override void OnData(ISource source, int index)
    {
        // 设置快速均线输出
        QuickMaResult[index] = QuickMa_.Result[index];
        // 设置慢速均线输出
        SlowMaResult[index] = SlowMa_.Result[index];

        // 历史数据没有结束，不执行逻辑
        if (!IsHistoryOver)
            return;

        // 正在下单中，不执行逻辑
        if (OrderSending)
            return;

        // long trade
        LongTrade_ = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Buy && p.Comment == Label);
        // short trade
        ShortTrade_ = TradingAccount.Trades.FirstOrDefault(p => p.Code == Symbol.Code && p.Direction == ETradeDirection.Sell && p.Comment == Label);

        // 快速均线上穿慢速均线
        if (QuickMa_.Result.CrossOver(SlowMa_.Result))
        {
            // 如果有空单，平空单
            if (ShortTrade_ != null)
                CloseTrade(ShortTrade_);
            // 如果没有多单，开多单
            if (LongTrade_ == null)
                ExecuteMarketOrder(Symbol.Contract, ETradeDirection.Buy, Quantity, Label);
        }
        // 快速均线下穿慢速均线
        else if (QuickMa_.Result.CrossDown(SlowMa_.Result))
        {
            // 如果有多单，平多单
            if (LongTrade_ != null)
                CloseTrade(LongTrade_);
            // 如果没有空单，开空单
            if (ShortTrade_ == null)
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
                if (e.Trade != null)
                {
                    // 下单成功，弹窗和邮件提示
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
        // 关闭单子
        var ret = CloseTrade(t, callback: (e) =>
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


    // 提示，弹窗和邮件
    protected void MyAlert(string title, string msg)
    {
        Alert(title, msg, new AlertAction[]
        {
            new PopupAlertAction(), new EmailAlertAction()
        });
    }

    protected string Label => LongName + Id;

    protected SMA QuickMa_;
    protected SMA SlowMa_;

    protected ITrade LongTrade_;
    protected bool   LongSending_;
    protected bool   LongClosing_;
    protected ITrade ShortTrade_;
    protected bool   ShortSending_;
    protected bool   ShortClosing_;

    protected bool OrderSending => LongSending_ | LongClosing_ | ShortClosing_ | ShortSending_;
}
}