using App.Core.Types;
using Magnet.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;


namespace ScriptRuner
{



    internal class TypeRewriter : ITypeRewriter
    {
        
        public bool RewriteType(ITypeSymbol typeSymbolm, out Type newType)
        {
            if (typeSymbolm.ToDisplayString() == "System.Threading.Thread")
            {
                newType = typeof(NewThread);
                return true;
            }

            newType = null;
            return false;
        }
    }

}
