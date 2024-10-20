using App.Core.Events;
using App.Core;
using Magnet.Core;


namespace Scripts
{
    [Script(nameof(QManage))]
    public class QManage : MyScript, IPlayLifeEvent
    {


        [Function(null, "玩家上线")]
        public void OnOnline()
        {

        }



        [Function(null, "玩家下线")]
        public void OnOffline()
        {

        }

    }
}
