using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Magnet
{
    public class ScriptLoadContext : AssemblyLoadContext
    {
        private ScriptOptions Options;
        public ScriptLoadContext(ScriptOptions options) : base(isCollectible: true)
        {
            Options = options;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // 如果需要处理程序集加载，可以在此处自定义逻辑
            if (Options.AssemblyLoad != null)
            {
                return Options.AssemblyLoad(this,assemblyName);
            }
            Console.WriteLine($"Load Assembly: {assemblyName.Name}");
            if (assemblyName.Name == "AllowedAssembly")
            {
                return LoadFromAssemblyPath("path_to_your_allowed_assembly.dll");
            }
            return null;
        }
    }

}
