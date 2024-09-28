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
                var newType = SyntaxFactory.ParseTypeName("MyCustomNamespace.MyThread");
                return node.WithType(newType);
            }
            return base.VisitObjectCreationExpression(node);
        }



        public SyntaxNode VisitWith(SemanticModel model, SyntaxNode root)
        {
            this.semanticModel = model;
            return base.Visit(root);
        }



    }
}
