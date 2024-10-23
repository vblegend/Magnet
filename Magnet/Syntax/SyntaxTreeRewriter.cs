using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Magnet.Syntax
{
    internal class SyntaxTreeRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel semanticModel;

        private TypeResolver typeResolver;
        public SyntaxTreeRewriter(TypeResolver typeResolver)
        {
            this.typeResolver = typeResolver;
        }

        /// <summary>
        /// (Type)value
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            var typeFullName = this.typeResolver.TypeCast(typeInfo.Type);
            if (!String.IsNullOrEmpty(typeFullName))
            {
                return node.WithType(this.MakeNameSyntax(typeFullName));
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
                String typeFullName = null;
                if (node.Kind() == SyntaxKind.AsExpression)
                {
                    typeFullName = this.typeResolver.AsType(typeInfo.Type);
                }
                else
                {
                    typeFullName = this.typeResolver.IsType(typeInfo.Type);
                }
                if (!String.IsNullOrEmpty(typeFullName))
                {
                    return node.WithRight(this.MakeNameSyntax(typeFullName));
                }
            }
            return base.VisitBinaryExpression(node);
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            var typeFullName = this.typeResolver.TypeCreation(typeInfo.Type);
            if (!String.IsNullOrEmpty(typeFullName))
            {
                return node.WithType(this.MakeNameSyntax(typeFullName));
            }
            return base.VisitObjectCreationExpression(node);
        }

        public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            // 获取 typeof 内的类型
            var typeSyntax = node.Type;
            // 使用语义模型获取类型信息
            var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
            var typeFullName = this.typeResolver.TypeOf(typeInfo.Type);
            if (!String.IsNullOrEmpty(typeFullName))
            {
                return node.WithType(this.MakeNameSyntax(typeFullName));
            }
            return base.VisitTypeOfExpression(node);
        }


        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // 获取方法的符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            // 获取方法符号
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            // 判断是否是 Thread.Sleep 方法
            if (methodSymbol != null && methodSymbol.IsStatic)
            {
                var typeFullName = this.typeResolver.TypeStaticMethodCall(methodSymbol.ContainingType, methodSymbol);
                if (!String.IsNullOrEmpty(typeFullName))
                {
                    //构建新的 ThreadProxy.Sleep 调用
                    var newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, MakeNameSyntax(typeFullName), SyntaxFactory.IdentifierName(methodSymbol.Name));
                    //替换原来的方法调用表达式
                    var newInvocation = node.WithExpression(newExpression);
                    return newInvocation;
                }
            }
            return base.VisitInvocationExpression(node);
        }


        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var memberSymbol = symbolInfo.Symbol as ISymbol;
            if (memberSymbol != null && memberSymbol.IsStatic && (memberSymbol.Kind == SymbolKind.Property || memberSymbol.Kind == SymbolKind.Field))
            {
                var typeFullName = this.typeResolver.TypeStaticMemberAccess(memberSymbol.ContainingType, memberSymbol);
                if (!String.IsNullOrEmpty(typeFullName))
                {
                    return node.WithExpression(MakeNameSyntax(typeFullName));
                }
            }
            return base.VisitMemberAccessExpression(node);
        }




        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol.IsStatic)
            {
                if (VisitAttributes(node.AttributeLists, out var attributeLists))
                {
                    return node.WithAttributeLists(attributeLists);
                }
            }
            return base.VisitMethodDeclaration(node);
        }




        private bool VisitAttributes(SyntaxList<AttributeListSyntax> attributeLists, out SyntaxList<AttributeListSyntax> newList)
        {
            newList = SyntaxFactory.List<AttributeListSyntax>();
            var count = 0;
            // 遍历方法的所有属性
            foreach (var attributeList in attributeLists)
            {
                List<AttributeSyntax> attributeList2 = new List<AttributeSyntax>();
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                    var attr = attribute;
                    var typeFullName = this.typeResolver.MethodAttribute(symbol.ContainingType);
                    if (!String.IsNullOrEmpty(typeFullName))
                    {
                        count++;
                        attr = attribute.WithName(SyntaxFactory.ParseName(typeFullName));
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
            semanticModel = model;
            return base.Visit(root);
        }



    }
}
