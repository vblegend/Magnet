
using System;


namespace Magnet.Exceptions
{
    /// <summary>
    /// MagnetScript already exists（）
    /// </summary>
    public sealed class ScriptExistException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ScriptExistException(string name)
        {
            Name = name;
        }
    }



}
