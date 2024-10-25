using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Magnet.Core;



namespace Magnet.Syntax
{
    internal class SyntaxTreeWalker : CSharpSyntaxWalker
    {

        private SemanticModel semanticModel;
        public readonly List<Diagnostic> Diagnostics = new List<Diagnostic>();
        private ScriptOptions scriptOptions;
        public readonly HashSet<String> ReferencedAssemblies = new HashSet<String>();


        private static readonly DiagnosticDescriptor InvalidScriptWarning1 = new DiagnosticDescriptor(
                id: "SW001",
                title: "Invalid Script Warning",
                messageFormat: "Script '{0}' is missing the [ScriptAttribute] attribute",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);



        private static readonly DiagnosticDescriptor InvalidScriptWarning2 = new DiagnosticDescriptor(
                id: "SW002",
                title: "Invalid Script Warning",
                messageFormat: "The script '{0}' does not inherit an 'AbstractScript'",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);


        private static readonly DiagnosticDescriptor ConfusingGlobalFieldDefinitionWarning = new DiagnosticDescriptor(
                id: "SW003",
                title: "Confusing Global Field Definition Warning",
                messageFormat: "If the field '{1} {0};' is a global variable, mark the [GlobalAttribute] attribute",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);


        private static readonly DiagnosticDescriptor ConfusingGlobalPropertyDefinitionWarning = new DiagnosticDescriptor(
                id: "SW004",
                title: "Confusing Global Property Definition Warning",
                messageFormat: "If the property '{1} {0};' is a global variable, mark the [GlobalAttribute] attribute",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);


        private static readonly DiagnosticDescriptor AsyncUsageNotAllowed = new DiagnosticDescriptor(
               id: "SE001",
               title: "Async usage not allowed",
               messageFormat: "async/await usage is prohibited in this context",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);


        private static readonly DiagnosticDescriptor IllegalNamespaces = new DiagnosticDescriptor(
               id: "SE002",
               title: "Illegal Namespaces, usage not allowed",
               messageFormat: "Namespace '{0}' has been disabled",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor IllegalTypes = new DiagnosticDescriptor(
               id: "SE003",
               title: "Illegal Type, usage not allowed",
               messageFormat: "Typed '{0}' has been disabled",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);

        public SyntaxTreeWalker(ScriptOptions scriptOptions)
        {
            this.scriptOptions = scriptOptions;
        }

        public void VisitWith(SemanticModel model, SyntaxNode root)
        {
            semanticModel = model;
            base.Visit(root);
        }




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
                    Diagnostics.Add(Diagnostic.Create(InvalidScriptWarning1, node.GetLocation(), classSymbol.ToDisplayString()));
                }

                if (!hasSubClassOfAbstractScript)
                {
                    Diagnostics.Add(Diagnostic.Create(InvalidScriptWarning2, node.GetLocation(), classSymbol.ToDisplayString()));
                }
            }
            // 获取基类和接口的声明列表
            if (node.BaseList != null)
            {
                foreach (var baseTypeSyntax in node.BaseList.Types)
                {
                    var typeInfo = semanticModel.GetTypeInfo(baseTypeSyntax.Type);
                    var typeSymbol = typeInfo.Type;
                    if (typeSymbol != null)
                    {
                        if (typeSymbol.TypeKind == TypeKind.Class)
                        {
                            // base class
                            CheckType(baseTypeSyntax, typeSymbol);
                        }
                        else if (typeSymbol.TypeKind == TypeKind.Interface)
                        {
                            // interface 
                            CheckType(baseTypeSyntax, typeSymbol);
                        }
                    }
                }
            }


