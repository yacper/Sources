using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
    [Indicator(Group = "Custom")]
    public class SampleVisualTrades : Indicator
    {
        protected override void OnStart()
        {
            if(TradingAccount == null)
                return;

            DrawTrades();

            TradingAccount.Trades.Opened += (s, e) => DrawTrades();
            TradingAccount.Trades.Closed += (s, e) => DrawTrades();
        }

        protected void DrawTrade(int index, ITrade trade)
        {
            var width = 10;
            var height = 24;
            var anchor = new Point(1, 1);
            var offset = new Point(-width, -height * (index + 1));

            var o = Chart.MainArea.DrawAnchoredText($"{index}", anchor, offset, trade.ToString(), null, HorizontalAlignment.Right);
            Trades_.Add(o);
        }

        protected void DrawTrades()
        {
            ClearTrades();

            if (TradingAccount != null)
                for (int i = 0; i < TradingAccount.Trades.Count; i++)
                    DrawTrade(i, TradingAccount.Trades[i]);
        }
        protected void ClearTrades()
        {
            Trades_.ForEach(t => Chart.MainArea.RemoveObject(t));
            Trades_.Clear();
        }

        protected override void OnData(ISource source, int index)
        {
        }

        List<IAnchoredText> Trades_ = new List<IAnchoredText>();
    }
}