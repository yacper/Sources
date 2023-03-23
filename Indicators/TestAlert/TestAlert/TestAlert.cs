using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Neo.Api;
using Neo.Api.Alert;
using Neo.Api.Attributes;
using Neo.Api.MarketData;
using Neo.Api.Providers;
using Neo.Api.Scripts;
using Neo.Api.Symbols;
using Neo.Common.Scripts;
using RLib.Base;
using RLib.Graphics;

namespace Neo.Scripts.Custom
{
[Indicator(Group = "Trends")]
public class TestAlert : Indicator
{
#region 用户参数

#endregion

    protected override void OnStart()
    {
		Alert("test popup", "hello", new List<AlertAction>
        {
            new PopupAlertAction(),
            new EmailAlertAction(),
            new SoundAlertAction(new Uri("Resources/Sounds/alarm.mp3", UriKind.Relative), EAlertSoundDuration.S10)
        });

    }

   

}
}