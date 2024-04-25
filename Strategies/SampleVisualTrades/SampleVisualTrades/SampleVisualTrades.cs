using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
    [Strategy(Group = "Custom")]
    public class SampleVisualTrades : Strategy
    {
        #region 用户Paras

        [Parameter, DefaultValue("Hello world!")]
        public string Message { get; set; }

        #endregion

        protected override void OnStart()
        {
			
            Chart.MainArea.DrawHorizontalLine("1", 2400);
			

			Chart.MainArea.DrawVerticalLine("1", new DateTime(2024, 4, 24));

            Chart.MainArea.DrawHorizontalLine("1", 2350);

            Chart.MainArea.DrawVerticalLine("2", new DateTime(2024, 4, 24));

            Chart.MainArea.DrawTrendLine("3", new ChartPoint(new DateTime(2024, 4, 24), 2200), new ChartPoint(new DateTime(2024, 3, 28), 2100));

            Chart.MainArea.DrawAnchoredText("4", new Point(1, 1), new Point(-200, -100) , "test");

            Chart.MainArea.DrawRectangle("5", new ChartPoint(new DateTime(2024, 4, 24), 2200), new ChartPoint(new DateTime(2024, 3, 28), 2100));


            Chart.MainArea.DrawEllipse("6", new ChartPoint(new DateTime(2024, 4, 24), 2200), new ChartPoint(new DateTime(2024, 3, 28), 2100));

            Chart.MainArea.DrawTriangle("7", new ChartPoint(new DateTime(2024, 4, 24), 2200), new ChartPoint(new DateTime(2024, 3, 28), 2100), new ChartPoint(new DateTime(2024, 3, 20), 2000));


            Chart.MainArea.DrawRuler("8", new ChartPoint(new DateTime(2024, 3, 7), 2193), new ChartPoint(new DateTime(2024, 3, 12), 2415), ERulerType.Both);

            Chart.MainArea.DrawFibonacciRetracement("9", new ChartPoint(new DateTime(2024, 3, 27), 2193), new ChartPoint(new DateTime(2024, 4, 5), 2415));
        }

        protected override void OnData(ISource source, int index)
        {

        }
    }
}