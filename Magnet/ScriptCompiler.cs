using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Text;
using Magnet.Context;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Globalization;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Magnet
{
    public class ScriptCompiler
    {
        private String[] baseUsing = new String[] { "System", "Magnet.Context", "System.Threading.Tasks" };
        public ScriptOptions Options { get; private set; }

        public PortableExecutableReference[] referencesForCodegen = [];

        private List<String> diagnostics;


        public IReadOnlyList<String> Diagnostics => diagnostics;

        private CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        private ScriptLoadContext scriptLoadContext = new ScriptLoadContext();
        public static readonly Assembly[] ImportantAssemblies = new[]
        {
            Assembly.Load("System.Runtime"),
            typeof(Object).Assembly,
            typeof(Console).Assembly,
            typeof(ScriptAttribute).Assembly,
            typeof(List<>).Assembly,
            typeof(Enumerable).Assembly
        };


        public ScriptCompiler(ScriptOptions options)
        {
            this.diagnostics = new List<String>();
            this.Options = options;
            var libs = ImportantAssemblies.Concat(options.Imports);
            this.referencesForCodegen = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(e => libs.Contains(e))
            .Distinct()
            .Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();
            //
            this.compilationOptions = this.compilationOptions.WithAllowUnsafe(false);
            this.compilationOptions = this.compilationOptions.WithConcurrentBuild(true);
            this.compilationOptions = this.compilationOptions.WithUsings("System", "System.Reflection", "System.Threading.Tasks");
            this.compilationOptions = this.compilationOptions.WithOptimizationLevel(this.Options.Debug ? OptimizationLevel.Debug : OptimizationLevel.Release);
        }






        private SyntaxTree GlobalUsings(SyntaxTree syntaxTree, params String[] usings)
        {
            // 创建全局 using 指令
            var compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            var directives = usings
                .Select(us => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(us.Trim())))
                .ToArray();
            compilationUnit = compilationUnit.AddUsings(directives);
            Console.WriteLine("===============================================================");
            Console.WriteLine(compilationUnit.NormalizeWhitespace().ToFullString());
            Console.WriteLine("===============================================================");
            // 更新语法树
            return syntaxTree.WithRootAndOptions(compilationUnit, syntaxTree.Options);
        }





        // 加载并编译目录中的所有脚本
        public ScriptEngine LoadScriptsFromDirectory()
        {
            var rootDir = Path.GetFullPath(this.Options.BaseDirectory);
            if (Directory.Exists(Path.Join(rootDir, "obj").ToString())) Directory.Delete(Path.Join(rootDir, "obj").ToString(), true);
            if (Directory.Exists(Path.Join(rootDir, "bin").ToString())) Directory.Delete(Path.Join(rootDir, "bin").ToString(), true);
            var scriptFiles = Directory.GetFiles(rootDir, this.Options.ScriptFilePattern, SearchOption.AllDirectories);
            var parseTasks = scriptFiles.Select(file => ParseSyntaxTree(Path.GetFullPath(file))).ToArray();
            var syntaxTrees = Task.WhenAll(parseTasks).Result;
            var assembly = CompileSyntaxTree(syntaxTrees);
            if (diagnostics.Count > 0)
            {
                foreach (var diagnostic in diagnostics) Console.WriteLine(diagnostic);
            }
            return new ScriptEngine(scriptLoadContext, assembly);
        }


        private async Task<SyntaxTree> ParseSyntaxTree(String filePath)
        {
            var code = await File.ReadAllTextAsync(filePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(text: code, path: filePath, encoding: Encoding.UTF8);
            return GlobalUsings(syntaxTree, baseUsing.Concat(this.Options.Using).ToArray());
        }



        // 使用 Roslyn 编译脚本
        private Assembly CompileSyntaxTree(SyntaxTree[] syntaxTrees)
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: this.Options.Name,
                syntaxTrees: syntaxTrees,
                references: referencesForCodegen,
                options: this.compilationOptions
            );

            // 检查是否有不允许的 API 调用
            var walker = new ForbiddenApiWalker();

            Console.WriteLine("===============================================================");
            Console.WriteLine(compilation.SyntaxTrees.FirstOrDefault()?.GetRoot().ToFullString());
            Console.WriteLine("===============================================================");

            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                walker.VisitWith(semanticModel, syntaxTree.GetRoot());
            }
            if (walker.HasForbiddenApis)
            {
                diagnostics.AddRange(walker.ForbiddenApis);
                return null;
            }
            return emitStream(compilation, this.Options.Debug);
        }






        private Assembly emitStream(CSharpCompilation compilation, Boolean debuging)
        {
            var emitOptions = new EmitOptions();
            MemoryStream pdbStream = null;
            MemoryStream execStream = new MemoryStream();
            if (debuging)
            {
                pdbStream = new MemoryStream();
                emitOptions = emitOptions.WithDebugInformationFormat(DebugInformationFormat.PortablePdb);
            }
            EmitResult result = compilation.Emit(execStream, pdbStream, options: emitOptions);
            if (!result.Success)
            {
                // 处理编译错误
                foreach (var diagnostic in result.Diagnostics)
                {
                    diagnostics.Add(diagnostic.ToString());
                }
                return null;
            }
            execStream.Seek(0, SeekOrigin.Begin);
            if (pdbStream != null) pdbStream.Seek(0, SeekOrigin.Begin);

            Assembly assembly = scriptLoadContext.LoadFromStream(execStream, pdbStream);
            if (pdbStream != null) pdbStream.Dispose();
            execStream.Dispose();
            return assembly;
        }



    }
}