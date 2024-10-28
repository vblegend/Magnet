using System;


namespace Magnet.Core
{
    /// <summary>
    /// The abstract interface of the State context
    /// </summary>
    public interface IStateContext
    {


        /// <summary>
        /// get script instance of the T type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InstanceOfType<T>() where T : AbstractScript;


        /// <summary>
        /// get script instance of the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AbstractScript InstanceOfType(Type type);

        /// <summary>
        /// get script instance of the script name
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public AbstractScript InstanceOfName(String scriptName);

        /// <summary>
        /// Script information output stream
        /// </summary>
        public IOutput Output { get; }

        /// <summary>
        /// Get the state provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public T GetProvider<T>(string providerName = null) where T : class;
    }
}
