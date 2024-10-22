
using App.Core.Timer;
using App.Core.UserInterface;
using Magnet.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;



namespace App.Core
{
    public abstract class GameScript : AbstractScript
    {

        [Autowired]
        protected readonly GlobalVariableStore GLOBAL;

        [Autowired("SELF")]
        protected readonly IObjectContext Player;

        [Autowired]
        private readonly ITimerManager timerManager;


        private IStateContext StateContext => (this as IScriptInstance).GetStateContext();


        protected override void Initialize()
        {
            var timerService = this.StateContext?.GetProvider<TimerService>();
            timerManager?.SetTimerService(timerService);
        }



        protected sealed override void UnInitialize()
        {
            CLEAR_TIMER();
        }


#if RELEASE 
        [DebuggerHidden]
#endif
        protected void CLEAR_TIMER()
        {
            timerManager?.ClearTimer(this);
        }

#if RELEASE 
        [DebuggerHidden]
#endif
        protected void ENABLE_TIMER(Int32 timerIndex)
        {
            //Int64 combined = ((Int64)timerIndex << 32) | (uint)intervalSecond;
            //Int32 aRecovered = (Int32)(combined >> 32);
            //UInt32 bRecovered = (UInt32)(combined & 0xFFFFFFFF);
            timerManager?.EnableTimer(this, timerIndex);
        }

#if RELEASE
        [DebuggerHidden]
#endif
        protected void DISABLE_TIMER(Int32 timerIndex)
        {
            timerManager?.DisableTimer(this, timerIndex);
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








        #region DEBUG
        [Conditional("DEBUG")]
        public void Debug(Object @object, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string? callMethod = null)
        {
            this.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {@object}");
        }


        [Conditional("DEBUG")]
        public void Debug(string message, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string? callMethod = null)
        {
            this.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {message}");
        }

        [Conditional("DEBUG")]
        public void Debug(string format, object[] args, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string? callMethod = null)
        {
            this.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {String.Format(format, args)}");
        }
        #endregion

        #region PRINT
        public void Print(string message)
        {
            this.Output(MessageType.Print, message);
        }

        public void Print(Object message)
        {
            this.Output(MessageType.Print, $"{message}");
        }

        public void Print(string format, params object?[] args)
        {
            this.Print(message: String.Format(format, args));
        }
        #endregion




        #region Assert

        [DebuggerHidden]
        public void Assert(Boolean condition)
        {
            System.Diagnostics.Debug.Assert(condition);
        }

        #endregion



    }
}
