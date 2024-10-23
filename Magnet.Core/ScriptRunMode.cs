

namespace Magnet.Core
{

    /// <summary>
    /// The compilation mode of the current script
    /// </summary>
    public enum ScriptRunMode
    {
        /// <summary>
        /// Debug mode, not optimized code, can load debugger debugging
        /// </summary>
        Debug = 0,

        /// <summary>
        /// Production mode, optimize code, remove debug file information
        /// </summary>
        Release = 1,
    }
}
