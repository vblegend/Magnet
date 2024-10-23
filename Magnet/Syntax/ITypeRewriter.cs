using Microsoft.CodeAnalysis;
using System;



namespace Magnet.Syntax
{
    public interface ITypeRewriter
    {
        string OnTypeCast(ITypeSymbol typeSymbol);

        string OnIsType(ITypeSymbol typeSymbol);

        string OnAsType(ITypeSymbol typeSymbol);

        string OnTypeCreation(ITypeSymbol typeSymbol);

        string OnTypeOf(ITypeSymbol typeSymbol);

        string OnTypeStaticMethodCall(ITypeSymbol typeSymbol, IMethodSymbol methodSymbol);

        string OnTypeStaticMemberAccess(ITypeSymbol typeSymbol, ISymbol memberSymbol);

        string OnMethodAttribute(ITypeSymbol typeSymbol);

    }
}