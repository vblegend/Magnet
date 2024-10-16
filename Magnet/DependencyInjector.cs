using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Magnet
{
    internal class DependencyInjector
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public void Inject(object instance)
        {
            var fields = instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttribute<AutowiredAttribute>() != null)
                .ToArray();

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;

                // 尝试从服务字典中获取服务
                if (_services.TryGetValue(fieldType, out var service))
                {
                    field.SetValue(instance, service);
                }
                else
                {
                    // 处理子类和接口注入
                    var serviceType = _services.Keys.FirstOrDefault(k => fieldType.IsAssignableFrom(k));
                    if (serviceType != null)
                    {
                        field.SetValue(instance, _services[serviceType]);
                    }
                }
            }
        }
    }
}
