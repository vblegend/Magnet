using System;
using System.Collections.Generic;


namespace Magnet.Core
{

    /// <summary>
    /// 
    /// </summary>
    public interface IObjectProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public Type Type {  get; }
        /// <summary>
        /// 
        /// </summary>
        public String SlotName { get; }
        /// <summary>
        /// 
        /// </summary>
        public Object Value { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slotName"></param>
        /// <returns></returns>
        public Boolean TypeIs<T>(String slotName = null);

    }
}
