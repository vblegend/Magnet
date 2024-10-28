using App.Core.Timer;
using Magnet.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;



namespace App.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class GameScript : AbstractScript
    {

        [Autowired(typeof(GlobalVariableStore))]
        protected readonly GlobalVariableStore GLOBAL;

        [Autowired("SELF")]
        protected readonly IObjectContext Player;

        [Autowired]
        private readonly ITimerManager timerManager;


        private IStateContext StateContext => (this as IScriptInstance).GetStateContext();

        /// <inheritdoc/>
        protected override void Initialize()
        {
            var timerService = this.StateContext?.GetProvider<ITimerService>();
            timerManager?.SetTimerService(timerService);
        }

        /// <inheritdoc/>
        protected override void Shutdown()
        {

        }



        /// <summary>
        /// clear state all timers
        /// </summary>
#if RELEASE
        [DebuggerHidden]
#endif
        protected void ClearTimers()
        {
            timerManager?.ClearTimer(this);
        }


        /// <summary>
        /// enable a timer
        /// </summary>
        /// <param name="timerIndex"></param>
#if RELEASE
        [DebuggerHidden]
#endif
        protected void EnableTimer(Int32 timerIndex)
        {
            timerManager?.EnableTimer(this, timerIndex);
        }


        /// <summary>
        ///  disable a timer
        /// </summary>
        /// <param name="timerIndex"></param>
#if RELEASE
        [DebuggerHidden]
#endif
        protected void DisableTimer(Int32 timerIndex)
        {
            timerManager?.DisableTimer(this, timerIndex);
        }


#if RELEASE
        [DebuggerHidden]
#endif
        protected IItemBuilder Make(String item)
        {
            return new ItemBuilder(item);
        }

#if RELEASE
        [DebuggerHidden]
#endif
        protected IItemBuilder Make(Int32 itemId)
        {
            return new ItemBuilder(itemId);
        }



#if RELEASE
        [DebuggerHidden]
#endif
        protected void Give(IItemBuilder item)
        {
            if (item is ItemBuilder builder)
            {
                this.Print(builder);
            }
        }




#if RELEASE
        [DebuggerHidden]
#endif
        protected Int32 Random(Int32 maxValue)
        {
            return System.Random.Shared.Next(maxValue);
        }








        #region DEBUG
        [Conditional("DEBUG")]
        public void Debug(Object @object, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string callMethod = null)
        {
            this.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {@object}");
        }


        [Conditional("DEBUG")]
        public void Debug(string message, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string callMethod = null)
        {
            this.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {message}");
        }

        [Conditional("DEBUG")]
        public void Debug(string format, object[] args, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string callMethod = null)
        {
            this.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {String.Format(format, args)}");
        }
        #endregion

        #region PRINT

        [Conditional("USE_PRINT")]
        public void Print(string message)
        {
            this.Output(MessageType.Print, message);
        }


        [Conditional("USE_PRINT")]
        public void Print(Object message)
        {
            this.Output(MessageType.Print, $"{message}");
        }


        [Conditional("USE_PRINT")]
        public void Print(string format, params object[] args)
        {
            this.Output(MessageType.Print, String.Format(format, args));
        }
        #endregion




        #region Assert
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public void Assert(Boolean condition)
        {
            System.Diagnostics.Debug.Assert(condition);
        }

        #endregion



    }
}
