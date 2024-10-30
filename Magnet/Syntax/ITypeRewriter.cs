using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        /// rewrite typs
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="typeSymbol"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        Boolean RewriteType(CSharpSyntaxNode syntaxNode, ITypeSymbol typeSymbol , out Type newType);


    }
}