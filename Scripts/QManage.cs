using App.Core.Events;
using App.Core;
using Magnet.Core;
using App.Core.Timer;
using App.Core.UserInterface;
using System.Collections.Generic;
using System.Threading;
using System;
struct ABCD<T>
{
    public T Value;
}


[Script(nameof(QManage))]
public class QManage : GameScript, IPlayLifeEvent
{
    [Autowired("A")]
    private readonly ScriptExample scriptExample11;

    [Autowired]
    private readonly IKillContext KilledContext;



    void IPlayLifeEvent.OnOnline(IOnlineContext ctx)
    {
        this.Print("上线了。。。");
        Give(Make("木剑").Quality(5).Count(8).Alias("xxl").Upgrade());
        EnableTimer(0);
    }


    void IPlayLifeEvent.OnOffline(IOfflineContext ctx)
    {
        this.Print("下线了。。。");
        DisableTimer(0);
    }


    [Timer(0, 1, Duration.Second)]
    private void OnTimer_0()
    {
        Print($"Timer 0 is Working");
    }


    [Timer(1, 1, Duration.Second)]
    private void OnTimer_1()
    {
        Print($"Timer 1 is Working");
    }



    /// <summary>
    /// management resources release
    /// </summary>
    protected override void Shutdown()
    {
        ClearTimers();
        base.Shutdown();
    }
}



