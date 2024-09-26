using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Text;
using Magnet.Context;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Magnet
{
    public class ScriptManager
    {

        private Assembly assembly = null;
        public ScriptOptions Options { get; private set; }


        public ScriptManager(ScriptOptions options)
        {
            this.Options = options;
        }






        private SyntaxTree GlobalUsings(SyntaxTree syntaxTree, params String[] usings)
        {
            // 创建全局 using 指令
            var compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            foreach (var us in usings)
            {
                // 添加全局 using 指令到语法树
                var globalUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(us));
                compilationUnit = compilationUnit.AddUsings(globalUsing);
            }
            // 更新语法树
            return syntaxTree.WithRootAndOptions(compilationUnit, syntaxTree.Options);
        }





        // 加载并编译目录中的所有脚本
        public void LoadScriptsFromDirectory(string scriptDirectory)
        {
            var rootDir = Path.GetFullPath(scriptDirectory);
            var scriptFiles = Directory.GetFiles(rootDir, "*.cs");
            SyntaxTree[] syntaxTrees = scriptFiles
                                .Select(file =>
                                {
                                    var filePath = Path.GetFullPath(file);
                                    var code = File.ReadAllText(filePath);
                                    var syntaxTree = CSharpSyntaxTree.ParseText(text: code, path: filePath, encoding: Encoding.UTF8);
                                    return GlobalUsings(syntaxTree, "System", "Magnet.Context");
                                })
                                .ToArray();
            assembly = CompileScript(syntaxTrees);
        }




        // 使用 Roslyn 编译脚本
        private Assembly CompileScript(SyntaxTree[] syntaxTrees)
        {
            var refs = new List<String>()
            {
                "System.Runtime.dll",
                "System.Console.dll",
                "System.Private.CoreLib.dll",
                "System.Linq.dll",
                "System.Collections.dll",
                "Magnet.Context.dll"
            };



            // 引入所需的程序集引用，包括 System.Console
            var references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Where(a =>
                {
                    var fileName = Path.GetFileName(a.Location);
                    return refs.Contains(fileName);
                })
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();
            references.Add(MetadataReference.CreateFromFile(typeof(ScriptAttribute).Assembly.Location));
            //
            var RuntimeDll = typeof(object).Assembly.Location;
            var ConsoleDll = typeof(Console).Assembly.Location;
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);


            options = options.WithAllowUnsafe(false);
            options = options.WithConcurrentBuild(true);
            //options = options.WithUsings("System", "System.Reflection");
            if (this.Options.Debug)
            {
                options = options.WithOptimizationLevel(OptimizationLevel.Debug);
            }
            else
            {
                options = options.WithOptimizationLevel(OptimizationLevel.Release);
            }

            var compilation = CSharpCompilation.Create(
                assemblyName: this.Options.Name,
                syntaxTrees: syntaxTrees,
                references: references,
                options: options
            );

            // 检查是否有不允许的 API 调用
            var walker = new ForbiddenApiWalker();
            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                walker.VisitWith(semanticModel, syntaxTree.GetRoot());
            }

            if (walker.HasForbiddenApis)
            {
                foreach (var item in walker.ForbiddenApis)
                {
                    Console.WriteLine(item);
                }
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
                    Console.WriteLine(diagnostic.ToString());
                }
                return null;
            }
            execStream.Seek(0, SeekOrigin.Begin);
            if (pdbStream != null) pdbStream.Seek(0, SeekOrigin.Begin);
            var context = new ScriptLoadContext();
            Assembly assembly = context.LoadFromStream(execStream, pdbStream);
            if (pdbStream != null) pdbStream.Dispose();
            execStream.Dispose();

            return assembly;
        }





        // 运行指定脚本中的方法
        public object RunScriptMethod(string scriptFile, string className, string methodName, params object[] parameters)
        {
            var type = assembly.GetType(className);
            if (type != null)
            {
                var method = type.GetMethod(methodName);
                if (method != null)
                {
                    var instance = Activator.CreateInstance(type); // 创建类实例
                    return method.Invoke(instance, parameters); // 调用方法
                }
            }
            throw new Exception("Variable not found");
        }

        public object GetScriptVariable(string scriptFile, string className, string variableName)
        {
            var type = assembly.GetType(className);
            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                var field = type.GetField(variableName);
                return field?.GetValue(instance);
            }
            throw new Exception("Variable not found");
        }

        public void SetScriptVariable(string scriptFile, string className, string variableName, object value)
        {
            var type = assembly.GetType(className);
            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                var field = type.GetField(variableName);
                field?.SetValue(instance, value);
            }
        }
    }
}