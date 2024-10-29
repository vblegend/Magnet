

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Magnet.Core
{
    /// <summary>
    /// Abstract interface to a script instance 
    /// </summary>
    public interface IScriptInstance
    {
        /// <summary>
        /// Injected script context
        /// </summary>
        /// <param name="stateContext"></param>
        void InjectedContext(IStateContext stateContext);

        /// <summary>
        /// 实验用的，没啥用
        /// </summary>
        /// <param name="providers"></param>
        void ProviderInject(List<IObjectProvider> providers);



        /// <summary>
        /// all script loaded , initialize
        /// </summary>
        void Initialize();

        /// <summary>
        /// script being destory
        /// </summary>
        void Shutdown();

        /// <summary>
        /// get script state
        /// </summary>
        /// <returns></returns>
        [return: NotNull]
        IStateContext GetStateContext();
    }
}
