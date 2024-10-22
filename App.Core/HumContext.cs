using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    public class HumContext : IObjectContext
    {
        public SnowflakeId ObjectId => 123456;

        public Point Position =>  new Point(333,333);

        public String Name = "Hello";
    }
}
