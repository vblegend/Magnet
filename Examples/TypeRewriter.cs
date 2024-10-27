using App.Core.Types;
using Magnet;
using Magnet.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Threading;


namespace ScriptRuner
{





    internal class TypeRewriter : ITypeRewriter
    {
        private String ClearGenericParameters(ITypeSymbol typeFullName)
        {
            return typeFullName.ToDisplayString().Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public bool RewriteType(CSharpSyntaxNode syntaxNode, ITypeSymbol typeSymbol, out Type newType)
        {
            //Console.WriteLine(typeSymbolm.ToDisplayString());
            Console.WriteLine(syntaxNode.Location() + " " + typeSymbol.ToDisplayString());
            var typeFullName = ClearGenericParameters(typeSymbol);

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
