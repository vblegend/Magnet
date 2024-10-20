using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Events
{
    public interface IPlayLifeEvent
    {


        void OnOnline();

        void OnOffline();

    }
}
