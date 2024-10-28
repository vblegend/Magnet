using Magnet.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;


namespace Magnet.Syntax
{
    internal class SyntaxTreeRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;

        private TypeResolver typeResolver;
        public SyntaxTreeRewriter(TypeResolver typeResolver)
        {
            this.typeResolver = typeResolver;
        }

        public Boolean TryGetReplaceType(CSharpSyntaxNode syntaxNode, ITypeSymbol typeSymbol, out NameSyntax? nameSyntax)
        {
            if (this.typeResolver.Resolver(syntaxNode, typeSymbol, out var newType))
            {
                var noParametersType = newType.Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
                nameSyntax = this.MakeNameSyntax(noParametersType);
                return true;
            }
            nameSyntax = null;
            return false;
        }



        // 替换元类型
        public override SyntaxNode VisitTupleType(TupleTypeSyntax node)
        {
            SeparatedSyntaxList<TupleElementSyntax> eles = new SeparatedSyntaxList<TupleElementSyntax>();
            Boolean changed = false;
            for (int i = 0; i < node.Elements.Count; i++)
            {
                var element = node.Elements[i];
                var typeInfo = _semanticModel.GetTypeInfo(element.Type);
                if (typeInfo.Type != null && TryGetReplaceType(element, typeInfo.Type, out var replacementType))
                {
                    var newTypeNode = replacementType.WithTriviaFrom(element.Type);
                    eles.Add(element.WithType(newTypeNode));
                    changed = true;
                }
                else
                {
                    eles.Add(element);
                }
            }
            if (changed) return node.WithElements(eles);
            return base.VisitTupleType(node);
        }



        // 替换指针类型
        public override SyntaxNode VisitPointerType(PointerTypeSyntax node)
        {
            // 获取当前类型的符号
            var typeInfo = _semanticModel.GetTypeInfo(node.ElementType);
            if (typeInfo.Type != null && TryGetReplaceType(node.ElementType, typeInfo.Type, out var replacementType))
            {
                // 使用替换类型
                var newTypeNode = replacementType.WithTriviaFrom(node.ElementType);
                return node.WithElementType(newTypeNode);
            }
            return base.VisitPointerType(node);
        }

        // 替换数组类型
        public override SyntaxNode VisitArrayType(ArrayTypeSyntax node)
        {
            // 获取当前类型的符号
            var typeInfo = _semanticModel.GetTypeInfo(node.ElementType);
            if (typeInfo.Type != null && TryGetReplaceType(node.ElementType, typeInfo.Type, out var replacementType))
            {
                // 使用替换类型
                var newTypeNode = replacementType.WithTriviaFrom(node.ElementType);
                return node.WithElementType(newTypeNode);
            }
            return base.VisitArrayType(node);
        }



