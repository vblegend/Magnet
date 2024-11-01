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
        /// Gets the first script object that implements the specified type
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <returns></returns>
        public T FirstAs<T>() where T : class;

        /// <summary>
        /// Gets the first AbstractScript object that implements the specified type
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <param name="type">AbstractScript or interface</param>
        /// <returns></returns>
        public T FirstAs<T>(Type type) where T : AbstractScript;
        /// <summary>
        /// Gets all script objects that implement the specified type
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <returns></returns>
        public IEnumerable<T> TypeOf<T>() where T : class;

        /// <summary>
        /// Gets the script object with the specified name
        /// </summary>
        /// <typeparam name="T">AbstractScript or interface</typeparam>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public T NameAs<T>(String scriptName) where T : class;


        /// <summary>
        /// Script information output stream
        /// </summary>
        public IOutput Output { get; }
    }
}
