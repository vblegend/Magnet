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
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        string OnTypeCast(ITypeSymbol typeSymbol);

        /// <summary>
        /// Handle object cast expression types
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        string OnIsType(ITypeSymbol typeSymbol);

        /// <summary>
        /// Handle as expression types
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        string OnAsType(ITypeSymbol typeSymbol);

        /// <summary>
        /// Handle new expression types
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        string OnTypeCreation(ITypeSymbol typeSymbol);

        /// <summary>
        /// Handle typeof() expression types
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        string OnTypeOf(ITypeSymbol typeSymbol);

        /// <summary>
        /// Handle static method calls types
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="methodSymbol"></param>
        /// <returns></returns>
        string OnTypeStaticMethodCall(ITypeSymbol typeSymbol, IMethodSymbol methodSymbol);

        /// <summary>
        /// Handle static property access types
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="memberSymbol"></param>
        /// <returns></returns>
        string OnTypeStaticMemberAccess(ITypeSymbol typeSymbol, ISymbol memberSymbol);


        /// <summary>
        /// Handle method attribute define
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        string OnMethodAttribute(ITypeSymbol typeSymbol);

    }
}