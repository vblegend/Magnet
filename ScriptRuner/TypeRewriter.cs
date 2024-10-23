using Magnet.Syntax;
using Microsoft.CodeAnalysis;
using System;


namespace ScriptRuner
{
    internal class TypeRewriter : ITypeRewriter
    {
        public string OnAsType(ITypeSymbol typeSymbol)
        {

            //Console.WriteLine( $"found as {typeSymbol}" );
            return null;
        }


        public string OnIsType(ITypeSymbol typeSymbol)
        {
            //Console.WriteLine($"found is {typeSymbol}");
            return null;
        }


        public string OnMethodAttribute(ITypeSymbol typeSymbol)
        {
            //Console.WriteLine($"found [{typeSymbol}]");
            return null;
        }

        public string OnTypeCast(ITypeSymbol typeSymbol)
        {
            // Console.WriteLine($"found ({typeSymbol})obj");
            return null;
        }

        public string OnTypeCreation(ITypeSymbol typeSymbol)
        {
            //Console.WriteLine($"found new {typeSymbol}()");
            return null;
        }

        public string OnTypeOf(ITypeSymbol typeSymbol)
        {
            //Console.WriteLine($"found typeof({typeSymbol})");

            return null;
        }

        public string OnTypeStaticMemberAccess(ITypeSymbol typeSymbol, ISymbol memberSymbol)
        {
            //Console.WriteLine($"found {typeSymbol}.{memberSymbol}");
            return null;
        }

        public string OnTypeStaticMethodCall(ITypeSymbol typeSymbol, IMethodSymbol methodSymbol)
        {
            //Console.WriteLine($"found {typeSymbol}.{methodSymbol}()");
            return null;
        }
    }

}
