using App.Core.Events;
using App.Core;
using Magnet.Core;
using App.Core.Timer;



[Script(nameof(QManage))]
public class QManage : GameScript, IPlayLifeEvent
{


    [Function(null, "玩家上线")]
    public void OnOnline(IOnlineContext ctx)
    {
        PRINT("上线了。。。");
    }



    [Function(null, "玩家下线")]
    public void OnOffline(IOfflineContext ctx)
    {

    }


    [Timer(0, 1, Duration.Second)]
    private void OnTimer_0()
    {
        PRINT($"Timer 0 is Working");
    }


    [Timer(1, 1, Duration.Second)]
    private void OnTimer_1()
    {
        PRINT($"Timer 1 is Working");
    }




}



