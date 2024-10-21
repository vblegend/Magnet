
using App.Core.UserInterface;
using Magnet.Core;
using System;



namespace App.Core
{
    public abstract class MyScript : AbstractScript
    {

        [Autowired]
        protected readonly GlobalVariableStore GLOBAL;

        [Autowired("SELF")]
        protected readonly IObjectContext SELF;

        [Autowired()]
        protected readonly ITimerManager TimerManager;




        protected sealed override void UnInitialize()
        {
            CLEAR_TIMER();
        }


#if RELEASE 
        [DebuggerHidden]
#endif
        protected void CLEAR_TIMER()
        {
            var state = (this as IScriptInstance).GetState();
            if (state != null && TimerManager != null)
            {
                TimerManager.ClearTimer(state);
            }
        }

#if RELEASE 
        [DebuggerHidden]
#endif
        protected void ENABLE_TIMER(Int32 timerIndex)
        {
            //Int64 combined = ((Int64)timerIndex << 32) | (uint)intervalSecond;
            //Int32 aRecovered = (Int32)(combined >> 32);
            //UInt32 bRecovered = (UInt32)(combined & 0xFFFFFFFF);
            var state = (this as IScriptInstance).GetState();
            if (state != null && TimerManager != null)
            {
                TimerManager.EnableTimer(state, timerIndex);
            }
        }

#if RELEASE
        [DebuggerHidden]
#endif
        protected void DISABLE_TIMER(Int32 timerIndex)
        {
            var state = (this as IScriptInstance).GetState();
            if (state != null && TimerManager != null)
            {
                TimerManager.DisableTimer(state, timerIndex);
            }
        }


#if RELEASE
        [DebuggerHidden]
#endif
        protected IItemBuilder MAKE(String item)
        {


            return null;
        }

#if RELEASE
        [DebuggerHidden]
#endif
        protected IItemBuilder MAKE(Int32 itemId)
        {


            return null;
        }

#if RELEASE
        [DebuggerHidden]
#endif
        protected void GIVE(IItemBuilder item)
        {

        }




#if RELEASE
        [DebuggerHidden]
#endif
        protected Int32 RANDOM(Int32 maxValue)
        {
            return Random.Shared.Next(maxValue);
        }


    }
}
