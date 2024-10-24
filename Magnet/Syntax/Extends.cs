using Microsoft.CodeAnalysis.CSharp;
using System;


namespace Magnet
{
    internal static class Extends
    {
        public static String Location(this CSharpSyntaxNode node)
        {
            var local = node.GetLocation();
            var pos = local.GetLineSpan();
            if (pos.Path != null)
            {
                // user-visible line and column counts are 1-based, but internally are 0-based.
                return  "(" + pos.Path + "@" + (pos.StartLinePosition.Line + 1) + ":" + (pos.StartLinePosition.Character + 1) + ")";
            }

            return "";
        }



}
}
