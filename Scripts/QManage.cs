using App.Core.Events;
using App.Core;
using Magnet.Core;
using App.Core.Timer;
using App.Core.UserInterface;
using System.Collections.Generic;
using System.Threading;
using System;



[Script(nameof(QManage))]
public class QManage : GameScript, IPlayLifeEvent
{


    [Function(null, "玩家上线")]
    void IPlayLifeEvent.OnOnline(IOnlineContext ctx)
    {
        Print("上线了。。。");
        Give(Make("木剑").Quality(5).Count(8).Alias("xxl").Upgrade());
    }



    [Function(null, "玩家下线")]
    void IPlayLifeEvent.OnOffline(IOfflineContext ctx)
    {
        (Int32, Boolean) ddd = (123, true);

        var s33 = new List<String>();

        Thread saaaa = new Thread(() => { Print("123"); });
        Thread[] ffff = new Thread[0];

        System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(() => { });
        NewList<App.Core.Types.NewThread> esd2 = new NewList<App.Core.Types.NewThread>();
        esd2.Add(new App.Core.Types.NewThread(null));

        var s = System.Threading.Thread.CurrentThread?.Priority;

        var t = typeof(System.Threading.Thread);
        var n = nameof(System.Threading.Thread);

        UI.DIALOG(120)?.TO(Player);

        UI.DIALOG(120).TO(Player)
          .POSITION(11, 22)
          .TEXT([
              "====[<$name>]====",
              "bbbbbb",
              "cccccc",
              "dddddd"
          ])
          .FLOATING([
              UI.GIF("view", "package://item.wix,12", 0, 0),
              UI.IMAGE("view", "package://item.wix,12", 0, 0),
              UI.BUTTTON("ok", "package://item.wix,13", 15, 100),
              UI.BUTTTON("cancel", "package://item.wix,14", 200, 100)
          ])
          .SEND();

        Debug($"SCRIPT GLOBAL.VAR = {GLOBAL.S[1]}");
        GLOBAL.S[1] = "Hello Wrold";

        List<string> list = [];

        debugger();
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



