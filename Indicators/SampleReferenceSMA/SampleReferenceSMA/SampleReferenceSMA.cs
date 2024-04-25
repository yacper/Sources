using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;

namespace Sparks.Scripts.Custom
{
    [Indicator(Group = "Samples")]
    public class SampleReferenceSMA : Indicator
    {
        #region 用户参数
        [Parameter, Display(Name = "Source")]
        public IDatas Source { get; set; }

        [Parameter, Display(Name = "Periods"), Range(1, int.MaxValue), DefaultValue(7)]
        public int Periods { get; set; }

        [Output, Stroke("#b667c5")]
        public IIndicatorDatas Result { get; set; }

        #endregion

        protected override void OnStart()
        {
            // 创建SMA
            SMA_ = Indicators.CreateIndicator<SMA>(Source, Periods);
        }

        protected override void OnData(ISource source, int index)
        {
            Result[index] = SMA_.Result[index];
        }

        private SMA SMA_;
    }
}