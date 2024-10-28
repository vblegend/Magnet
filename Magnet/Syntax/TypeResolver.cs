using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;


namespace Magnet.Syntax
{
    internal sealed class TypeResolver
    {
        private readonly Dictionary<String, String> ReplaceTypes;
        private readonly ITypeRewriter typeRewriter;
        public readonly Boolean IsCanRewrite;



        public TypeResolver(ScriptOptions scriptOptions)
        {
            ReplaceTypes = new Dictionary<string, string>(scriptOptions.ReplaceTypes);
            typeRewriter = scriptOptions.typeRewriter;
            IsCanRewrite = ReplaceTypes.Count > 0 || typeRewriter != null;
        }

        public Boolean Resolver(CSharpSyntaxNode syntaxNode, ITypeSymbol typeSymbol, out String newType)
        {
            var typeName = typeSymbol.CleanTypeName();
            if (ReplaceTypes.TryGetValue(typeName, out newType)) return true;
            if (typeRewriter != null && typeRewriter.RewriteType(syntaxNode, typeSymbol, out var type))
            {
                newType = type.FullName;
                return true;
            }
            newType = null;
            return false;
        }
    }
}
