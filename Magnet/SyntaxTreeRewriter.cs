using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Magnet
{
    internal class SyntaxTreeRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel semanticModel;

        private Dictionary<String, String> ReplaceTypes;
        public SyntaxTreeRewriter(Dictionary<String, String> ReplaceTypes)
        {
            this.ReplaceTypes = ReplaceTypes;
        }

        /// <summary>
        /// (Type)value
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            if (this.ReplaceTypes.TryGetValue(typeInfo.Type?.ToString(), out var newTypeName))
            {
                return node.WithType(this.MakeNameSyntax(newTypeName));
            }
            return base.VisitCastExpression(node);
        }


        /// <summary>
        /// as  is
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.Kind() == SyntaxKind.AsExpression || node.Kind() == SyntaxKind.IsExpression)
            {
                var typeInfo = semanticModel.GetTypeInfo(node);
                if (this.ReplaceTypes.TryGetValue(typeInfo.Type?.ToString(), out var newTypeName))
                {
                    return node.WithRight(this.MakeNameSyntax(newTypeName));
                }
            }
            return base.VisitBinaryExpression(node);
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            if (this.ReplaceTypes.TryGetValue(typeInfo.Type?.ToString(), out var newTypeName))
            {
                return node.WithType(MakeNameSyntax(newTypeName));
            }
            return base.VisitObjectCreationExpression(node);
        }

        public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            // 获取 typeof 内的类型
            var typeSyntax = node.Type;
            // 使用语义模型获取类型信息
            var typeInfo = this.semanticModel.GetTypeInfo(typeSyntax);

            if (this.ReplaceTypes.TryGetValue(typeInfo.ConvertedType.ToString(), out var newTypeName))
            {
                return node.WithType(this.MakeNameSyntax(newTypeName));
            }
            return base.VisitTypeOfExpression(node);
        }


        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // 获取方法的符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            // 获取方法符号
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            // 判断是否是 Thread.Sleep 方法
            if (methodSymbol != null && methodSymbol.IsStatic)
            {
                if (this.ReplaceTypes.TryGetValue(methodSymbol.ContainingType?.ToString(), out var newTypeName))
                {
                    //构建新的 ThreadProxy.Sleep 调用
                    var newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, MakeNameSyntax(newTypeName), SyntaxFactory.IdentifierName(methodSymbol.Name));
                    //替换原来的方法调用表达式
                    var newInvocation = node.WithExpression(newExpression);
                    return newInvocation;
                }
            }
            return base.VisitInvocationExpression(node);
        }


        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // 获取属性或字段的符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var memberSymbol = symbolInfo.Symbol as ISymbol;

            if (memberSymbol != null && memberSymbol.IsStatic && (memberSymbol.Kind == SymbolKind.Property || memberSymbol.Kind == SymbolKind.Field))
            {
                if (this.ReplaceTypes.TryGetValue(memberSymbol.ContainingType?.ToString(), out var newTypeName))
                {
                    //var newExpression = SyntaxFactory.MemberAccessExpression(
                    //    SyntaxKind.SimpleMemberAccessExpression,
                    //    MakeNameSyntax("Magnet.Safety.ThreadProxy"),
                    //    SyntaxFactory.IdentifierName(memberSymbol.Name)
                    //);
                    return node.WithExpression(MakeNameSyntax(newTypeName));
                }
            }
            return base.VisitMemberAccessExpression(node);
        }




        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var symbol = this.semanticModel.GetDeclaredSymbol(node);
            if (symbol.IsStatic)
            {
                if (this.VisitAttributes(node.AttributeLists, out var attributeLists))
                {
                    return node.WithAttributeLists(attributeLists);
                }
            }
            return base.VisitMethodDeclaration(node);
        }




        private Boolean VisitAttributes(SyntaxList<AttributeListSyntax> attributeLists, out SyntaxList<AttributeListSyntax> newList)
        {
            newList = SyntaxFactory.List<AttributeListSyntax>();
            var count = 0;
            // 遍历方法的所有属性
            foreach (var attributeList in attributeLists)
            {
                List<AttributeSyntax> attributeList2 = new List<AttributeSyntax>();
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbol = this.semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                    var attr = attribute;
                    if (this.ReplaceTypes.TryGetValue(symbol.ContainingType?.ToString(), out var newTypeName))
                    {
                        count++;
                        attr = attribute.WithName(SyntaxFactory.ParseName(newTypeName));
                    }
                    attributeList2.Add(attr);
                }
                var ss = attributeList.WithAttributes(SyntaxFactory.SeparatedList(attributeList2));
                newList = newList.Add(ss);
            }
            return count > 0;
        }


        private NameSyntax MakeNameSyntax(string fullName)
        {
            var names = fullName.Split('.');
            NameSyntax nameSyntax = SyntaxFactory.IdentifierName(names[0]);

            for (var i = 1; i < names.Length; i++)
            {
                if (string.IsNullOrEmpty(names[i]))
                    continue;

                nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, SyntaxFactory.IdentifierName(names[i]));
            }
            return nameSyntax;
        }


        public SyntaxNode VisitWith(SemanticModel model, SyntaxNode root)
        {
            this.semanticModel = model;
            return base.Visit(root);
        }



    }
}
