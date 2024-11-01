using Magnet.Core;
using System;
using System.Buffers;



namespace Magnet
{
    /// <summary>
    /// script object dependency injector
    /// </summary>
    internal class DependencyInjector : IDisposable
    {
        private ObjectProvider[] _providers;
        private Int32 _count = 0;

        public DependencyInjector(MagnetScript _engine, StateOptions createStateOptions)
        {
            var length = _engine.Options.Providers.Count + createStateOptions.Providers.Count + 2;
            this._providers = ArrayPool<ObjectProvider>.Shared.Rent(length);
            this._count = _engine.Options.Providers.Count;

            for (var i = 0; i < this._count; i++)
            {
                this._providers[i] = _engine.Options.Providers[i];
            }
            if (createStateOptions.Providers.Count > 0)
            {
                foreach (var provider in createStateOptions.Providers)
                {
                    this.RegisterProviderInternal(provider);
                }
            }
        }

        internal void RegisterProviderInternal(Type valueType, Object value, String slotName = null)
        {
            this.RegisterProviderInternal(new ObjectProvider(null, valueType, value, slotName));
        }

        internal void RegisterProviderInternal(Type targetType, Type valueType, Object value, String slotName = null)
        {
            this.RegisterProviderInternal(new ObjectProvider(targetType, valueType, value, slotName));
        }

        internal void RegisterProviderInternal(ObjectProvider provider)
        {
            if (String.IsNullOrWhiteSpace(provider.SlotName))
            {
                _providers[this._count] = provider;
            }
            else
            {
                Array.Copy(_providers, 0, _providers, 1, this._count);
                _providers[0] = provider;
            }
            this._count++;
        }


        internal void Autowired(AbstractScript instance)
        {
            var metaTable = instance.MetaTable;
            for (int i = 0; i < metaTable.AutowriredTables.Length; i++)
            {
                var field = metaTable.AutowriredTables[i];
                var target = instance;
                if (field.IsStatic)
                {
                    if (field.IsFilled) continue;
                    target = null;
                }
                for (int j = 0; j < this._count; j++)
                {
                    var item = this._providers[j];
                    if ((item.TargetType == null || /* ReferenceEquals(item.TargetType, field.DeclaringType) */  item.TargetType == field.DeclaringType) &&                  // Provider 限定目标类型
                        (field.RequiredType == null || /* ReferenceEquals(item.ValueType, field.RequiredType)  */ item.ValueType == field.RequiredType) &&                 // Autowrired 限定字段类型
                        (field.SlotName == null || field.SlotName == item.SlotName) &&                         // Provider 限定了槽名字
                        (/* ReferenceEquals(item.ValueType, field.FieldType) */  item.ValueType == field.FieldType || field.FieldType.IsAssignableFrom(item.ValueType)))  // 字段类型相同的// 继承的
                    {
                        field.Setter(target, item.Value);
                        if (field.IsStatic) field.IsFilled = true;
                        break;
                    }
                }
            }
        }





        public void Dispose()
        {
            if (this._providers != null)
            {
                ObjectProvider empty = default;
                ArrayPool<ObjectProvider>.Shared.Return(this._providers);
                // 清理数组数据 防止卡GC
                for (int i = 0; i < this._providers.Length; i++) {
                    this._providers[i] = empty;
                }
                this._providers = null;
            }

        }
    }
}
