using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Magnet.Core;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Scripting;




namespace Magnet.Syntax
{
    internal class SyntaxTreeWalker : CSharpSyntaxWalker
    {

        private SemanticModel semanticModel;
        public readonly List<Diagnostic> Diagnostics = new List<Diagnostic>();
        private ScriptOptions scriptOptions;
        public readonly HashSet<String> ReferencedAssemblies = new HashSet<String>();

        public SyntaxTreeWalker(ScriptOptions scriptOptions)
        {
            this.scriptOptions = scriptOptions;
        }

        public void VisitWith(SemanticModel model, SyntaxNode root)
        {
            semanticModel = model;
            base.Visit(root);
        }


        private void ReportDiagnosticInternal(DiagnosticDescriptor descriptor, CSharpSyntaxNode node, params Object[] messageArgs)
        {
            DiagnosticSeverity diagnosticSeverity = descriptor.DefaultSeverity;
            if (scriptOptions.diagnosticSeveritys.TryGetValue(descriptor.Id, out var reportDiagnostic))
            {
                if (reportDiagnostic == ReportDiagnostic.Suppress) return;
                if (reportDiagnostic != ReportDiagnostic.Default)
                {
                    diagnosticSeverity = InternalDiagnostics.MapReportToSeverity(reportDiagnostic);
                }
            };
            var diagnostic = Diagnostic.Create(descriptor, location: node.GetLocation(), additionalLocations: null, properties: null, effectiveSeverity: diagnosticSeverity, messageArgs: messageArgs);
            Diagnostics.Add(diagnostic);
        }

        private void CheckNamespace(CSharpSyntaxNode node, String _namespace)
        {
            if (this.scriptOptions.DisabledNamespaces.Contains(_namespace))
            {
                ReportDiagnosticInternal(InternalDiagnostics.IllegalNamespaces, node, _namespace);
            }
        }


        private void CheckType(CSharpSyntaxNode node)
        {
            var _typeFullName = "";
            var _namespace = "";
            if (node is TupleTypeSyntax tuple)
            {
                foreach (var item in tuple.Elements)
                {
                    CheckType(item.Type);
                }
                return;
            }
            ITypeSymbol type = null;
            // 参数
            if (node is ParameterSyntax parameterSyntax)
            {
                var parameterSymbol = semanticModel.GetDeclaredSymbol(node) as IParameterSymbol;
                if (parameterSymbol != null)
                {
                    type = parameterSymbol.Type;
                }
            }
            // 
            if (type == null && node is TypeSyntax typeSyntax)
            {
                var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                type = typeInfo.Type;
                if (type == null && node is IdentifierNameSyntax identifierNameSyntax)
                {
                    var symbol = semanticModel.GetSymbolInfo(identifierNameSyntax);
                    type = symbol.Symbol.ContainingType;
                }
            }
            // new 对象类型
            if (type == null && node is ObjectCreationExpressionSyntax creationExpressionSyntax)
            {
                var typeInfo = semanticModel.GetTypeInfo(creationExpressionSyntax);
                type = typeInfo.Type;
            }

            if (type == null)
            {
                Debugger.Break();
                return;
            }
            if (type.Kind == SymbolKind.ErrorType)
            {
                Debugger.Break();
                // 解析类型失败，类型错误，不处理 等后面编译器处理
                return;
            }

            // 处理可空类型
            if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
            {
                type = namedTypeSymbol.TypeArguments[0];
            }

            //拆数组类型
            while (type.Kind == SymbolKind.ArrayType || type.Kind == SymbolKind.PointerType)
            {
                if (type is IArrayTypeSymbol arrayTypeSymbol)
                {
                    type = arrayTypeSymbol.ElementType;
                }
                else if (type is IPointerTypeSymbol pointerTypeSymbol)
                {
                    type = pointerTypeSymbol.PointedAtType;
                }
            }

            // 过滤泛型参数T
            if (type.Kind != SymbolKind.TypeParameter)
            {
                _typeFullName = type.ToDisplayString();
                _namespace = type.ContainingNamespace.ToDisplayString();
            }

            if (!String.IsNullOrEmpty(_typeFullName))
            {
                //Console.WriteLine($"{node.Location()} {_typeFullName}");
                CheckNamespace(node, _namespace);

                var gl = _typeFullName.IndexOf('<');
                if (gl > 0)
                {
                    var _baseTypeName = _typeFullName.Substring(0, gl);
                    if (this.scriptOptions.DisabledTypes.Contains(_baseTypeName))
                    {
                        ReportDiagnosticInternal(InternalDiagnostics.IllegalTypes, node, _baseTypeName);
                    }
                }
                if (this.scriptOptions.DisabledTypes.Contains(_typeFullName))
                {
                    ReportDiagnosticInternal(InternalDiagnostics.IllegalTypes, node, _typeFullName);
                }
            }
            if (node is GenericNameSyntax generic)
            {
                foreach (var typeArg in generic.TypeArgumentList.Arguments)
                {
                    CheckType(typeArg);
                }
            }
        }

