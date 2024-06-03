// 可以通过nuget引入Atypical.TechnicalAnalysis.Candles， 它实现了TALib的Candels，注意，要加上预览版
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;
using TechnicalAnalysis.Candles;


namespace Sparks.Scripts.Custom
{
[Indicator(Group = "Custom")]
public class SampleReferenceTalibCandel : Indicator
{
    public static Color BearishColor = Color.Parse("#f23645");
    public static Stroke BearishStroke = new Stroke(BearishColor);
    public static Fill BearishFill = new Fill(BearishColor.WithAlpha(0.8f));

    public static Color BullishColor = Color.Parse("#089981");
    public static Stroke BullishStroke = new Stroke(BullishColor);
    public static Fill BullishFill = new Fill(BullishColor.WithAlpha(0.8f));

    public static Color NormalColor = Color.Parse("#888888");
    public static Stroke NormalStroke = new Stroke(NormalColor);
    public static Fill NormalFill = new Fill(NormalColor.WithAlpha(0.8f));


#region 用户参数

    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowDoji { get; set; }

        /// <summary>
        /// normal
        /// </summary>
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec Doji { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.Diamond,
        Content    = nameof(Doji),
        Location   = ELocation.BelowBar,
        ShowText   = true,
        Stroke     = NormalStroke,
        Fill       = NormalFill,
        FontSpec   = new FontSpec() { Color = NormalColor }
    };


    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowEveningDoji { get; set; }

        /// <summary>
        /// bearish
        /// </summary>
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec EveningDoji { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.ArrowDown,
        Content    = nameof(EveningDoji),
        Location   = ELocation.AboveBar,
        ShowText   = true,
        Stroke     = BearishStroke,
        Fill       = BearishFill,
        FontSpec   = new FontSpec() { Color = BearishColor }
    };

    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowShootingStar { get; set; }

        /// <summary>
        /// bearish
        /// </summary>
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec ShootingStar { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.ArrowDown,
        Content    = nameof(ShootingStar),
        Location   = ELocation.AboveBar,
        ShowText   = true,
        Stroke     = BearishStroke,
        Fill       = BearishFill,
        FontSpec   = new FontSpec() { Color = BearishColor }
    };

    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowMorningStar { get; set; }

        /// <summary>
        /// bullish
        /// </summary>
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec MorningStar { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.ArrowUp,
        Content    = nameof(MorningStar),
        Location   = ELocation.BelowBar,
        ShowText   = true,
        Stroke     = BullishStroke,
        Fill       = BullishFill,
        FontSpec   = new FontSpec() { Color = BullishColor }
    };

    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowHammer { get; set; }

        /// <summary>
        /// normal
        /// </summary>
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec Hammer { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.Diamond,
        Content    = nameof(Hammer),
        Location   = ELocation.BelowBar,
        ShowText   = true,
        Stroke     = NormalStroke,
        Fill       = NormalFill,
        FontSpec   = new FontSpec() { Color = NormalColor }
    };

    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowInvertedHammer { get; set; }

        /// <summary>
        /// normal
        /// </summary>
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec InvertedHammer { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.Diamond,
        Content    = "Inverted\nHammer",
        Location   = ELocation.BelowBar,
        ShowText   = true,
        Stroke     = NormalStroke,
        Fill       = NormalFill,
        FontSpec   = new FontSpec() { Color = NormalColor }
    };

    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowHarami { get; set; }

    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec BullishHarami { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.ArrowUp,
        Content    = "Harami",
        Location   = ELocation.BelowBar,
        ShowText   = true,
        Stroke     = BullishStroke,
        Fill       = BullishFill,
        FontSpec   = new FontSpec() { Color = BullishColor }
    };

    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec BearishHarami { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.ArrowDown,
        Content    = "Harami",
        Location   = ELocation.AboveBar,
        ShowText   = true,
        Stroke     = BearishStroke,
        Fill       = BearishFill,
        FontSpec   = new FontSpec() { Color = BearishColor }
    };


    [Parameter, Display(GroupName = "Inputs"), DefaultValue(true)]
    public bool ShowENGULFING { get; set; }

        /// <summary>
        /// bullish
        /// </summary>
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec ENGULFINGBullish { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.ArrowUp,
        Content    = "ENGULFING",
        Location   = ELocation.BelowBar,
        ShowText   = true,
        Stroke     = BullishStroke,
        Fill       = BullishFill,
        FontSpec   = new FontSpec() { Color = BullishColor }
    };
    [Parameter, Display(GroupName = "Inputs")]
    public MarkerSpec ENGULFINGBearish { get; set; } = new MarkerSpec()
    {
        MarkerType = EMarker.ArrowDown,
        Content    = "ENGULFING",
        Location   = ELocation.AboveBar,
        ShowText   = true,
        Stroke     = BearishStroke,
        Fill       = BearishFill,
        FontSpec   = new FontSpec() { Color = BearishColor }
    };



#endregion

