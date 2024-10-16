using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Text;
using Magnet.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Collections.Generic;



namespace Magnet
{
    public class MagnetScript
    {
        private String[] baseUsing = ["System", "Magnet.Core"];
        public ScriptOptions Options { get; private set; }

        public PortableExecutableReference[] referencesForCodegen = [];

        private List<String> diagnostics;

        internal IReadOnlyList<ScriptMetadata> scriptMetaInfos = new List<ScriptMetadata>();

        private Assembly scriptAssembly;


        public IReadOnlyList<String> Diagnostics => diagnostics;

        private CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        private ScriptLoadContext scriptLoadContext;

        public event Action<MagnetScript> Unloading;


        public static readonly Assembly[] ImportantAssemblies =
        [
            Assembly.Load("System.Runtime"),
            Assembly.Load("System.Private.CoreLib"),
            typeof(ScriptAttribute).Assembly,
        ];


        public void Unload()
        {
            if (this.Unloading != null)
            {
                this.Unloading.Invoke(this);
                this.Unloading = null;
            }
            if (this.scriptLoadContext != null)
            {
                this.scriptLoadContext.Unload();
                this.scriptLoadContext = null;
            }
            if (this.scriptAssembly != null)
            {
                this.scriptAssembly = null;
            }
            this.referencesForCodegen = Array.Empty<PortableExecutableReference>();
            this.diagnostics.Clear();
            this.scriptMetaInfos = Array.Empty<ScriptMetadata>();
        }









        public MagnetScript(ScriptOptions options)
        {
            this.diagnostics = new List<String>();
            this.Options = options;
            this.scriptLoadContext = new ScriptLoadContext(options);
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
            this.compilationOptions = this.compilationOptions.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
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
                var baseType = typeof(AbstractScript);
                this.scriptMetaInfos = types.Where(type => type.IsPublic && !type.IsAbstract && type.IsSubclassOf(baseType) && type.GetCustomAttribute<ScriptAttribute>() != null)
                                        .Select(type =>
                                        {
                                            var attribute = type.GetCustomAttribute<ScriptAttribute>();
                                            var scriptConfig = new ScriptMetadata();
                                            scriptConfig.ScriptType = type;
                                            scriptConfig.ScriptAlias = String.IsNullOrEmpty(attribute.Alias) ? type.Name : attribute.Alias;

                                            ParseScriptAutowriredFields(scriptConfig);
                                            ParseScriptMethods(scriptConfig);
                                            Console.WriteLine($"Found Script：{type.Name}");
                                            return scriptConfig;
                                        }).ToImmutableList();
            }
            return result;
        }




        private void ParseScriptAutowriredFields(ScriptMetadata metaInfo)
        {
            var fieldList = new List<FieldInfo>();
            var type = metaInfo.ScriptType;
            while (type != null)
            {
                var _fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var fieldInfo in _fields)
                {
                    var attribute = fieldInfo.GetCustomAttribute<AutowiredAttribute>();
                    if (attribute != null)
                    {
                        var autowrired = new AutowriredField();
                        autowrired.FieldInfo = fieldInfo;
                        autowrired.RequiredType = attribute.Type;
                        autowrired.Alias = String.IsNullOrEmpty(attribute.Alias) ? fieldInfo.Name : attribute.Alias;
                        metaInfo.AutowriredFields.Add(autowrired);
                    }
                }

                type = type.BaseType;
            }

        }

        private void ParseScriptMethods(ScriptMetadata metaInfo)
        {
            var fieldList = new List<MethodInfo>();
            var type = metaInfo.ScriptType;
            while (type != null)
            {
                var _methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var methodInfo in _methods)
                {
                    var attribute = methodInfo.GetCustomAttribute<FunctionAttribute>();
                    if (attribute != null)
                    {
                        var exportMethod = new ScriptExportMethod();
                        exportMethod.MethodInfo = methodInfo;
                        exportMethod.Alias = String.IsNullOrEmpty(attribute.Alias) ? methodInfo.Name : attribute.Alias;
   
                        metaInfo.ExportMethods.Add(exportMethod.Alias, exportMethod);
                    }
                }
                type = type.BaseType;
            }

        }






        public MagnetState CreateScriptState()
        {
            if (this.scriptAssembly == null) throw new Exception("没有可用的脚本程序集");
            return new MagnetState(this);
        }











        private async Task<SyntaxTree> ParseSyntaxTree(String filePath)
        {
            var code = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
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
            var walker = new ForbiddenApiWalker(this.Options);
            var rewriter = new SyntaxTreeRewriter(this.Options.ReplaceTypes);
            for (int i = 0; i < syntaxTrees.Length; i++)
            {
                var syntaxTree = syntaxTrees[i];
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var newRoot = rewriter.VisitWith(semanticModel, syntaxTree.GetRoot());
                var newSyntaxTree = newRoot.SyntaxTree;
                if (String.IsNullOrEmpty(newSyntaxTree.FilePath))
                {
                    newSyntaxTree = CSharpSyntaxTree.Create((CSharpSyntaxNode)newRoot, (CSharpParseOptions)syntaxTree.Options, syntaxTree.FilePath, Encoding.UTF8);
                }
                compilation = compilation.ReplaceSyntaxTree(syntaxTree, newSyntaxTree);
                // walker
                semanticModel = compilation.GetSemanticModel(newSyntaxTree);
                walker.VisitWith(semanticModel, newSyntaxTree.GetRoot());
            }

            //Console.WriteLine("===============================================================");
            //Console.WriteLine(compilation.SyntaxTrees.FirstOrDefault()?.GetRoot().ToFullString());
            //Console.WriteLine("===============================================================");

            foreach (var tree in compilation.SyntaxTrees)
            {
                if (string.IsNullOrEmpty(tree.FilePath))
                {
                    //throw new InvalidOperationException("SyntaxTree 文件路径为空");
                }
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