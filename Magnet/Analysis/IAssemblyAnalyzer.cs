using System.Reflection;

namespace Magnet.Analysis
{
    /// <summary>
    /// Assembly type analysis
    /// </summary>
    public interface IAssemblyAnalyzer : IAnalyzer
    {
        /// <summary>
        /// Assembly type loaded
        /// </summary>
        /// <param name="assembly"></param>
        void DefineAssembly(Assembly assembly);
    }
}
