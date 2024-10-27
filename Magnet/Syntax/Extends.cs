using Microsoft.CodeAnalysis.CSharp;
using System;


namespace Magnet
{
    public static class Extends
    {
        public static String Location(this CSharpSyntaxNode node)
        {
            var local = node.GetLocation();
            var pos = local.GetLineSpan();
            return pos.Path + "(" + (pos.StartLinePosition.Line + 1) + "," + (pos.StartLinePosition.Character + 1 + ")");
        }
    }
}
