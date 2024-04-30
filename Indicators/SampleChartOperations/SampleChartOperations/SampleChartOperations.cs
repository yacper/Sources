using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
[Indicator(Group = "Samples", IsOverlay = true)]
public class SampleChartOperations : Indicator
{
    protected override void OnStart()
    {
        Chart.Areas.Added   += (s, e) => Info($"IndicatorAreaAdded");
        Chart.Areas.Removed += (s, e) => Info($"IndicatorAreaRemoved");

        {
            // mouse events
            ChartArea.MouseEnter += (s, e) => Info($"MouseEnter:{e}");
            ChartArea.MouseMove  += (s, e) => Info($"MouseMove:{e}");
            ChartArea.MouseLeave += (s, e) => Info($"MouseLeave:{e}");
            ChartArea.MouseWheel += (s, e) => Info($"MouseWheel:{e}");

            ChartArea.MouseUp   += (s, e) => Info($"MouseUp:{e}");
            ChartArea.MouseDown += (s, e) => Info($"MouseDown:{e}");

            ChartArea.DragStart += (s, e) => Info($"DragStart:{e}");
            ChartArea.Drag      += (s, e) => Info($"Drag:{e}");
            ChartArea.DragEnd   += (s, e) => Info($"DragEnd:{e}");
        }

        {// objects
            ChartArea.Objects.Added   += (s, e) => Info($"ObjectAdded:{e}");
            ChartArea.Objects.Removed += (s, e) => Info($"ObjectRemoved:{e}");
        }

        ChartArea.SelectedObjectsChanged += (s) => Info($"SelectedObjectsChanged:{s.SelectedObjects.Select(p => p.ToString()).Join(',')}");
        ChartArea.HoveringObjectChanged  += (s) => Info($"HoveringObjectChanged:{s.HoveringObject?.ToString()}");
    }

    protected override void OnData(ISource source, int index) { }
}
}