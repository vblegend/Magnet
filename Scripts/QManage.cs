using App.Core.Events;
using App.Core;
using Magnet.Core;



[Script(nameof(QManage))]
public class QManage : MyScript, IPlayLifeEvent
{


    [Function(null, "玩家上线")]
    public void OnOnline(IOnlineContext ctx)
    {

    }



    [Function(null, "玩家下线")]
    public void OnOffline(IOfflineContext ctx)
    {

    }


    [Timer(10, 5, Duration.Second)]
    private void OnTimer_0()
    {
        this.PRINT($"Timer 0 is Working");
    }


    [Timer(20, 10, Duration.Second)]
    private void OnTimer_1()
    {
        this.PRINT($"Timer 1 is Working");
    }



}



