
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Magnet.Core
{


    /// <summary>
    /// Base script object, script object needs to inherit this type, provides some basic scripting mechanisms
    /// </summary>
    public abstract class AbstractScript : IScriptInstance
    {

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [NotNull]
        internal ScriptMetaTable MetaTable;

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [NotNull]
        [Autowired]
        private readonly IStateContext _stateContext;



        /// <summary>
        /// Gets the context of the script state
        /// </summary>
        /// <returns></returns>
        [return: NotNull]
        public IStateContext GetStateContext()
        {
            return _stateContext;
        }

        /// <summary>
        /// Enter the debug breakpoint in debug mode
        /// </summary>
        ////
        [DebuggerHidden] // JUMP BREAK CALLER
        [Conditional("USE_DEBUGGER")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void debugger()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            Debugger.Break();
        }

        #region IScriptInstance

        void IScriptInstance.Initialize()
        {
            this.Initialize();
        }

        void IScriptInstance.Shutdown()
        {
            this.Shutdown();
        }
        #endregion



        /// <summary>
        /// Script initialization 
        /// </summary>
        protected virtual void Initialize()
        {

        }

        /// <summary>
        /// Script Being shutdown
        /// </summary>
        protected virtual void Shutdown()
        {

        }


        /// <summary>
        /// Output a message to the output stream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
#if RELEASE
        [DebuggerHidden]
#endif
        public void Output(MessageType type, String message)
        {
            _stateContext.Output.Write(type, message);
        }


    }
}