        /// <summary>
        /// 命名空间声明
        /// </summary>
        /// <param name="node"></param>
        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var _namespace = node.Name.ToString();
            CheckNamespace(node, _namespace);
            base.VisitNamespaceDeclaration(node);
        }







        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node"></param>
        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var containingClass = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (containingClass == null) return;
            var classSymbol = semanticModel.GetDeclaredSymbol(containingClass);
            if (classSymbol == null) return;
            if (HasAttribute(classSymbol, typeof(ScriptAttribute)))
            {
                if (IsSubclassOf(classSymbol, typeof(AbstractScript)))
                {
                    ReportDiagnosticInternal(InternalDiagnostics.IllegalConstructor, node, classSymbol.ToDisplayString());
                }
            }
            base.VisitConstructorDeclaration(node);
        }


        /// <summary>
        /// 析构函数
        /// </summary>
        /// <param name="node"></param>
        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            var containingClass = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (containingClass == null) return;
            var classSymbol = semanticModel.GetDeclaredSymbol(containingClass);
            if (classSymbol == null) return;
            if (HasAttribute(classSymbol, typeof(ScriptAttribute)))
            {
                if (IsSubclassOf(classSymbol, typeof(AbstractScript)))
                {
                    ReportDiagnosticInternal(InternalDiagnostics.IllegalDestructor, node, classSymbol.ToDisplayString());
                }
            }
            base.VisitDestructorDeclaration(node);
        }



        /// <summary>
        /// 继承类型列表
        /// </summary>
        /// <param name="node"></param>
        public override void VisitBaseList(BaseListSyntax node)
        {
            foreach (var baseTypeSyntax in node.Types)
            {
                CheckType(baseTypeSyntax.Type);
            }
            base.VisitBaseList(node);
        }


        /// <summary>
        /// 结构定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            base.VisitStructDeclaration(node);
        }

        /// <summary>
        /// 接口定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            base.VisitInterfaceDeclaration(node);
        }

        /// <summary>
        /// 类定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
            var hasScriptAttribute = false;
            var hasSubClassOfAbstractScript = false;
            if (HasAttribute(node, typeof(ScriptAttribute))) hasScriptAttribute = true;
            if (IsSubclassOf(node, typeof(AbstractScript))) hasSubClassOfAbstractScript = true;
            if (hasScriptAttribute || hasSubClassOfAbstractScript)
            {
                if (!hasScriptAttribute)
                {
                    ReportDiagnosticInternal(InternalDiagnostics.InvalidScriptWarning1, node, classSymbol.ToDisplayString());
                }

                if (!hasSubClassOfAbstractScript)
                {
                    ReportDiagnosticInternal(InternalDiagnostics.InvalidScriptWarning2, node, classSymbol.ToDisplayString());
                }
            }
            base.VisitClassDeclaration(node);
        }

        /// <summary>
        /// 委托定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            CheckType(node.ReturnType);
            foreach (var parameter in node.ParameterList.Parameters)
            {
                CheckType(parameter.Type);
            }
            base.VisitDelegateDeclaration(node);
        }

        /// <summary>
        /// 索引器定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            CheckType(node.Type);
            foreach (var parameter in node.ParameterList.Parameters)
            {
                CheckType(parameter.Type);
            }
            base.VisitIndexerDeclaration(node);
        }


        /// <summary>
        /// 命名空间引用
        /// </summary>
        /// <param name="node"></param>
        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var _namespace = node.Name.ToFullString();
            if (node.Alias is NameEqualsSyntax nameEqual)
            {
                CheckType(node.Name);
            }
            else
            {
                CheckNamespace(node, _namespace);
            }
            base.VisitUsingDirective(node);
        }


        /// <summary>
        /// 属性列表
        /// </summary>
        /// <param name="node"></param>
        public override void VisitAttributeList(AttributeListSyntax node)
        {
            foreach (var attribute in node.Attributes)
            {
                CheckType(attribute.Name);
            }
            base.VisitAttributeList(node);
        }

        /// <summary>
        /// 字段定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                if (!HasAttribute(node, typeof(GlobalAttribute)))
                {
                    var variableDeclaration = node.Declaration;
                    var fieldType = variableDeclaration.Type.ToString();
                    foreach (var variable in variableDeclaration.Variables)
                    {
                        var fieldName = variable.Identifier.Text;
                        ReportDiagnosticInternal(InternalDiagnostics.ConfusingGlobalFieldDefinitionWarning, node, fieldName, fieldType);
                    }
                }
            }
            base.VisitFieldDeclaration(node);
        }

        /// <summary>
        /// 属性定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            CheckType(node.Type);
            if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                if (!HasAttribute(node, typeof(GlobalAttribute)))
                {
                    var propertyType = node.Type.ToString();
                    var propertyName = node.Identifier.Text;
                    ReportDiagnosticInternal(InternalDiagnostics.ConfusingGlobalPropertyDefinitionWarning, node, propertyName, propertyType);
                }
            }
            base.VisitPropertyDeclaration(node);
        }

        /// <summary>
        /// Is匹配表达式
        /// </summary>
        /// <param name="node"></param>
        public override void VisitIsPatternExpression(IsPatternExpressionSyntax node)
        {
            if (node.Pattern is DeclarationPatternSyntax declaration)
            {
                CheckType(declaration.Type);
            }
            base.VisitIsPatternExpression(node);
        }


        //public override void VisitTupleType(TupleTypeSyntax node)
        //{
        //    base.VisitTupleType(node);
        //}

        //public override void VisitArrayType(ArrayTypeSyntax node)
        //{
        //    base.VisitArrayType(node);
        //}

        /// <summary>
        /// 泛型参数
        /// </summary>
        /// <param name="node"></param>
        //public override void VisitTypeParameter(TypeParameterSyntax node)
        //{
        //    base.VisitTypeParameter(node);
        //}

        /// <summary>
        /// 泛型参数列表
        /// </summary>
        /// <param name="node"></param>
        //public override void VisitTypeParameterList(TypeParameterListSyntax node)
        //{
        //    base.VisitTypeParameterList(node);
        //}


        //public override void VisitPointerType(PointerTypeSyntax node)
        //{
        //    base.VisitPointerType(node);
        //}

        //public override void VisitInitializerExpression(InitializerExpressionSyntax node)
        //{
        //    base.VisitInitializerExpression(node);
        //}
        public override void VisitParameter(ParameterSyntax node)
        {
            CheckType(node);
            base.VisitParameter(node);
        }




        /// <summary>
        /// 泛型类型约束
        /// </summary>
        /// <param name="node"></param>
        public override void VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
        {
            foreach (var constraint in node.Constraints)
            {
                if (constraint is TypeConstraintSyntax typeConstraintSyntax)
                {
                    CheckType(typeConstraintSyntax.Type);
                }
            }
            base.VisitTypeParameterConstraintClause(node);
        }


        /// <summary>
        /// await 表达式
        /// </summary>
        /// <param name="node"></param>
        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            if (!this.scriptOptions.AllowAsync)
            {
                ReportDiagnosticInternal(InternalDiagnostics.AsyncUsageNotAllowed, node);
            }
        }


        /// <summary>
        /// 方法声明
        /// </summary>
        /// <param name="node"></param>
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (node.Modifiers.Any(SyntaxKind.AsyncKeyword) && !scriptOptions.AllowAsync)
            {
                ReportDiagnosticInternal(InternalDiagnostics.AsyncUsageNotAllowed, node);
            }
            // Return Type
            CheckType(node.ReturnType);
            base.VisitMethodDeclaration(node);
        }


        /// <summary>
        /// typeof 表达式
        /// </summary>
        /// <param name="node"></param>
        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            CheckType(node.Type);
            base.VisitTypeOfExpression(node);
        }

        /// <summary>
        /// 变量定义
        /// </summary>
        /// <param name="node"></param>
        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            CheckType(node.Type);
            base.VisitVariableDeclaration(node);
        }

        /// <summary>
        /// 成员访问
        /// </summary>
        /// <param name="node"></param>
        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            CheckType(node.Name);
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
        {
            CheckType(node.Name);
            base.VisitMemberBindingExpression(node);
        }

        /// <summary>
        /// 条件成员访问
        /// </summary>
        /// <param name="node"></param>
        public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            if (node.Expression is IdentifierNameSyntax identifierNameSyntax)
            {
                CheckType(node.Expression);
            }
            base.VisitConditionalAccessExpression(node);
        }


        /// <summary>
        /// call
        /// </summary>
        /// <param name="node"></param>
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // Generic Type
            if (node.Expression is GenericNameSyntax generic)
            {
                foreach (var typeArg in generic.TypeArgumentList.Arguments)
                {
                    CheckType(typeArg);
                }
            }
            // nameof
            if (node.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text == "nameof")
            {
                CheckType(node.ArgumentList.Arguments[0].Expression as TypeSyntax);
            }
            base.VisitInvocationExpression(node);
        }

        /// <summary>
        /// new 表达式
        /// </summary>
        /// <param name="node"></param>
        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            CheckType(node);  // node.Type
            base.VisitObjectCreationExpression(node);
        }


        /// <summary>
        /// 类型强制转换
        /// </summary>
        /// <param name="node"></param>
        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            CheckType(node.Type);
            base.VisitCastExpression(node);
        }




        private bool HasAttribute(MemberDeclarationSyntax node, Type attributeType)
        {
            var attrTypeFullName = attributeType.FullName;
            foreach (var attributeList in node.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(attribute);
                    var attributeSymbol = symbolInfo.Symbol as IMethodSymbol;
                    if (attributeSymbol?.ContainingType.ToString() == attrTypeFullName) return true;
                }
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
                    var symbolInfo = semanticModel.GetSymbolInfo(attribute);
                    var attributeSymbol = symbolInfo.Symbol as IMethodSymbol;
                    if (attributeSymbol?.ContainingType.ToString() == attrTypeFullName) return true;
                }
            }
            return false;
        }


        private bool IsSubclassOf(ClassDeclarationSyntax node, Type type)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
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


        private Boolean HasAttribute(INamedTypeSymbol typeSymbol, Type attributeType)
        {
            foreach (var attribute in typeSymbol.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == attributeType.Name && attribute.AttributeClass?.ContainingNamespace.ToString() == attributeType.Namespace)
                {
                    return true;
                }
            }
            return false;
        }

        private Boolean IsSubclassOf(INamedTypeSymbol typeSymbol, Type baseType)
        {
            var _baseType = typeSymbol.BaseType;
            while (_baseType != null)
            {
                if (_baseType.Name == baseType.Name && _baseType.ContainingNamespace.ToString() == baseType.Namespace)
                {
                    return true;
                }
                _baseType = _baseType.BaseType;
            }
            return false;
        }
    }
}