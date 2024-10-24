using App.Core;
using App.Core.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRuner
{
    internal class ObjectKilledContext : IKillContext
    {
        public IMap Map => null;

        public IObjectContext Target => null;
    }
}
