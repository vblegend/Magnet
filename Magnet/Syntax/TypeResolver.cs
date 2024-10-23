using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;


namespace Magnet.Syntax
{
    internal sealed class TypeResolver
    {
        private readonly Dictionary<String, String> ReplaceTypes;
        private readonly ITypeRewriter typeRewriter;

        public TypeResolver(ScriptOptions scriptOptions)
        {
            ReplaceTypes = new Dictionary<string, string>(scriptOptions.ReplaceTypes);
            typeRewriter = scriptOptions.typeRewriter;
        }

        public String TypeCast(ITypeSymbol type)
        {
            return cast(e => e.OnTypeCast, type);
        }

        public String IsType(ITypeSymbol type)
        {
            return cast(e => e.OnIsType, type);
        }

        public String AsType(ITypeSymbol type)
        {
            return cast(e => e.OnAsType, type);
        }

        public String TypeCreation(ITypeSymbol type)
        {
            return cast(e => e.OnTypeCreation, type);
        }

        public String TypeOf(ITypeSymbol type)
        {
            return cast(e => e.OnTypeOf, type);
        }

        public String TypeStaticMethodCall( ITypeSymbol typeSymbol, IMethodSymbol methodSymbol)
        {
            var typeName = typeSymbol.ToString();
            if (ReplaceTypes.TryGetValue(typeName, out var value)) return value;
            if (typeRewriter != null)
            {
                value = typeRewriter.OnTypeStaticMethodCall(typeSymbol, methodSymbol);
            }
            if (!String.IsNullOrEmpty(value)) return value;
            return null;
        }

        public String TypeStaticMemberAccess(ITypeSymbol typeSymbol, ISymbol memberSymbol)
        {
            var typeName = typeSymbol.ToString();
            if (ReplaceTypes.TryGetValue(typeName, out var value)) return value;
            if (typeRewriter != null)
            {
                value = typeRewriter.OnTypeStaticMemberAccess(typeSymbol, memberSymbol);
            }
            if (!String.IsNullOrEmpty(value)) return value;
            return null;
        }

        public String MethodAttribute(ITypeSymbol type)
        {
            return cast(e => e.OnMethodAttribute, type);
        }

        private String cast(Func<ITypeRewriter, Func<ITypeSymbol, String>> action, ITypeSymbol type)
        {
            var typeName = type.ToString();
            if (ReplaceTypes.TryGetValue(typeName, out var value)) return value;
            if (typeRewriter != null)
            {
                value = action(typeRewriter)(type);
            }
            if (!String.IsNullOrEmpty(value)) return value;
            return null;
        }


    }
}
