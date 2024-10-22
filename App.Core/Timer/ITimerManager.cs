﻿using Magnet.Core;
using System;

namespace App.Core.Timer
{
    public interface ITimerManager
    {
        void SetTimerService(TimerService? timerService);
        void EnableTimer(IScriptInstance context, int timerIndex);
        void DisableTimer(IScriptInstance context, int timerIndex);
        void ClearTimer(IScriptInstance context);
    }
}
