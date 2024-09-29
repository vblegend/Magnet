using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Magnet
{
    internal class SyntaxTreeRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel semanticModel;




        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            if (typeInfo.Type?.ToString() == "System.Threading.Thread")
            {
                // 替换为自定义的 MyThread
                var newType = SyntaxFactory.ParseTypeName("Magnet.Proxy.ThreadProxy");
                var newObjectCreation = node.WithType(newType);
                Console.WriteLine(newObjectCreation.ToFullString());
                return newObjectCreation;
            }
            return base.VisitObjectCreationExpression(node);
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

        // 判断是不是调用 debugger()
        private bool IsDebuggerCall(InvocationExpressionSyntax node)
        {
            var expression = node.Expression;
            if (expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text == "debugger";
            }
            return false;
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // 获取方法的符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            // 获取方法符号
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;


            //if (methodSymbol != null && methodSymbol.Name == "Invoke" && IsDebuggerCall(node))
            //{
            //    // 返回 null 以删除该调用
            //    return null;
            //}








            // 判断是否是 Thread.Sleep 方法
            if (methodSymbol != null && methodSymbol.ContainingType.ToString() == "System.Threading.Thread")
            {
                if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (methodSymbol.IsStatic)
                    {
                        //Console.WriteLine(memberAccess.Expression);
                        //var newExpression = memberAccess.WithExpression(MakeNameSyntax("Magnet.Proxy.ThreadProxy"));
                        //构建新的 ThreadProxy.Sleep 调用
                        var newExpression = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MakeNameSyntax("Magnet.Proxy.ThreadProxy"),
                            SyntaxFactory.IdentifierName(methodSymbol.Name)
                        );
                        //替换原来的方法调用表达式
                        var newInvocation = node.WithExpression(newExpression);
                        return newInvocation;
                    }
                }
            }

            // 如果不是 Thread.Sleep 调用，返回原节点
            return base.VisitInvocationExpression(node);
        }


        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // 获取属性或字段的符号信息
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var memberSymbol = symbolInfo.Symbol as ISymbol;

            if (memberSymbol != null && memberSymbol.ContainingType.ToString() == "System.Threading.Thread")
            {
                // 判断是属性或字段的访问
                if (memberSymbol.Kind == SymbolKind.Property || memberSymbol.Kind == SymbolKind.Field)
                {
                    // 将 Thread.CurrentThread 等静态属性、字段替换为 ThreadProxy 对应的属性或字段
                    var newExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MakeNameSyntax("Magnet.Proxy.ThreadProxy"),
                        SyntaxFactory.IdentifierName(memberSymbol.Name)
                    );
                    var result = node.WithExpression(MakeNameSyntax("Magnet.Proxy.ThreadProxy"));
                    //Console.WriteLine(result.ToFullString());
                    return result;
                }
            }
            return base.VisitMemberAccessExpression(node);
        }

        public SyntaxNode VisitWith(SemanticModel model, SyntaxNode root)
        {
            this.semanticModel = model;
            return base.Visit(root);
        }



    }
}
