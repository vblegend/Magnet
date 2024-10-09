using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRuner
{
    internal class ObjectKilledContext : IKilledContext
    {
        public object Murderer => 123456;

        public DateTime KilledTime =>  DateTime.Now;

        public long ObjectId => 10200402121;
    }
}
