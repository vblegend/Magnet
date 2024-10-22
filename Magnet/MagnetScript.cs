using Magnet.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;




namespace Magnet
{
    public enum ScrriptStatus
    {
        /// <summary>
        /// The script is unavailable because the compilation method has not been executed
        /// </summary>
        NotReady,
        /// <summary>
        /// The script has been compiled
        /// </summary>
        Ready,
        /// <summary>
        /// An error occurred during script compilation
        /// </summary>
        CompileError,
        /// <summary>
        /// Uninstalling a script
        /// </summary>
        Unloading,
        /// <summary>
        /// The script was uninstalled
        /// </summary>
        Unloaded
    }


    public delegate void CompileErrorHandler(MagnetScript magnetScript, ImmutableArray<Diagnostic> diagnostics);

    public delegate void CompileCompleteHandler(MagnetScript magnetScript, Assembly assembly, Stream assemblyStream, Stream pdbStream = null);


    public sealed partial class MagnetScript
    {
        private String[] baseUsing = ["System", "Magnet.Core"];
        public ScriptOptions Options { get; private set; }
        private List<String> diagnostics;
        internal IReadOnlyList<ScriptMetadata> scriptMetaInfos = new List<ScriptMetadata>();
        private readonly WeakReference<Assembly> scriptAssembly = new WeakReference<Assembly>(null);
        public IReadOnlyList<String> Diagnostics => diagnostics;
        private CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        private ScriptLoadContext scriptLoadContext;
        public event Action<MagnetScript> Unloading;
        public event Action<MagnetScript> Unloaded;
        public event CompileErrorHandler CompileError;
        public event CompileCompleteHandler CompileComplete;

        private readonly List<ITypeProcessor> TypeProcessors;
        private readonly Int64 UniqueId = Random.Shared.NextInt64();
        public readonly String Name;
        private readonly Dictionary<Int64, MagnetState> SurvivalStates = new();
        private static GCEventListener gcEventListener = new GCEventListener();
        internal TrackerColllection ReferenceTrackers = new TrackerColllection();
        public ScrriptStatus Status { get; private set; }


        private static readonly Assembly[] ImportantAssemblies = [Assembly.Load("System.Runtime"), Assembly.Load("System.Private.CoreLib"), typeof(ScriptAttribute).Assembly,];


