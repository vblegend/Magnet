using Magnet.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Magnet
{
    public class ScriptEngine : IDisposable
    {
        private Assembly assembly = null;

        private Dictionary<Type, BaseScript> instances = new Dictionary<Type, BaseScript>();



        internal ScriptEngine(Assembly assembly)
        {
                
        }




        public T GetDelegate<T>(String scriptName,String methodName) where T : Delegate
        {

            // 创建 TestClass 的实例
            ScriptEngine obj = new ScriptEngine(null);

            // 获取对象的类型
            Type type = obj.GetType();

            // 获取方法信息 (MethodInfo)
            MethodInfo methodInfo = type.GetMethod("SayHello");

            // 创建一个 Delegate 并绑定到 obj 对象
            return (T)Delegate.CreateDelegate(typeof(T), obj, methodInfo);
            //return (T)Marshal.GetDelegateForFunctionPointer(addr, typeof(T));
        }




        public void Initialize()
        {

        }

        public void Dispose()
        {

        }



    }
}
