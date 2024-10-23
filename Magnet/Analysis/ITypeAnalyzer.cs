using System;
 

namespace Magnet.Analysis
{

    /// <summary>
    /// Script type analysis
    /// </summary>
    public interface ITypeAnalyzer: IAnalyzer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        void DefineType(Type type);

    }
}
