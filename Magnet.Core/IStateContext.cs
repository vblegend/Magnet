using System;
using System.Collections.Generic;


namespace Magnet.Core
{
    /// <summary>
    /// The abstract interface of the State context
    /// </summary>
    public interface IStateContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <returns></returns>
        public T FirstAs<T>() where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public T FirstAs<T>(Type type) where T : AbstractScript;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <returns></returns>
        public IEnumerable<T> TypeOf<T>() where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public T NameAs<T>(String scriptName) where T : class;

        /// <summary>
        /// Get the state provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public T GetProvider<T>(string providerName = null) where T : class;


        /// <summary>
        /// Script information output stream
        /// </summary>
        public IOutput Output { get; }
    }
}
