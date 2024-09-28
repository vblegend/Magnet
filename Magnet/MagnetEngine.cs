using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Text;
using Magnet.Context;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;



namespace Magnet
{
    public class MagnetEngine
    {
        private String[] baseUsing = new String[] { "System", "Magnet.Context", "System.Threading" };
        public ScriptOptions Options { get; private set; }

        public PortableExecutableReference[] referencesForCodegen = [];

        private List<String> diagnostics;

        private IReadOnlyList<ScriptMetaInfo> scriptMetaInfos = new List<ScriptMetaInfo>();

        private Assembly scriptAssembly;


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
            typeof(Enumerable).Assembly,
        };


        public MagnetEngine(ScriptOptions options)
        {
            this.diagnostics = new List<String>();
            this.Options = options;
            var libs = ImportantAssemblies.Concat(options.References);
            this.referencesForCodegen = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(e => libs.Contains(e))
            .Distinct()
            .Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();
            //
            this.compilationOptions = this.compilationOptions.WithAllowUnsafe(false);
            this.compilationOptions = this.compilationOptions.WithConcurrentBuild(true);
            this.compilationOptions = this.compilationOptions.WithOptimizationLevel((OptimizationLevel)this.Options.Mode);
            scriptLoadContext.Unloading += ScriptLoadContext_Unloading;
        }

        private void ScriptLoadContext_Unloading(System.Runtime.Loader.AssemblyLoadContext obj)
        {
            Console.WriteLine("脚本已被卸载...");
        }

        public CompilationUnitSyntax AddUsingStatement(CompilationUnitSyntax root, string name)
        {
            CompilationUnitSyntax rootCompilation = root;
            if (rootCompilation.Usings.Any(u => u.Name.ToString() == name))
            {
                // using statement already exists.
                return root;
            }

            UsingDirectiveSyntax usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(name));
            rootCompilation = rootCompilation.AddUsings(usingDirective);
            return rootCompilation;
        }

        private SyntaxTree GlobalUsings(SyntaxTree syntaxTree, params String[] usings)
        {
            // 创建全局 using 指令
            var compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            var directives = usings.Select(MakeUsingDirective);
            foreach (var _using in usings)
            {
                compilationUnit = AddUsingStatement(compilationUnit, _using);
            }

            //Console.WriteLine("===============================================================");
            //Console.WriteLine(compilationUnit.NormalizeWhitespace().ToFullString());
            //Console.WriteLine("===============================================================");
            // 更新语法树
            return syntaxTree.WithRootAndOptions(compilationUnit, syntaxTree.Options);
        }


        private UsingDirectiveSyntax MakeUsingDirective(string usingName)
        {
            var names = usingName.Split('.');
            NameSyntax nameSyntax = SyntaxFactory.IdentifierName(names[0]);

            for (var i = 1; i < names.Length; i++)
            {
                if (string.IsNullOrEmpty(names[i]))
                    continue;

                nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, SyntaxFactory.IdentifierName(names[i]));
            }
            return SyntaxFactory.UsingDirective(nameSyntax);
        }


        // 加载并编译目录中的所有脚本
        public EmitResult Compile()
        {
            var rootDir = Path.GetFullPath(this.Options.BaseDirectory);
            var scriptFiles = Directory.GetFiles(rootDir, this.Options.ScriptFilePattern, SearchOption.AllDirectories);
            var parseTasks = scriptFiles.Select(file => ParseSyntaxTree(Path.GetFullPath(file))).ToArray();
            var syntaxTrees = Task.WhenAll(parseTasks).Result;
            var result = CompileSyntaxTree(syntaxTrees);
            if (result.Success)
            {
                var types = this.scriptAssembly.GetTypes();
                var baseType = typeof(BaseScript);
                this.scriptMetaInfos = types.Where(type => type.IsPublic && !type.IsAbstract && type.IsSubclassOf(baseType) && type.GetCustomAttribute<ScriptAttribute>() != null)
                                        .Select(type =>
                                        {
                                            var attribute = type.GetCustomAttribute<ScriptAttribute>();
                                            if (String.IsNullOrEmpty(attribute.Name)) attribute.Name = type.Name;
                                            return new ScriptMetaInfo(attribute, type);
                                        }).ToImmutableList();
            }
            return result;
        }


        public MagnetState CreateScriptState()
        {
            if (this.scriptAssembly == null) throw new Exception("没有可用的脚本程序集");
            ScriptCollection scriptCollection = new ScriptCollection();
            foreach (var meta in this.scriptMetaInfos)
            {
                var instance = (BaseScript)Activator.CreateInstance(meta.Type);
                if (instance is IScriptInstance scriptInstance) scriptInstance.InjectedContext(scriptCollection);
                scriptCollection.Add(meta.Attribute, instance);
            }
            return new MagnetState(scriptCollection);
        }





        public void Unload()
        {
            if (this.scriptLoadContext != null)
            {
                this.scriptLoadContext.Unload();
                this.scriptLoadContext = null;
            }
            if (this.scriptAssembly != null)
            {
                this.scriptAssembly = null;
            }
        }







        private async Task<SyntaxTree> ParseSyntaxTree(String filePath)
        {
            var code = await File.ReadAllTextAsync(filePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(text: code, path: filePath, encoding: Encoding.UTF8);
            return GlobalUsings(syntaxTree, baseUsing.Concat(this.Options.Using).ToArray());
        }



        // 使用 Roslyn 编译脚本
        private EmitResult CompileSyntaxTree(SyntaxTree[] syntaxTrees)
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: this.Options.Name,
                syntaxTrees: syntaxTrees,
                references: referencesForCodegen,
                options: this.compilationOptions
            );

            // 检查是否有不允许的 API 调用
            var walker = new ForbiddenApiWalker();

            //Console.WriteLine("===============================================================");
            //Console.WriteLine(compilation.SyntaxTrees.FirstOrDefault()?.GetRoot().ToFullString());
            //Console.WriteLine("===============================================================");

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
            return emitStream(compilation);
        }






        private EmitResult emitStream(CSharpCompilation compilation)
        {
            var emitOptions = new EmitOptions();
            MemoryStream pdbStream = null;
            MemoryStream execStream = new MemoryStream();
            if (this.Options.Mode == ScriptRunMode.Debug)
            {
                pdbStream = new MemoryStream();
                emitOptions = emitOptions.WithDebugInformationFormat(DebugInformationFormat.PortablePdb);
            }
            EmitResult result = compilation.Emit(execStream, pdbStream, options: emitOptions);
            if (result.Success)
            {
                execStream.Seek(0, SeekOrigin.Begin);
                if (pdbStream != null) pdbStream.Seek(0, SeekOrigin.Begin);
                this.scriptAssembly = scriptLoadContext.LoadFromStream(execStream, pdbStream);
            }
            if (pdbStream != null) pdbStream.Dispose();
            execStream.Dispose();
            return result;
        }



    }
}