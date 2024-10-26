using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App.Core.Types
{
    public class NewThread
    {

        public Int32 Priority = 123;


        public static NewThread CurrentThread
        {
            get
            {
                return new NewThread();
            }
        }
    }
}