            base.VisitClassDeclaration(node);
        }




        private void CheckNamespace(CSharpSyntaxNode node, String _namespace)
        {
            if (this.scriptOptions.DisabledNamespaces.Contains(_namespace))
            {
                Diagnostics.Add(Diagnostic.Create(IllegalNamespaces, node.GetLocation(), _namespace));
            }
        }

        private void CheckType(CSharpSyntaxNode node, ITypeSymbol typeSymbol)
        {
            var typeFullName = typeSymbol.ToDisplayString();
            var _namespace = typeSymbol.ContainingNamespace.ToDisplayString();
            CheckNamespace(node, _namespace);
            if (this.scriptOptions.DisabledTypes.Contains(typeFullName))
            {
                Diagnostics.Add(Diagnostic.Create(IllegalTypes, node.GetLocation(), typeFullName));
            }
            //Console.WriteLine($"{node.Location()} {typeFullName}");
        }



        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var _namespace = node.Name.ToFullString();
            if (node.Alias is NameEqualsSyntax nameEqual)
            {
                var typeInfo = semanticModel.GetTypeInfo(node.Name);
                if (typeInfo.ConvertedType != null)
                {
                    CheckType(node, typeInfo.Type);
                    _namespace = typeInfo.ConvertedType.ContainingNamespace.ToDisplayString();
                }
            }
            else
            {
                CheckNamespace(node, _namespace);
            }
            base.VisitUsingDirective(node);
        }



        public override void VisitAttributeList(AttributeListSyntax node)
        {
            foreach (var attribute in node.Attributes)
            {
                var attributeTypeInfo = semanticModel.GetTypeInfo(attribute);
                CheckType(attribute, attributeTypeInfo.Type);
            }
            base.VisitAttributeList(node);
        }

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
                        Diagnostics.Add(Diagnostic.Create(ConfusingGlobalFieldDefinitionWarning, node.GetLocation(), fieldName, fieldType));
                    }
                }
            }
            base.VisitFieldDeclaration(node);
        }


        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            CheckType(node.Type, typeInfo.Type);
            if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                if (!HasAttribute(node, typeof(GlobalAttribute)))
                {
                    var propertyType = node.Type.ToString();
                    var propertyName = node.Identifier.Text;
                    Diagnostics.Add(Diagnostic.Create(ConfusingGlobalPropertyDefinitionWarning, node.GetLocation(), propertyName, propertyType));
                }
            }
            base.VisitPropertyDeclaration(node);
        }




        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            if (!this.scriptOptions.AllowAsync)
            {
                Diagnostics.Add(Diagnostic.Create(AsyncUsageNotAllowed, node.GetLocation()));
            }
        }



        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (node.Modifiers.Any(SyntaxKind.AsyncKeyword) && !scriptOptions.AllowAsync)
            {
                Diagnostics.Add(Diagnostic.Create(AsyncUsageNotAllowed, node.GetLocation()));
            }
            // Return Type
            var typeInfo = semanticModel.GetTypeInfo(node.ReturnType);
            CheckType(node.ReturnType, typeInfo.Type);
            // Parameters Type
            foreach (var parameter in node.ParameterList.Parameters)
            {
                var parameterTypeInfo = semanticModel.GetTypeInfo(parameter.Type);
                if (parameterTypeInfo.Type != null)
                {
                    CheckType(parameter.Type, parameterTypeInfo.Type);
                }
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            CheckType(node.Type, typeInfo.Type);
            base.VisitTypeOfExpression(node);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Type);
            var typeSymbol = typeInfo.Type;
            if (typeSymbol != null)
            {
                if (node.Type is GenericNameSyntax generic)
                {
                    var genericTypes = typeSymbol.ToDisplayString().Split("<", StringSplitOptions.RemoveEmptyEntries);
                    CheckType(node.Type, typeSymbol);
                    foreach (var typeArg in generic.TypeArgumentList.Arguments)
                    {
                        var typeArgInfo = semanticModel.GetTypeInfo(typeArg);
                        CheckType(typeArg, typeArgInfo.Type);
                    }
                }
                else
                {
                    CheckType(node.Type, typeSymbol);
                }
            }
            base.VisitVariableDeclaration(node);
        }


        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // 获取访问的符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;
            if (symbol != null)
            {
                // 判断是否为静态访问
                if (symbol.IsStatic)
                {
                    // 静态成员访问的静态类型
                    CheckType(node.Name, symbol.ContainingType);
                }
                else
                {        
                    // 实例成员访问的实例类型
                    CheckType(node.Name, symbol.ContainingType);
                }
            }
            base.VisitMemberAccessExpression(node);
        }


        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol != null)
            {
                // Generic Type
                if (node.Expression is GenericNameSyntax generic)
                {
                    foreach (var typeArg in generic.TypeArgumentList.Arguments)
                    {
                        var typeArgInfo = semanticModel.GetTypeInfo(typeArg);
                        CheckType(typeArg, typeArgInfo.Type);
                    }
                }
            }
            // nameof
            if (node.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text == "nameof")
            {
                var typeInfo = semanticModel.GetTypeInfo(node.ArgumentList.Arguments[0].Expression);
                CheckType(node.Expression, typeInfo.Type);
            }
            base.VisitInvocationExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            CheckType(node, typeInfo.Type);
            base.VisitObjectCreationExpression(node);
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
    }
}