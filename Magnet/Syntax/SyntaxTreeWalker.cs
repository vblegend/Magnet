using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Magnet.Core;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;



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
            base.VisitClassDeclaration(node);
        }



        //public override void VisitUsingDirective(UsingDirectiveSyntax node)
        //{
        //    var _namespace = node.Name.ToFullString();
        //    if (node.Alias is NameEqualsSyntax nameEqual)
        //    {
        //        var typeInfo = semanticModel.GetTypeInfo(node.Name);
        //        if (typeInfo.ConvertedType != null)
        //        {
        //            _namespace = typeInfo.ConvertedType.ContainingNamespace.ToDisplayString();
        //        }
        //    }
        //    // 检测 using 引用的 命名空间
        //    // Console.WriteLine($"{node.Location()}  {node.Parent.GetType().Name} {_namespace}");
        //    base.VisitUsingDirective(node);
        //}


        private void CheckNamespace(CSharpSyntaxNode node, String _namespace)
        {
            if (this.scriptOptions.DisabledNamespace.Contains(_namespace))
            {
                Diagnostics.Add(Diagnostic.Create(IllegalNamespaces, node.GetLocation(), _namespace));
            }
        }

        public override void VisitQualifiedName(QualifiedNameSyntax node)
        {
            var _namespace = node.ToFullString();
            if (node.Parent is UsingDirectiveSyntax usingNode)
            {
                _namespace = usingNode.Name.ToFullString();
                if (usingNode.Alias is NameEqualsSyntax nameEqual)
                {
                    var typeInfo = semanticModel.GetTypeInfo(usingNode.Name);
                    if (typeInfo.ConvertedType != null)
                    {
                        _namespace = typeInfo.ConvertedType.ContainingNamespace.ToDisplayString();
                    }
                }
            }
            else
            {
                var symbolInfo = semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol?.Kind == SymbolKind.NamedType)
                {
                    var typeSymbol = (INamedTypeSymbol)symbolInfo.Symbol;
                    _namespace = typeSymbol.ContainingNamespace.ToDisplayString();
                }
            }
            CheckNamespace(node, _namespace);
        }


        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            //Console.WriteLine($"{node.Location()}  {node.Parent.GetType().Name} {node.Identifier.Text}");
            // 获取符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;
            if (symbol != null)
            {
                var containingAssembly = symbol.ContainingAssembly;
                if (containingAssembly != null)
                {
                    // 添加引用的程序集名称
                    ReferencedAssemblies.Add(containingAssembly.Name);
                }
            }

            base.VisitIdentifierName(node);
        }










        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node.Expression);
            var typeName = typeInfo.Type?.ToDisplayString();
            var memberName = node.Name.Identifier.Text;
            var isStatic = typeInfo.Type.IsStatic;


            base.VisitMemberAccessExpression(node);
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
            //if (symbol != null)
            //{
            //    var ModuleInitializerAttribute = symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.ToString() == "System.Runtime.CompilerServices.ModuleInitializerAttribute");
            //    if (ModuleInitializerAttribute != null)
            //    {
            //        AddReport(node, $"ModuleInitializer");
            //    }

            //    // 检查方法的特性是否包含 DllImport
            //    var dllImportAttribute = symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.ToString() == "System.Runtime.InteropServices.DllImportAttribute");
            //    if (dllImportAttribute != null)
            //    {
            //        AddReport(node, $"DllImport");
            //    }
            //}
            base.VisitMethodDeclaration(node);
        }

        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            base.VisitTypeOfExpression(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol != null)
            {
                var methodName = methodSymbol.Name;
                var containingTypeName = methodSymbol.ContainingType.ToDisplayString();
                bool isStatic = methodSymbol.IsStatic;
                //Console.WriteLine($"Method: {methodName}, Type: {containingTypeName}, IsStatic: {isStatic}");
            }
            base.VisitInvocationExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            var type = typeInfo.Type?.ToDisplayString();
            //
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