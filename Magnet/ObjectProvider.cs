using Magnet.Core;
using System;


namespace Magnet
{
    internal class ObjectProvider
    {
        internal ObjectProvider(Type targetType, Type valueType, Object value, String slotName)
        {
            this.TargetType = targetType;
            this.ValueType = valueType;
            this.Value = value;
            this.SlotName = slotName;
        }

        /// <summary>
        /// 限定字段所属对象类型
        /// </summary>
        public readonly Type TargetType;

        /// <summary>
        /// 注入值的类型
        /// </summary>
        public readonly Type ValueType;

        /// <summary>
        /// 指定槽名称
        /// </summary>
        public readonly String SlotName;
        /// <summary>
        /// 注入值
        /// </summary>
        public readonly Object Value;


    }
}
