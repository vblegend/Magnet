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

        public Boolean Resolver(ITypeSymbol typeSymbol, out String newType)
        {
            var typeName = typeSymbol.ToString();
            if (ReplaceTypes.TryGetValue(typeName, out newType)) return true;
            if (typeRewriter != null && typeRewriter.RewriteType(typeSymbol, out var type))
            {
                newType = type.FullName;
                return true;
            }
            newType = null;
            return false;
        }
    }
}
