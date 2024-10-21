namespace App.Core.Events
{


    public interface IPlayLifeEvent
    {

        void OnOnline(IOnlineContext ctx);

        void OnOffline(IOfflineContext ctx);

    }
}
