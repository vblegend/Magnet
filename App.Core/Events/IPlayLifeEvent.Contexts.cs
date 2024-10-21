using System;


namespace App.Core.Events
{
    public interface IOfflineContext
    {
        String UserId { get; }
        String UserName { get; }
        String PlayerName { get; }
        String PlayerId { get; }
        DateTime Time { get; }
    }

    public interface IOnlineContext
    {
        String UserId { get; }
        String UserName { get; }
        String PlayerName { get; }
        String PlayerId { get; }
        DateTime Time { get; }
    }

}
