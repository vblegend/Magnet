using System;
using System.Reflection;


namespace Magnet.Core
{
    public interface ITypeProcessor: IDisposable
    {
        void ProcessAssembly(Assembly assembly);
        void ProcessScript(Type scriptType);
    }
}