        public Boolean IsAlive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var scriptModules = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in scriptModules)
                {
                    var attrs = assembly.GetCustomAttributes(typeof(ScriptAssemblyAttribute), false);
                    if (attrs.Length == 0) continue;
                    foreach (var attr in attrs)
                    {
                        if (attr is ScriptAssemblyAttribute attribute)
                        {
                            if (attribute.UniqueId == this.UniqueId) return true;
                        }
                    }
                }
                return false;
            }
        }






        /// <summary>
        /// Uninstall the script assembly.
        /// Before performing the Unload method, ensure that all MagnetStates are destroyed and no internal script resource objects are forcibly referenced
        /// The unloading step is asynchronous and the Unloaded event is unloaded after completion
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
                    state = null;
                }
            }
            foreach (var provider in TypeProcessors) provider.Dispose();
            this.TypeProcessors.Clear();
            this.Unloading?.Invoke(this);
            this.Unloading = null;
            this.diagnostics.Clear();
            this.scriptMetaInfos = [];
            this.scriptLoadContext?.Unload();
            this.Status = ScrriptStatus.Unloading;
        }

        private void GcEventListener_OnGCFinalizers()
        {
            ReferenceTrackers.AliveObjects();
            if (this.Status == ScrriptStatus.Unloading && !this.IsAlive)
            {
                gcEventListener.OnGCFinalizers -= GcEventListener_OnGCFinalizers;
                this.Unloaded?.Invoke(this);
                this.scriptLoadContext = null;
                this.Unloaded = null;
                this.Options = null;
                this.Status = ScrriptStatus.Unloaded;
            }
        }

        public MagnetScript(ScriptOptions options)
        {
            this.diagnostics = new List<String>();
            this.Options = options;
            this.Name = options.Name;
            this.Status = ScrriptStatus.NotReady;
            this.TypeProcessors = Options.TypeProcessors;
            this.scriptLoadContext = new ScriptLoadContext(options);
            this.compilationOptions = this.compilationOptions.WithAllowUnsafe(false);
            this.compilationOptions = this.compilationOptions.WithConcurrentBuild(true);
            this.compilationOptions = this.compilationOptions.WithOptimizationLevel((OptimizationLevel)this.Options.Mode);
            this.compilationOptions = this.compilationOptions.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
            this.compilationOptions = this.compilationOptions.WithModuleName(this.Options.Name);
            gcEventListener.OnGCFinalizers += GcEventListener_OnGCFinalizers;
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
            if (this.Status != ScrriptStatus.NotReady && this.Status != ScrriptStatus.CompileError)
            {
                throw new Exception("Wrong state");
            }

            var parseOptions = CSharpParseOptions.Default;
            var symbols = new List<String>();
            if (this.Options.Mode == ScriptRunMode.Debug) symbols.Add("DEBUG");
            if (this.Options.UseDebugger) symbols.Add("USE_DEBUGGER");
            if (this.Options.PreprocessorSymbols != null && this.Options.PreprocessorSymbols.Length > 0) symbols.AddRange(this.Options.PreprocessorSymbols);
            if (symbols.Count > 0)
            {
                parseOptions = parseOptions.WithPreprocessorSymbols(symbols);
            }
            var rootDir = Path.GetFullPath(this.Options.BaseDirectory);
            var scriptFiles = Directory.GetFiles(rootDir, this.Options.ScriptFilePattern, SearchOption.AllDirectories);
            var parseTasks = scriptFiles.Select(file => ParseSyntaxTree(Path.GetFullPath(file), parseOptions)).ToArray();
            var syntaxTrees = new List<SyntaxTree>(Task.WhenAll(parseTasks).Result);
            SyntaxTree assemblyAttribute = CSharpSyntaxTree.ParseText($"[assembly: Magnet.Core.ScriptAssembly({this.UniqueId})]\n[assembly: System.Reflection.AssemblyVersion(\"1.0.0.0\")]");
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

                                                EmitTypeProcessScript(type);
                                                //Console.WriteLine($"Found Script：{type.Name}");
                                                return scriptConfig;
                                            }).ToImmutableList();
                    assembly = null;
                }



            }
            return result;
        }


        private void EmitTypeProcessScript(Type type)
        {
            foreach (var provider in this.TypeProcessors) provider.ProcessScript(type);
        }
        private void EmitTypeProcessAssembly(Assembly assembly)
        {
            foreach (var provider in this.TypeProcessors) provider.ProcessAssembly(assembly);
        }
        private void ParseScriptAutowriredFields(ScriptMetadata metaInfo)
        {
            var fieldList = new List<FieldInfo>();
            var type = metaInfo.ScriptType;
            while (type != null)
            {
                var _fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
                foreach (var fieldInfo in _fields)
                {
                    var attribute = fieldInfo.GetCustomAttribute<AutowiredAttribute>();
                    if (attribute != null)
                    {
                        var autowrired = new AutowriredField();
                        autowrired.FieldInfo = fieldInfo;
                        autowrired.RequiredType = attribute.Type;
                        autowrired.SlotName = attribute.ProviderName;
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
        public MagnetState CreateState(StateOptions options = null)
        {
            if (this.Status != ScrriptStatus.Ready || !this.scriptAssembly.TryGetTarget(out _))
            {
                throw new Exception("A copy of the script is unavailable, uncompiled, or failed to compile");
            }
            if (options == null) options = StateOptions.Default;
            if (options.Identity == -1)
            {
                Int64 identity = -1;
                while (identity == -1 || SurvivalStates.ContainsKey(identity))
                {
                    identity = Random.Shared.NextInt64();
                }

                options.WithIdentity(identity);
            }
            if (SurvivalStates.ContainsKey(options.Identity))
            {
                throw new Exception("The Identity state already exists.");
            }
            var state = new MagnetState(this, options);
            state.Unloading += State_Unloading;
            //this.ReferenceTrackers.Add(state);
            // TODO 性能问题
            SurvivalStates.TryAdd(options.Identity, state);
            return state;
        }

        private void State_Unloading(MagnetState state)
        {
            SurvivalStates.Remove(state.Identity, out var value);
        }

        private async Task<SyntaxTree> ParseSyntaxTree(String filePath, CSharpParseOptions parseOptions)
        {
            var code = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(text: code, options: parseOptions, path: filePath, encoding: Encoding.UTF8);
            return GlobalUsings(syntaxTree, baseUsing.Concat(this.Options.Using).ToArray());
        }



        // 使用 Roslyn 编译脚本
        private EmitResult CompileSyntaxTree(List<SyntaxTree> syntaxTrees)
        {

            var libs = ImportantAssemblies.Concat(Options.References);

            var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(e => !e.IsDynamic && libs.Contains(e))
            .Distinct()
            .Select(a => MetadataReference.CreateFromFile(a.Location));



            var compilation = CSharpCompilation.Create(

                assemblyName: this.Options.Name,
                syntaxTrees: syntaxTrees,
                references: references,
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
                var assembly = scriptLoadContext.LoadFromStream(execStream, pdbStream);
                this.scriptAssembly.SetTarget(assembly);
                this.Status = ScrriptStatus.Ready;
                EmitTypeProcessAssembly(assembly);
                CompileComplete?.Invoke(this, assembly, execStream, pdbStream);
                if (pdbStream != null) pdbStream.Dispose();
                if (execStream != null && !String.IsNullOrEmpty(Options.OutPutFile))
                {
                    execStream.Seek(0, SeekOrigin.Begin);
                    File.WriteAllBytes(Options.OutPutFile, execStream.ToArray());
                }
            }
            else
            {
                this.Status = ScrriptStatus.CompileError;
                CompileError?.Invoke(this, result.Diagnostics);
            }
            execStream.Dispose();
            return result;
        }

    }
}