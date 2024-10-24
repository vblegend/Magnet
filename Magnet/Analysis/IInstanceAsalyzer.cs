using Magnet.Core;


namespace Magnet.Analysis
{
    /// <summary>
    /// Script instance object analysis
    /// </summary>
    public interface IInstanceAsalyzer : IAnalyzer
    {
        /// <summary>
        /// Script instance create
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="script"></param>
        /// <param name="context"></param>
        void DefineInstance(ScriptMetadata metadata , AbstractScript script, IStateContext context);
    }
}