        // 处理字段声明中的类型替换
        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            //var newNode = base.VisitFieldDeclaration(node) as FieldDeclarationSyntax;
            //var typeInfo = semanticModel.GetTypeInfo(node.Declaration.Type);
            //if (typeInfo.Type != null && TryGetReplaceType(node.Declaration.Type, typeInfo.Type, out var replacementType))
            //{
            //    var newTypeNode = replacementType.WithTriviaFrom(node.Declaration.Type);
            //    return newNode.WithDeclaration(node.Declaration.WithType(newTypeNode));
            //}
            return base.VisitFieldDeclaration(node);
        }

        // 处理属性声明中的类型替换
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var newNode = base.VisitPropertyDeclaration(node) as PropertyDeclarationSyntax;
            var typeInfo = _semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(node.Type, typeInfo.Type, out var replacementType))
            {
                var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                return newNode.WithType(newTypeNode);
            }
            return newNode;
        }

        // 处理方法声明中的返回类型替换
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var typeInfo = _semanticModel.GetTypeInfo(node.ReturnType);
            if (typeInfo.Type != null && TryGetReplaceType(node.ReturnType, typeInfo.Type, out var replacementType))
            {
                var newReturnType = replacementType.WithTriviaFrom(node.ReturnType);
                return node.WithReturnType(newReturnType);
            }
            return base.VisitMethodDeclaration(node);
        }

        // 处理委托声明中的返回类型替换
        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            var typeInfo = _semanticModel.GetTypeInfo(node.ReturnType);
            if (typeInfo.Type != null && TryGetReplaceType(node.ReturnType, typeInfo.Type, out var replacementType))
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
                var typeInfo = _semanticModel.GetTypeInfo(node.Type);
                if (typeInfo.Type != null && TryGetReplaceType(node.Type, typeInfo.Type, out var replacementType))
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
            Microsoft.CodeAnalysis.TypeInfo typeInfo = default;
            var typed2 = base.Visit(node.Type) as TypeSyntax;
            var node2 = node.WithType(typed2);


            if (typed2 is IdentifierNameSyntax identifierName)
            {
                typeInfo = _semanticModel.GetTypeInfo(node2);
            }
            else
            {
                return node2;
            }

            if (typeInfo.Type != null && TryGetReplaceType(node2.Type, typeInfo.Type, out var replacementType))
            {
                replacementType = replacementType.WithTriviaFrom(node.Type);
                return node.WithType(replacementType);
            }
            return base.VisitObjectCreationExpression(node);
        }




        //处理变量声明中的类型替换
        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var typeInfo = _semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(node.Type, typeInfo.Type, out var replacementType))
            {
                var type = Visit(node.Type) as TypeSyntax;
                var vars = node.Variables.Select(e => VisitVariableDeclarator(e) as VariableDeclaratorSyntax);
                var newNode = node.WithVariables(SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(vars));
                newNode = newNode.WithType(type);
                newNode = newNode.WithTriviaFrom(node);
                return newNode;
            }
            var node2 = base.VisitVariableDeclaration(node) as VariableDeclarationSyntax;
            return node2;
        }

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {

            return base.VisitVariableDeclarator(node);
        }



        // 处理类型转换表达式中的类型替换
        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            var typeInfo = _semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(node.Type, typeInfo.Type, out var replacementType))
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
                var rightTypeInfo = _semanticModel.GetTypeInfo(node.Right);
                if (rightTypeInfo.Type != null && TryGetReplaceType(node.Right, rightTypeInfo.Type, out var replacementType))
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
            var typeInfo = _semanticModel.GetTypeInfo(node.Type);
            if (typeInfo.Type != null && TryGetReplaceType(node.Type, typeInfo.Type, out var replacementType))
            {
                var newTypeNode = replacementType.WithTriviaFrom(node.Type);
                return node.WithType(newTypeNode);
            }
            return base.VisitTypeOfExpression(node);
        }

        public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
        {
            if (node.Parent is QualifiedNameSyntax)
            {
                return base.VisitQualifiedName(node);
            }
            if (node.Parent is NamespaceDeclarationSyntax)
            {
                return base.VisitQualifiedName(node);
            }
            if (node.Parent is UsingDirectiveSyntax)
            {
                return base.VisitQualifiedName(node);
            }


            var typed = base.VisitQualifiedName(node) as QualifiedNameSyntax;
            var symbolInfo = _semanticModel.GetSymbolInfo(typed);
            if (symbolInfo.Symbol is ITypeSymbol typeSymbol && TryGetReplaceType(typed, typeSymbol, out var replacementType))
            {

                return replacementType;
            }
            return base.VisitQualifiedName(node);
        }



        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Parent is QualifiedNameSyntax)
            {
                return base.VisitIdentifierName(node);
            }
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is ITypeSymbol type)
            {
                if (TryGetReplaceType(node, type, out var replacementType))
                {
                    //ParseNameSyntax(replacementType, out var _typeToken, out var _namespaceNameSyntax);
                    //Console.WriteLine(node.Location());

                    replacementType = replacementType.WithTriviaFrom(node);
                    return replacementType;
                }
            }
            return base.VisitIdentifierName(node);
        }





        public override SyntaxNode VisitGenericName(GenericNameSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is ITypeSymbol typeSymbol && TryGetReplaceType(node, typeSymbol, out var replacementType))
            {
                ParseNameSyntax(replacementType, out var typeName, out var _namespace);
                var typeArgumentList = VisitTypeArgumentList(node.TypeArgumentList) as TypeArgumentListSyntax;
                var newGenericName = SyntaxFactory.GenericName(typeName).WithTypeArgumentList(typeArgumentList);
                if (_namespace != null)
                {   // 命名空间 必须要
                    SyntaxFactory.QualifiedName(_namespace, newGenericName);
                }
                // 复制原节点的语法标记
                newGenericName = newGenericName.WithTriviaFrom(node);
                return newGenericName;
            }
            return base.VisitGenericName(node);
        }


        public void ParseNameSyntax(in NameSyntax nameSyntax, out SyntaxToken _typeToken, out NameSyntax _namespaceNameSyntax)
        {
            var nodes = nameSyntax.ToFullString().Split('.', StringSplitOptions.RemoveEmptyEntries);
            _typeToken = SyntaxFactory.Identifier(nodes.Last());
            var _namespace = String.Join('.', nodes[..^1]);

            if (String.IsNullOrEmpty(_namespace))
            {
                _namespaceNameSyntax = null;
            }
            else
            {
                _namespaceNameSyntax = SyntaxFactory.ParseName(_namespace);
            }
        }




        public override SyntaxNode VisitOmittedTypeArgument(OmittedTypeArgumentSyntax node)
        {
            return base.VisitOmittedTypeArgument(node);
        }


        public override SyntaxNode VisitArgument(ArgumentSyntax node)
        {
            return base.VisitArgument(node);
        }




        public override SyntaxNode VisitBaseList(BaseListSyntax node)
        {
            List<BaseTypeSyntax> args = new List<BaseTypeSyntax>();
            foreach (var arg in node.Types)
            {
                if (arg.Type is GenericNameSyntax baseType)
                {
                    var type = VisitGenericName(baseType) as GenericNameSyntax;
                    args.Add(arg.WithType(type));
                }
                else
                {
                    var typeInfo = _semanticModel.GetTypeInfo(arg.Type);
                    if (typeInfo.Type != null && TryGetReplaceType(arg.Type, typeInfo.Type, out var replacementType))
                    {
                        var arg2 = replacementType.WithTriviaFrom(arg.Type);
                        var v2 = arg.WithType(arg2);
                        args.Add(v2);
                    }
                    else
                    {
                        args.Add(arg);
                    }
                }


            }
            return node.WithTypes(SyntaxFactory.SeparatedList(args));
        }




        /// <summary>
        /// 列表，包括 泛型参数类型列表、方法参数列表、属性参数列表，BaseType列表
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public override SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list)
        {
            // AttributeSyntax AttributeArgumentSyntax ParameterSyntax VariableDeclaratorSyntax CollectionElementSyntax TypeParameterSyntax
            //Console.WriteLine(list.GetType().FullName);
            // 
            if (list is SeparatedSyntaxList<BaseTypeSyntax> baseTypeList)
            {
                List<TNode> args = new List<TNode>();
                foreach (var arg in baseTypeList)
                {
                    var typeInfo = _semanticModel.GetTypeInfo(arg.Type);
                    if (typeInfo.Type != null && TryGetReplaceType(arg.Type, typeInfo.Type, out var replacementType))
                    {
                        var arg2 = replacementType.WithTriviaFrom(arg.Type);
                        var v2 = arg.WithType(arg2);
                        args.Add(v2 as TNode);
                    }
                    else
                    {
                        args.Add(arg as TNode);
                    }
                }
                return SyntaxFactory.SeparatedList(args);
            }

            if (list is SeparatedSyntaxList<TypeSyntax> typeList)
            {
                List<TNode> args = new List<TNode>();
                foreach (var arg in typeList)
                {
                    var typeInfo = _semanticModel.GetTypeInfo(arg);
                    if (typeInfo.Type != null && TryGetReplaceType(arg, typeInfo.Type, out var replacementType))
                    {

                        var arg2 = replacementType.WithTriviaFrom(arg);
                        args.Add(arg2 as TNode);
                    }
                    else
                    {
                        args.Add(arg as TNode);
                    }
                }
                return SyntaxFactory.SeparatedList(args);
            }

            //Console.WriteLine(list.GetType().FullName);

            return base.VisitList(list);
        }





        //public override SyntaxNode VisitTypeArgumentList(TypeArgumentListSyntax node)
        //{
        //    node = base.VisitTypeArgumentList(node) as TypeArgumentListSyntax;
        //    List<TypeSyntax> args = new List<TypeSyntax>();
        //    foreach (var arg in node.Arguments)
        //    {
        //        var typeInfo = semanticModel.GetTypeInfo(arg);
        //        if (typeInfo.Type != null && TryGetReplaceType(arg, typeInfo.Type, out var replacementType))
        //        {
        //            var arg2 = replacementType.WithTriviaFrom(arg);
        //            args.Add(arg2);
        //        }
        //        else
        //        {
        //            args.Add((arg ));
        //        }
        //    }
        //    node = node.WithArguments(SyntaxFactory.SeparatedList(args));
        //    return node;
        //}


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
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            // 获取方法符号
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            // 判断是否是 Thread.Sleep 方法
            if (methodSymbol != null && methodSymbol.IsStatic)
            {

                if (TryGetReplaceType(node, methodSymbol.ContainingType, out var newTypeSyntax))
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
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            var memberSymbol = symbolInfo.Symbol as ISymbol;
            if (memberSymbol != null && memberSymbol.IsStatic && (memberSymbol.Kind == SymbolKind.Property || memberSymbol.Kind == SymbolKind.Field))
            {
                if (TryGetReplaceType(node, memberSymbol.ContainingType, out var newTypeSyntax))
                {
                    return node.WithExpression(newTypeSyntax);
                }
            }

            if (memberSymbol != null && memberSymbol.Kind == SymbolKind.NamedType)
            {
                // TODO nameof 带有命名空间时有问题啊
                var typeInfo = _semanticModel.GetTypeInfo(node);
                if (TryGetReplaceType(node, typeInfo.Type, out var newTypeSyntax))
                {
                    var fullName = newTypeSyntax.ToFullString();
                    var res = SyntaxFactory.ParseExpression(fullName).WithTriviaFrom(node);
                    return res;
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
                    var symbol = _semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                    var attr = attribute;
                    if (TryGetReplaceType(attribute, symbol.ContainingType, out var newTypeSyntax))
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
            var names = fullName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            NameSyntax nameSyntax = SyntaxFactory.IdentifierName(names[0]);
            for (var i = 1; i < names.Length; i++)
            {
                nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, SyntaxFactory.IdentifierName(names[i]));
            }
            return nameSyntax;
        }


        public SyntaxNode VisitWith(SemanticModel model, SyntaxNode root)
        {
            //var sss = MakeNameSyntax("AAA.BBB.CCC.ClassA");
            //Console.WriteLine(sss.ToFullString());
            _semanticModel = model;
            var ROOT = base.Visit(root);
            //Console.WriteLine(ROOT.NormalizeWhitespace());
            return ROOT;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // 生成脚本对象实例化方法
            // 检查是否有 ScriptAttribute
            bool hasScriptAttribute = HasAttribute(node, typeof(ScriptAttribute));
            // 检查是否继承自 BaseScript
            bool inheritsFromBaseScript = IsSubclassOf(node, typeof(AbstractScript));
            // 检查是否为非抽象类
            bool isAbstractClass = node.Modifiers.Any(SyntaxKind.AbstractKeyword);
            bool isStaticClass = node.Modifiers.Any(SyntaxKind.StaticKeyword);

            if (hasScriptAttribute && inheritsFromBaseScript && !isAbstractClass && !isStaticClass)
            {
                var newClassNode = Generate_Script_Instance_Method(node);
                return newClassNode;
            }
            return base.VisitClassDeclaration(node);
        }


        private bool IsSubclassOf(ClassDeclarationSyntax node, Type type)
        {
            var classSymbol = _semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
            var baseType = classSymbol.BaseType;
            var typeFullName = type.FullName;
            while (baseType != null)
            {
                var baseTypeFullName = baseType.ToDisplayString();
                if (baseTypeFullName == typeFullName)
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        private bool HasAttribute(ClassDeclarationSyntax node, Type attributeType)
        {
            var attrTypeFullName = attributeType.FullName;
            foreach (var attributeList in node.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbolInfo = _semanticModel.GetSymbolInfo(attribute);
                    var attributeSymbol = symbolInfo.Symbol as IMethodSymbol;
                    if (attributeSymbol?.ContainingType.ToString() == attrTypeFullName) return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 在脚本class中创建 脚本实例化的方法
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ClassDeclarationSyntax Generate_Script_Instance_Method(ClassDeclarationSyntax node)
        {
            //var attribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.ParseName(typeof(FactoryAttribute).FullName))));
            var methodBody = SyntaxFactory.ParseStatement("return new " + node.Identifier.Text + "();");
            var modifiers = new SyntaxTokenList([SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)]);
            var methodName = SyntaxFactory.Identifier(IdentifierDefine.GENERATE_SCRIPT_INSTANCE_METHOD);
            var returnType = SyntaxFactory.ParseTypeName("AbstractScript");
            var staticMethod = SyntaxFactory.MethodDeclaration(returnType, methodName)
                .WithModifiers(modifiers)
                //.WithAttributeLists(SyntaxFactory.SingletonList(attribute))
                .WithBody(SyntaxFactory.Block(methodBody));
            return node.AddMembers(staticMethod);
        }

    }
}
