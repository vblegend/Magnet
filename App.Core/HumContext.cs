using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    public class HumContext : IObjectContext
    {
        public long ObjectId => 123456;

        public String Name = "Hello";
    }
}