    protected override void OnStart() { }

    protected override void OnData(ISource source, int index)
    {
        IBar b = source[index] as IBar;

        double[] opens  = Bars.Opens.ToArray();
        double[] highs  = Bars.Highs.ToArray();
        double[] lows   = Bars.Lows.ToArray();
        double[] closes = Bars.Closes.ToArray();

        if (ShowENGULFING)
        {
            var result = TACandle.CdlEngulfing(index, index, opens, highs, lows, closes);
            if (result.NBElement != 0)
            {
                if (result.Integers[0] == 100)
                {
                    var spec   = ENGULFINGBullish;
                    Area.DrawMarker($"{nameof(ENGULFINGBullish)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                    spec.Content, spec.ShowText,
                                    spec.FontSpec, spec.Size);
                }
                else if (result.Integers[0] == -100)
                {
                    var spec = ENGULFINGBearish;
                    Area.DrawMarker($"{nameof(ENGULFINGBearish)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                    spec.Content, spec.ShowText,
                                    spec.FontSpec, spec.Size);
                }

            }
        }

        if (ShowHarami)
        {
            {
                var result = TACandle.CdlHarami(0, index, opens, highs, lows, closes);
                //var result = TACandle.CdlHarami(index, index, opens, highs, lows, closes);
                if (result.NBElement != 0)
                {
                    if (result.Integers[0] == 100)
                    {
                        var spec = BullishHarami;
                        Area.DrawMarker($"{nameof(BullishHarami)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                    else if (result.Integers[0] == -100)
                    {
                        var spec = BearishHarami;
                        Area.DrawMarker($"{nameof(BearishHarami)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                }
            }
            {
                var result = TACandle.CdlHaramiCross(index, index, opens, highs, lows, closes);
                if (result.NBElement != 0)
                {
                    if (result.Integers[0] == 100)
                    {
                        var spec = BullishHarami;
                        Area.DrawMarker($"BullishHaramiCross{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                    else if (result.Integers[0] == -100)
                    {
                        var spec = BearishHarami;
                        Area.DrawMarker($"BearishHaramiCross{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                }
            }
        }

        if (ShowDoji)
        {
            var spec   = Doji;
            var result = TACandle.CdlDoji(index, index, opens, highs, lows, closes);
            if (result.NBElement != 0)
            {
                if (result.Integers[0] == 100)
                {
                    Area.DrawMarker($"{nameof(Doji)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                    spec.Content, spec.ShowText,
                                    spec.FontSpec, spec.Size);
                }
            }
        }


        if (ShowEveningDoji)
        {
            var spec   = EveningDoji;
            {
                var result = TACandle.CdlEveningDojiStar(0, index, opens, highs, closes, closes);
                //var result = TACandle.CdlEveningDojiStar(index, index, opens, highs, lows, closes);
                if (result.NBElement != 0)
                {
                    if (result.Integers[0] == 100)
                    {
                        Area.DrawMarker($"{nameof(EveningDoji)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                }
            }
            {
                var result = TACandle.CdlEveningStar(0, index, opens, highs, closes, closes);
                //var result = TACandle.CdlEveningDojiStar(index, index, opens, highs, lows, closes);
                if (result.NBElement != 0)
                {
                    if (result.Integers[0] == 100)
                    {
                        Area.DrawMarker($"{nameof(EveningDoji)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                }
            }
        }

        if (ShowShootingStar)
        {
            var spec   = ShootingStar;
            var result = TACandle.CdlShootingStar(0, index, opens, highs, closes, closes);
            //var result = TACandle.CdlShootingStar(index, index, opens, highs, lows, closes);
            for (int i = 0; i < index; i++)
            {
                if (result.Integers[i] == -100)
                {
                    IBar bb = source[i + result.BegIdx] as IBar;
                    Area.DrawMarker($"{nameof(ShootingStar)}{i + result.BegIdx}", new ChartPoint(bb.Time, bb.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                    spec.Content, spec.ShowText,
                                    spec.FontSpec, spec.Size);
                }
            }
 

            //if (result.NBElement != 0)
            //{
            //    if (result.Integers[0] == -100)
            //    {
            //        Area.DrawMarker($"{nameof(ShootingStar)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
            //                        spec.Content, spec.ShowText,
            //                        spec.FontSpec, spec.Size);
            //    }
            //}

        }

        if (ShowMorningStar)
        {
            var spec = MorningStar;
            {
                var result = TACandle.CdlMorningStar(0, index, opens, highs, closes, closes, 0);
                //var result = TACandle.CdlMorningDojiStar(index, index, opens, highs, lows, closes);

                for (int i = 0; i < index; i++)
                {
                    if (result.Integers[i] == 100)
                    {
                        IBar bb = source[i + result.BegIdx] as IBar;
                        Area.DrawMarker($"{nameof(MorningStar)}{i + result.BegIdx}", new ChartPoint(bb.Time, bb.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                }
            }
            {
                var result = TACandle.CdlMorningDojiStar(0, index, opens, highs, closes, closes);
//var result = TACandle.CdlMorningDojiStar(index, index, opens, highs, lows, closes);

                for (int i = 0; i < index; i++)
                {
                    if (result.Integers[i] == 100)
                    {
                        IBar bb = source[i + result.BegIdx] as IBar;
                        Area.DrawMarker($"MorningDojiStar{i + result.BegIdx}", new ChartPoint(bb.Time, bb.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                        spec.Content, spec.ShowText,
                                        spec.FontSpec, spec.Size);
                    }
                }
            }
            //if (result.NBElement != 0)
            //{
            //    if (result.Integers[0] == -100)
            //    {
            //        Area.DrawMarker($"{nameof(MorningStar)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
            //                        spec.Content, spec.ShowText,
            //                        spec.FontSpec, spec.Size);
            //    }
            //}
       }


        if (ShowHammer)
        {
            var spec   = Hammer;
            //var result = TACandle.CdlHammer(0, index, opens, highs, lows, closes);
            var result = TACandle.CdlHammer(index, index, opens, highs, lows, closes);
            if (result.NBElement != 0)
            {
                if (result.Integers[0] == 100)
                {
                    Area.DrawMarker($"{nameof(Hammer)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                    spec.Content, spec.ShowText,
                                    spec.FontSpec, spec.Size);
                }
            }
        }

        if (ShowInvertedHammer)
        {
            var spec   = InvertedHammer;
            var result = TACandle.CdlInvertedHammer(0, index, opens, highs, lows, closes);
            //var result = TACandle.CdlHammer(index, index, opens, highs, lows, closes);
            if (result.NBElement != 0)
            {
                if (result.Integers[0] == 100)
                {
                    Area.DrawMarker($"{nameof(InvertedHammer)}{index}", new ChartPoint(b.Time, b.Low), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill,
                                    spec.Content, spec.ShowText,
                                    spec.FontSpec, spec.Size);
                }
            }
        }

        //if (ShowBullishHarami | ShowBearishHarami)
        //{
        //    var result = TACandle.CdlHarami(0, index, opens, highs, lows, closes);
        //    for (int i = 0; i < index; i++)
        //    {
        //        if (ShowBullishHarami && result.Integers[i] == 100)
        //        {
        //            var spec = BullishHarami;
        //            Area.DrawMarker($"BullishHarami{i}", new ChartPoint(Bars[i].Time, Bars[i].High), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill, spec.Content,
        //                            spec.ShowText,
        //                            spec.FontSpec, spec.Size);
        //        }
        //        else if (ShowBearishHarami && result.Integers[i] == -100)
        //        {
        //            var spec = BearishHarami;
        //            Area.DrawMarker($"BearishHarami{i}", new ChartPoint(Bars[i].Time, Bars[i].High), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill, spec.Content,
        //                            spec.ShowText,
        //                            spec.FontSpec, spec.Size);
        //        }
        //    }

        //    result = TACandle.CdlHaramiCross(0, index, opens, highs, lows, closes);
        //    for (int i = 0; i < index; i++)
        //    {
        //        if (ShowBullishHarami && result.Integers[i] == 100)
        //        {
        //            var spec = BullishHarami;
        //            Area.DrawMarker($"BullishHarami{i}", new ChartPoint(Bars[i].Time, Bars[i].High), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill, spec.Content,
        //                            spec.ShowText,
        //                            spec.FontSpec, spec.Size);
        //        }
        //        else if (ShowBearishHarami && result.Integers[i] == -100)
        //        {
        //            var spec = BearishHarami;
        //            Area.DrawMarker($"BearishHarami{i}", new ChartPoint(Bars[i].Time, Bars[i].High), spec.Location, spec.MarkerType, spec.Char, spec.Stroke, spec.Fill, spec.Content,
        //                            spec.ShowText,
        //                            spec.FontSpec, spec.Size);
        //        }
        //    }
        //}
    }
}
}