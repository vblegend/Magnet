using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Text;
using Magnet.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;




namespace Magnet
{
    public sealed partial class MagnetScript
    {
        private String[] baseUsing = ["System", "Magnet.Core"];
        public ScriptOptions Options { get; private set; }
        private PortableExecutableReference[] referencesForCodegen = [];
        private List<String> diagnostics;
        internal IReadOnlyList<ScriptMetadata> scriptMetaInfos = new List<ScriptMetadata>();
        private WeakReference<Assembly> scriptAssembly = new WeakReference<Assembly>(null);
        public IReadOnlyList<String> Diagnostics => diagnostics;
        private CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        private ScriptLoadContext scriptLoadContext;
        public event Action<MagnetScript> Unloading;
        public event Action<MagnetScript> Unloaded;
        public String Name => Options.Name;
        private readonly Dictionary<Int64, MagnetState> SurvivalStates = new( 65535);



        private static readonly Assembly[] ImportantAssemblies =
        [
            Assembly.Load("System.Runtime"),
            Assembly.Load("System.Private.CoreLib"),
            typeof(ScriptAttribute).Assembly,
        ];


        /// <summary>
        /// Uninstall the script assembly.
        /// Before performing the Unload method, ensure that all MagnetStates are destroyed and no internal script resource objects are forcibly referenced
        /// </summary>
        /// <param name="force">Force destruction of all MagnetState instances</param>
        /// <exception cref="ScriptUnloadFailureException">The script states is not destroyed, or resources inside the script are externally referenced</exception>
        public void Unload(Boolean force = false)
        {
 
            if (force)
            {
                var keys = SurvivalStates.Keys;
                foreach (var key in keys)
                {
                    var state = SurvivalStates[key];
                    state?.Dispose();
                }
            }
            this.Unloading?.Invoke(this);
            this.Unloading = null;
            this.diagnostics.Clear();
            this.scriptMetaInfos = [];
            this.referencesForCodegen = [];
            this.scriptLoadContext?.Unload();
            // 触发垃圾回收

            var count = 5;
            while (count > 0 && Exists(this.Name))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                count--;
            }
            if (Exists(this.Name))
            {
                throw new ScriptUnloadFailureException();
            }
            //this.scriptAssembly(null);
            RemoveCache(this);
            // TODO 检测程序集被卸载后
            this.Unloaded?.Invoke(this);
            this.scriptLoadContext = null;
            this.Unloaded = null;
            this.Options = null;
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
            this.compilationOptions = this.compilationOptions.WithModuleName(this.Options.Name);

        }

        private CompilationUnitSyntax AddUsingStatement(CompilationUnitSyntax root, string name)
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


        /// <summary>
        /// Load and compile all scripts in the directory
        /// </summary>
        /// <returns></returns>
        public EmitResult Compile()
        {
            var rootDir = Path.GetFullPath(this.Options.BaseDirectory);
            var scriptFiles = Directory.GetFiles(rootDir, this.Options.ScriptFilePattern, SearchOption.AllDirectories);
            var parseTasks = scriptFiles.Select(file => ParseSyntaxTree(Path.GetFullPath(file))).ToArray();
            var syntaxTrees = new List<SyntaxTree>(Task.WhenAll(parseTasks).Result);
            SyntaxTree assemblyAttribute = CSharpSyntaxTree.ParseText("[assembly: Magnet.Core.ScriptAssembly()]");
            syntaxTrees.Insert(0, assemblyAttribute);
            var result = CompileSyntaxTree(syntaxTrees);
            if (result.Success)
            {
                if (this.scriptAssembly.TryGetTarget(out var assembly))
                {
                    var types = assembly.GetTypes();
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
                                                //Console.WriteLine($"Found Script：{type.Name}");
                                                return scriptConfig;
                                            }).ToImmutableList();

                    assembly = null;
                }



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
                        autowrired.SlotName = attribute.SlotName;
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




        /// <summary>
        /// Create a script state machine
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public MagnetState CreateState(Int64 identity = -1)
        {
            if (!this.scriptAssembly.TryGetTarget(out _))
            {
                throw new Exception("A copy of the script is unavailable, uncompiled, or failed to compile");
            }
            if (identity == -1)
            {
                while (identity == -1 || SurvivalStates.ContainsKey(identity))
                {
                    identity = Random.Shared.NextInt64();
                }
            }
            if (SurvivalStates.ContainsKey(identity))
            {
                throw new Exception("The Identity state already exists.");
            }
            var state = new MagnetState(this, identity);
            state.Unloading += State_Unloading;
            // TODO 性能问题
            SurvivalStates.TryAdd(identity, state);
            return state;
        }

        private void State_Unloading(MagnetState state)
        {
            SurvivalStates.Remove(state.Identity, out var value);
        }

        private async Task<SyntaxTree> ParseSyntaxTree(String filePath)
        {
            var code = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(text: code, path: filePath, encoding: Encoding.UTF8);
            return GlobalUsings(syntaxTree, baseUsing.Concat(this.Options.Using).ToArray());
        }



        // 使用 Roslyn 编译脚本
        private EmitResult CompileSyntaxTree(List<SyntaxTree> syntaxTrees)
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
            for (int i = 0; i < syntaxTrees.Count; i++)
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
                if (Exists(this.Name)) throw new ScriptExistException(this.Name);
                this.scriptAssembly.SetTarget(scriptLoadContext.LoadFromStream(execStream, pdbStream));
                AddCache(this);
            }
            if (pdbStream != null) pdbStream.Dispose();
            if (execStream != null && !String.IsNullOrEmpty(Options.OutPutFile))
            {
                execStream.Seek(0, SeekOrigin.Begin);
                File.WriteAllBytes(Options.OutPutFile, execStream.ToArray());
            }
            execStream.Dispose();
            return result;
        }


    }
}