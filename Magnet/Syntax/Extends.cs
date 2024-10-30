using Microsoft.CodeAnalysis.CSharp;
using System;


namespace Magnet
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extends
    {
        /// <summary>
        /// get syntax location
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static String Location(this CSharpSyntaxNode node)
        {
            var local = node.GetLocation();
            var pos = local.GetLineSpan();
            return pos.Path + "(" + (pos.StartLinePosition.Line + 1) + "," + (pos.StartLinePosition.Character + 1 + ")");
        }
    }
}
