using Microsoft.CodeAnalysis;
using System;



namespace Magnet.Syntax
{
    /// <summary>
    /// MagnetScript type rewriter
    /// The types in the script are replaced when the script is compiled
    /// </summary>
    public interface ITypeRewriter
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeSymbolm"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        Boolean RewriteType(ITypeSymbol typeSymbolm , out Type newType);


    }
}