using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Graphics;
using Sparks.Trader.Api;
using Sparks.Trader.Common;
using Sparks.Trader.Scripts;
using Sparks.Utils;

namespace Sparks.Scripts.Custom
{
public enum ETestAlertType
{
    Popup,
    Audio,
    AudioWithFile,
    Email,
    EmailWithCC,
}

[Indicator(Group = "Samples")]
public class SampleAlert : Strategy
{
#region 用户参数

    [Parameter, DefaultValue(ETestAlertType.Popup)]
    public ETestAlertType TestAlertType { get; set; }

    [Parameter, DefaultValue("Hello world!")]
    public string Message { get; set; }

    [Parameter ]
    public string EmailTo { get; set; }

    [Parameter]
    public List<string> EmailCC { get; set; } = new List<string>();

#endregion

    protected override void OnStart()
    {
        switch (TestAlertType)
        {
            case ETestAlertType.Popup:
                // 默认弹出窗口
                Alert("Sample Alert", Message);
                break;
            case ETestAlertType.Audio:
                // 预制Audio声音类型
                Alert("Sample Alert", Message, AlertAction.Sound(ESoundType.Alert, EAlertSoundDuration.M1));
                break;
            case ETestAlertType.AudioWithFile:
                // 使用自定义声音文件
                Alert("Sample Alert", Message, AlertAction.Sound("Resources/Sounds/info.mp3", EAlertSoundDuration.M1));
                break;

            case ETestAlertType.Email:
                // 发送邮件
                if(!string.IsNullOrWhiteSpace(EmailTo))
                    Alert("Sample Alert", Message, AlertAction.Email(EmailTo));
                else
                {
                    Alert("Sample Alert", "请指定Email");
                    Error("请指定Email");
                }
                break;
            case ETestAlertType.EmailWithCC:
                // 发送邮件与CC
                if(!string.IsNullOrWhiteSpace(EmailTo) && EmailCC.Any())
                    Alert("Sample Alert", Message, AlertAction.Email(EmailTo, EmailCC));
                else
                {
                    Alert("Sample Alert", "请指定Email和EmailCC");
                    Error("请指定Email和EmailCC");
                }
                break;

        }
    }

    protected override void OnData(ISource source, int index) { }
}
}