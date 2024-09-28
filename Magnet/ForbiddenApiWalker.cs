using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Magnet
{


    public class ForbiddenSymbols
    {
        public String Method;
        public String Typed;
    }


    // ms

    // 自定义的 Roslyn 语法树 Walker，用于检测不允许的 API 调用
    public class ForbiddenApiWalker : CSharpSyntaxWalker
    {
        private SemanticModel semanticModel;
        public readonly List<String> ForbiddenApis = new List<String>();


        public bool HasForbiddenApis
        {
            get
            {
                return ForbiddenApis.Count > 0;
            }
        }


        public void VisitWith(SemanticModel model, SyntaxNode root)
        {
            this.semanticModel = model;
            base.Visit(root);
        }






        private List<ForbiddenSymbols> forbiddenSymbols = new List<ForbiddenSymbols>()
        {
            new ForbiddenSymbols(){ Method = "Load", Typed = "Assembly"},
            new ForbiddenSymbols(){ Method = "LoadFrom", Typed = "Assembly"},
            new ForbiddenSymbols(){ Method = "Start", Typed = "Process"},
            new ForbiddenSymbols(){ Method = "Start", Typed = "Thread"},
            new ForbiddenSymbols(){ Method = "QueueUserWorkItem", Typed = "ThreadPool"},
            new ForbiddenSymbols(){ Method = "GetMethods", Typed = "Type"},
            new ForbiddenSymbols(){ Method = "GetMethod", Typed = "Type"},



            

            new ForbiddenSymbols(){ Typed = "FileStream"},
            new ForbiddenSymbols(){ Typed = "File"},
            new ForbiddenSymbols(){ Typed = "Directory"},
            new ForbiddenSymbols(){ Typed = "Thread"},
            new ForbiddenSymbols(){ Typed = "Type"},
            new ForbiddenSymbols(){ Typed = "Process"},
            new ForbiddenSymbols(){ Typed = "Assembly"},
        };


        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // 获取左侧表达式的类型信息
            var typeInfo = this.semanticModel.GetTypeInfo(node.Expression);
            var type = typeInfo.Type?.ToString();

            if (node.Name.Identifier.Text == "Socket")
            {
                if (type == "System.Net" || type == "System.Net.Sockets")
                {


                    var location = node.GetLocation();
                    var lineSpan = location.GetLineSpan();
                    Console.WriteLine($"Forbidden API 'Socket' accessed at line {lineSpan.StartLinePosition.Line + 1}, " +
                                      $"column {lineSpan.StartLinePosition.Character + 1}");
                }
            }

            base.VisitMemberAccessExpression(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // 获取方法的语义信息
            var symbol = this.semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
            {
                // 检查方法的特性是否包含 DllImport

                // System.Runtime.CompilerServices.ModuleInitializer
                // System.Runtime.InteropServices.DllImportAttribute
                var dllImportAttribute = symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.ToString() == "System.Runtime.InteropServices.DllImportAttribute");
                if (dllImportAttribute != null)
                {
                    this.AddReport(node, $"DllImport");
                }
            }

            base.VisitMethodDeclaration(node);
        }


        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            DefaultVisit(node);
        }


        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {


            // 获取调用表达式的标识符或成员访问表达式
            //var expression = node.Expression;

            // 检查是否是成员访问表达式（例如：myObject.Method()）
            if (node.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                // 尝试获取被调用对象的类型（即 memberAccess 的表达式部分）
                var typeSyntax = memberAccess.Expression;
                // 获取该类型的符号信息
                var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                var typeName = memberAccess.Expression.ToString();
                var methodName = memberAccess.Name.Identifier.Text;
                // 检查是否存在类型别名
                if (typeInfo.Type != null)
                {
                    var aliasInfo = semanticModel.GetAliasInfo(typeSyntax);
                    if (aliasInfo != null)
                    {
                        // 如果有别名，输出别名和对应的实际类型
                        typeName = typeInfo.Type.Name;
                    }
                    else
                    {
                        // 没有别名，输出实际类型
                        typeName = typeInfo.Type.Name;
                    }
                }
                var symbols = forbiddenSymbols.Find(e =>  e.Typed == typeName && (String.IsNullOrEmpty(e.Method) ||  e.Method == methodName )  );
                if (symbols != null)
                {
                    this.AddReport(node, $"{typeName}.{methodName}");
                }

            }

            //// 获取左侧表达式的类型信息
            //var typeInfo = this.semanticModel.GetTypeInfo(node.Expression);
            //var type = typeInfo.Type?.ToString();


            base.VisitInvocationExpression(node);
        }






        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            // 检查是否是 Socket 的实例化
            var typeInfo = this.semanticModel.GetTypeInfo(node);
            var type = typeInfo.Type?.ToString();
            if (type == "System.Threading.Thread")
            {
                // 获取位置并打印行列信息
                var location = node.GetLocation();
                var lineSpan = location.GetLineSpan();

                this.AddReport(node, $"new {type}");
            }
            base.VisitObjectCreationExpression(node);
        }




        private void AddReport(CSharpSyntaxNode node, String keyword)
        {
            // 获取位置并打印行列信息
            var location = node.GetLocation();
            var lineSpan = location.GetLineSpan();
            // 输出错误信息，包含行列
            Console.WriteLine($"({lineSpan.StartLinePosition.Line + 1},{lineSpan.StartLinePosition.Character + 1}) Error: Forbidden API '{keyword}' detected in script.");
        }

    }
}
