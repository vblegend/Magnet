using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    public interface ITimerManager
    {
        void EnableTimer(IStateContext context, Int32 timerIndex);

        void DisableTimer(IStateContext context, Int32 timerIndex);

        void ClearTimer(IStateContext context);



    }
}
