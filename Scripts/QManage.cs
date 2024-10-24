﻿using App.Core.Events;
using App.Core;
using Magnet.Core;
using App.Core.Timer;



[Script(nameof(QManage))]
public class QManage : GameScript, IPlayLifeEvent
{


    [Function(null, "玩家上线")]
    public void OnOnline(IOnlineContext ctx)
    {
        Print("上线了。。。");
        Give(Make("木剑").Quality(5).Count(8).Alias("xxl").Upgrade());
    }



    [Function(null, "玩家下线")]
    public void OnOffline(IOfflineContext ctx)
    {

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



