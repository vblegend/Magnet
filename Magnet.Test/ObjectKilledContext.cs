using App.Core;
using App.Core.Events;


namespace Magnet.Test
{
    internal class ObjectKilledContext : IKillContext
    {
        public IMap Map => null;

        public IObjectContext Target => null;
    }
}
