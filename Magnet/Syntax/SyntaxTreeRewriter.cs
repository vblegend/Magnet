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

        public Boolean TryGetReplaceType(CSharpSyntaxNode syntaxNode, ITypeSymbol typeSymbol, out NameSyntax nameSyntax)
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
            return base.VisitList(list);
        }


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
            //bool hasScriptAttribute = HasAttribute(node, typeof(ScriptAttribute));
            //// 检查是否继承自 BaseScript
            //bool inheritsFromBaseScript = IsSubclassOf(node, typeof(AbstractScript));
            //// 检查是否为非抽象类
            //bool isAbstractClass = node.Modifiers.Any(SyntaxKind.AbstractKeyword);
            //bool isStaticClass = node.Modifiers.Any(SyntaxKind.StaticKeyword);


            //if (hasScriptAttribute && inheritsFromBaseScript && !isAbstractClass && !isStaticClass)
            //{
            //    node = base.VisitClassDeclaration(node) as ClassDeclarationSyntax;
            //    var method = Generate_Script_Instance_Method(node);
            //    node = node.AddMembers(method);
            //    return node;
            //}
            return base.VisitClassDeclaration(node) as ClassDeclarationSyntax;
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




    }
}


/*
 
 
         /// <summary>
        /// 在脚本class中创建 脚本实例化的方法
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private MethodDeclarationSyntax Generate_Script_Instance_Method(ClassDeclarationSyntax node)
        {
            var methodBody = SyntaxFactory.ParseStatement("return new " + node.Identifier.Text + "();");
            var modifiers = new SyntaxTokenList([SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)]);
            var methodName = SyntaxFactory.Identifier(IdentifierDefine.GENERATE_SCRIPT_INSTANCE_METHOD);
            var returnType = SyntaxFactory.ParseTypeName("AbstractScript");
            var staticMethod = SyntaxFactory.MethodDeclaration(returnType, methodName)
                .WithModifiers(modifiers)
                .WithBody(SyntaxFactory.Block(methodBody));
            return staticMethod;
        }

        private struct AutowiredField
        {
            public IFieldSymbol FieldSymbol;
            public String Type;
            public String SlotName;
        }


        private List<AutowiredField> FindAutowiredFields(ClassDeclarationSyntax classDeclaration)
        {
            List<AutowiredField> list = new List<AutowiredField>();
            // 列出基类中的字段
            var baseType = _semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            while (baseType != null && baseType.SpecialType != SpecialType.System_Object)
            {
                var fields = baseType.GetMembers().OfType<IFieldSymbol>();
                foreach (var field in fields)
                {
                    // 获取字段的属性
                    var attributes = field.GetAttributes();
                    foreach (var attribute in attributes)
                    {
                        // 检查是否是 Abs 属性
                        if (attribute.AttributeClass?.Name == "AutowiredAttribute" &&
                            attribute.AttributeClass?.ContainingNamespace?.ToString() == "Magnet.Core") // 请替换为实际命名空间
                        {
                            // 获取字段类型和名称
                            var fieldType = field.Type.ToDisplayString();
                            var fieldName = field.Name;


                            var typeName = "";
                            var slotName = "";
                            foreach (TypedConstant arg in attribute.ConstructorArguments)
                            {
                                // 检查参数类型并根据类型赋值
                                if (arg.Kind == TypedConstantKind.Type && arg.Value is INamedTypeSymbol typeSymbol)
                                {
                                    // 获取 Type 参数
                                    typeName = typeSymbol.ToDisplayString();
                                }
                                else if (arg.Kind == TypedConstantKind.Primitive && arg.Value is string stringValue)
                                {
                                    // 获取 ProviderName 参数
                                    slotName = stringValue;
                                }
                            }
                            list.Add(new AutowiredField() { FieldSymbol = field, SlotName = slotName, Type = typeName });
                        }
                    }
                }
                baseType = baseType.BaseType;
            }
            return list;
        }


        /// <summary>
        /// 构造方法
        /// 本来想用构造方法实现注入，但脚本之间的对象注入没法实现，方法注入又不支持readonly字段
        /// 构造函数可以加速20%
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private MethodDeclarationSyntax GenerateConstructorMethod(ClassDeclarationSyntax node, List<AutowiredField> fields)
        {
            // 获取标记了 [Autowired] 属性的字段信息
            var statements = new List<StatementSyntax>();
            var index = 0;
            foreach (var filed in fields)
            {
                var fieldName = SyntaxFactory.IdentifierName(filed.FieldSymbol.Name);
                var fieldType = SyntaxFactory.ParseTypeName(filed.FieldSymbol.Type.ToDisplayString());
                var varName = "var" + index;
                List<String> expressions = [];
                // if (typeof(ScriptExample) == provider.GetType() && provider is ScriptExample var2 && slotName == "B")
                expressions.Add($"item.Value is {fieldType} {varName}");

                if (!String.IsNullOrEmpty(filed.Type))
                {
                    expressions.Add($"item.TypeIs<{filed.Type}>()");
                }

                if (!String.IsNullOrEmpty(filed.SlotName))
                {
                    expressions.Add($"item.SlotName == \"{filed.SlotName}\"");
                }
                var condition = SyntaxFactory.ParseExpression(String.Join(" && ", expressions));
                var assignment = SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldName, SyntaxFactory.IdentifierName(varName)));
                var ifStatement = SyntaxFactory.IfStatement(condition, SyntaxFactory.Block(assignment));
                statements.Add(ifStatement);
                index++;
            }
            var fors = SyntaxFactory.ForEachStatement(SyntaxFactory.ParseTypeName("var"), "item", SyntaxFactory.IdentifierName("providers"), SyntaxFactory.Block(statements));
            var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), nameof(IScriptInstance.Initialize))
                 .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[]
                 {
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("providers")).WithType(SyntaxFactory.ParseTypeName("System.Collections.Generic.List<IObjectProvider>")),
                 })))
                 .WithExplicitInterfaceSpecifier(SyntaxFactory.ExplicitInterfaceSpecifier(SyntaxFactory.IdentifierName(nameof(IScriptInstance))))
                .WithBody(SyntaxFactory.Block(statements)
                );
            method = method.WithBody(SyntaxFactory.Block(fors));

            return method;
        }
 
 */