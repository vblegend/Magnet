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



        public Boolean TryGetReplaceType(ITypeSymbol typeSymbol,out NameSyntax? nameSyntax)
        {
            if (this.typeResolver.Resolver(typeSymbol, out var newType))
            {
                nameSyntax = this.MakeNameSyntax(newType);
                return true;
            }
            nameSyntax = null;
            return false;
        }




        // 处理变量声明中的类型替换
        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            // 获取当前类型的符号
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                // 使用替换类型
                var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                return node.WithType(newTypeNode);
            }
            return base.VisitVariableDeclaration(node);
        }

        // 处理字段声明中的类型替换
        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Declaration.Type);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                var newTypeNode = replacementType.WithTriviaFrom(node.Declaration.Type);
                return node.WithDeclaration(node.Declaration.WithType(newTypeNode));
            }
            return base.VisitFieldDeclaration(node);
        }

        // 处理属性声明中的类型替换
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                return node.WithType(newTypeNode);
            }
            return base.VisitPropertyDeclaration(node);
        }

        // 处理方法声明中的返回类型替换
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.ReturnType);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                var newReturnType = replacementType.WithTriviaFrom(node.ReturnType);
                return node.WithReturnType(newReturnType);
            }
            return base.VisitMethodDeclaration(node);
        }

        // 处理委托声明中的返回类型替换
        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.ReturnType);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                var newReturnType = replacementType.WithTriviaFrom(node.ReturnType);
                return node.WithReturnType(newReturnType);
            }
            return base.VisitDelegateDeclaration(node);
        }

        // 处理参数声明中的类型替换
        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            if (node.Type != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(node.Type);
                if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
                {
                    var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                    return node.WithType(newTypeNode);
                }
            }
            return base.VisitParameter(node);
        }

        // 处理对象创建表达式中的类型替换
        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                return node.WithType(newTypeNode);
            }
            return base.VisitObjectCreationExpression(node);
        }

        // 处理类型转换表达式中的类型替换
        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                return node.WithType(newTypeNode);
            }
            return base.VisitCastExpression(node);
        }

        // 处理 is 表达式中的类型替换
        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.IsExpression) || node.IsKind(SyntaxKind.AsExpression))
            {
                var rightTypeInfo = semanticModel.GetTypeInfo(node.Right);
                if (rightTypeInfo.Type != null && TryGetReplaceType(rightTypeInfo.Type, out var replacementType))
                {
                    var newRightType = replacementType.WithTriviaFrom(node.Right);
                    return node.WithRight(newRightType);
                }
            }
            return base.VisitBinaryExpression(node);
        }

        // 处理 typeof 表达式中的类型替换
        public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(typeInfo.Type, out var replacementType))
            {
                var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                return node.WithType(newTypeNode);
            }
            return base.VisitTypeOfExpression(node);
        }









        /// <summary>
        /// (Type)value
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        //{
        //    var typeInfo = semanticModel.GetTypeInfo(node);
        //    var typeFullName = this.typeResolver.TypeCast(typeInfo.Type);
        //    if (!String.IsNullOrEmpty(typeFullName))
        //    {
        //        return node.WithType(this.MakeNameSyntax(typeFullName));
        //    }
        //    return base.VisitCastExpression(node);
        //}


        /// <summary>
        /// as  is
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        //public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        //{
        //    if (node.Kind() == SyntaxKind.AsExpression || node.Kind() == SyntaxKind.IsExpression)
        //    {
        //        var typeInfo = semanticModel.GetTypeInfo(node);
        //        String typeFullName = null;
        //        if (node.Kind() == SyntaxKind.AsExpression)
        //        {
        //            typeFullName = this.typeResolver.AsType(typeInfo.Type);
        //        }
        //        else
        //        {
        //            typeFullName = this.typeResolver.IsType(typeInfo.Type);
        //        }
        //        if (!String.IsNullOrEmpty(typeFullName))
        //        {
        //            return node.WithRight(this.MakeNameSyntax(typeFullName));
        //        }
        //    }
        //    return base.VisitBinaryExpression(node);
        //}

        //public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        //{
        //    var typeInfo = semanticModel.GetTypeInfo(node);
        //    var typeFullName = this.typeResolver.TypeCreation(typeInfo.Type);
        //    if (!String.IsNullOrEmpty(typeFullName))
        //    {
        //        return node.WithType(this.MakeNameSyntax(typeFullName));
        //    }
        //    return base.VisitObjectCreationExpression(node);
        //}

        //public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
        //{
        //    // 获取 typeof 内的类型
        //    var typeSyntax = node.Type;
        //    // 使用语义模型获取类型信息
        //    var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
        //    var typeFullName = this.typeResolver.TypeOf(typeInfo.Type);
        //    if (!String.IsNullOrEmpty(typeFullName))
        //    {
        //        return node.WithType(this.MakeNameSyntax(typeFullName));
        //    }
        //    return base.VisitTypeOfExpression(node);
        //}


        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // 获取方法的符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            // 获取方法符号
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            // 判断是否是 Thread.Sleep 方法
            if (methodSymbol != null && methodSymbol.IsStatic)
            {
    
                if (TryGetReplaceType(methodSymbol.ContainingType, out var newTypeSyntax))
                {
                    //构建新的 ThreadProxy.Sleep 调用
                    var newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, newTypeSyntax, SyntaxFactory.IdentifierName(methodSymbol.Name));
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
                if (TryGetReplaceType(memberSymbol.ContainingType, out var newTypeSyntax))
                {
                    return node.WithExpression(newTypeSyntax);
                }
            }
            return base.VisitMemberAccessExpression(node);
        }




        //public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        //{
        //    var symbol = semanticModel.GetDeclaredSymbol(node);
        //    if (symbol.IsStatic)
        //    {
        //        if (VisitAttributes(node.AttributeLists, out var attributeLists))
        //        {
        //            return node.WithAttributeLists(attributeLists);
        //        }
        //    }
        //    return base.VisitMethodDeclaration(node);
        //}




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
                    if (TryGetReplaceType(symbol.ContainingType, out var newTypeSyntax))
                    {
                        count++;
                        attr = attribute.WithName(newTypeSyntax);
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
