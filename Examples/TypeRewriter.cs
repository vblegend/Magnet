using App.Core.Types;
using Magnet;
using Magnet.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace ScriptRuner
{





    internal class TypeRewriter : ITypeRewriter
    {
        public bool RewriteType(CSharpSyntaxNode syntaxNode, ITypeSymbol typeSymbol, out Type newType)
        {
            var typeFullName = typeSymbol.CleanTypeName();

            if (typeFullName == "ScriptA.ABCD")
            {
                newType = typeof(NewList<>);
                return true;
            }

            if (typeFullName == "System.Threading.Thread")
            {
                newType = typeof(NewThread);
                return true;
            }

            newType = null;
            return false;
        }
    }

}
