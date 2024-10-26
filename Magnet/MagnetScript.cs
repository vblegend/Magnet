using Magnet.Analysis;
using Magnet.Core;
using Magnet.Syntax;
using Magnet.Tracker;
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
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;




namespace Magnet
{
    /// <summary>
    /// MagnetScript status
    /// </summary>
    public enum ScrriptStatus
    {
        /// <summary>
        /// The script is unavailable because the compilation method has not been executed
        /// </summary>
        NotReady,
        /// <summary>
        /// The script has been loaded
        /// </summary>
        Loaded,

        /// <summary>
        /// The script has been compiled
        /// </summary>
        CompileComplete,

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

    /// <summary>
    /// Compile errors callback delegate
    /// </summary>
    /// <param name="magnetScript"></param>
    /// <param name="diagnostics"></param>
    public delegate void CompileErrorHandler(MagnetScript magnetScript, ImmutableArray<Diagnostic> diagnostics);


    /// <summary>
    /// Compile complete callback delegate
    /// </summary>
    /// <param name="magnetScript"></param>
    /// <param name="assembly"></param>
    /// <param name="assemblyStream"></param>
    /// <param name="pdbStream"></param>
    public delegate void CompileCompleteHandler(MagnetScript magnetScript, Assembly assembly, Stream assemblyStream, Stream pdbStream = null);

    /// <summary>
    /// Magnet script compiler
    /// </summary>
    public sealed partial class MagnetScript
    {
        private String[] baseUsing = ["System", "Magnet.Core"];
        internal ScriptOptions Options { get; private set; }

        internal IReadOnlyList<ScriptMetadata> scriptMetaInfos = new List<ScriptMetadata>();
        private readonly WeakReference<Assembly> scriptAssembly = new WeakReference<Assembly>(null);
        private CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        private ScriptLoadContext scriptLoadContext;
        internal readonly AnalyzerCollection Analyzers;

        private readonly Dictionary<Int64, MagnetState> SurvivalStates = new();
        private static GCEventListener gcEventListener = new GCEventListener();
        internal TrackerColllection ReferenceTrackers = new TrackerColllection();
        private static readonly Assembly[] ImportantAssemblies = [Assembly.Load("System.Runtime"), Assembly.Load("System.Private.CoreLib"), typeof(ScriptAttribute).Assembly];

        private MemoryStream assemblyStream = null;
        private MemoryStream pdbStream = null;

        /// <summary>
        /// The unique ID of the assembly compiled by the script
        /// </summary>
        public readonly Int64 UniqueId = Random.Shared.NextInt64();

        /// <summary>
        /// The name of the script may not be unique, but UniqueId is
        /// </summary>
        public readonly String Name;


        /// <summary>
        /// Current status of the Magnet script
        /// </summary>
        public ScrriptStatus Status { get; private set; }

        /// <summary>
        /// Received a request to uninstall the script
        /// </summary>
        public event Action<MagnetScript> Unloading;

        /// <summary>
        /// The script has been unmounted from memory
        /// </summary>
        public event Action<MagnetScript> Unloaded;

        /// <summary>
        /// Script compilation error
        /// </summary>
        public event CompileErrorHandler CompileError;

        /// <summary>
        /// Script compilation completed
        /// </summary>
        public event CompileCompleteHandler CompileComplete;


        /// <summary>
        /// Whether the script still resides in memory
        /// </summary>
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
        public void Unload(Boolean force = false)
        {
            this.Analyzers?.Disconnect(this);
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
            this.Unloading?.Invoke(this);
            this.Unloading = null;
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public MagnetScript(ScriptOptions options)
        {
            this.Options = options;
            this.Name = options.Name;
            this.Status = ScrriptStatus.NotReady;
            this.scriptLoadContext = new ScriptLoadContext(options);
            this.compilationOptions = this.compilationOptions.WithAllowUnsafe(this.Options.AllowUnsafe);
            this.compilationOptions = this.compilationOptions.WithConcurrentBuild(true);
            //this.compilationOptions = this.compilationOptions.WithDebugPlusMode(true);
            this.compilationOptions = this.compilationOptions.WithPlatform(this.Options.TargetPlatform);
            this.compilationOptions = this.compilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);
            this.compilationOptions = this.compilationOptions.WithOptimizationLevel((OptimizationLevel)this.Options.Mode);
            this.compilationOptions = this.compilationOptions.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
            this.compilationOptions = this.compilationOptions.WithModuleName(this.Name);

            //var SetDebugPlusModeDelegate = (Action<CSharpCompilationOptions, bool>)Delegate.CreateDelegate(typeof(Action<CSharpCompilationOptions, bool>), typeof(CSharpCompilationOptions).GetProperty("DebugPlusMode", BindingFlags.Instance | BindingFlags.NonPublic)!.SetMethod!);
            //SetDebugPlusModeDelegate(this.compilationOptions, true);


            Dictionary<string, ReportDiagnostic> values = new Dictionary<string, ReportDiagnostic>();
            foreach (var sd in options.suppressDiagnostics)
            {
                values[sd] = ReportDiagnostic.Suppress;
            }
            this.compilationOptions = this.compilationOptions.WithSpecificDiagnosticOptions(values);
            gcEventListener.OnGCFinalizers += GcEventListener_OnGCFinalizers;
            this.Analyzers = new AnalyzerCollection(Options.Analyzers);
            this.assemblyStream = new MemoryStream();
            this.pdbStream = (this.Options.Mode == ScriptRunMode.Debug) ? new MemoryStream() : null;
        }

        private CompilationUnitSyntax AddUsingStatement(CompilationUnitSyntax root, string name)
        {
            if (root.Usings.Any(u => u.Name.ToString() == name))
            {
                // using statement already exists.
                return root;
            }
            UsingDirectiveSyntax usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(name));
            var rootCompilation = root.AddUsings(usingDirective);
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
        public ICompileResult Compile()
        {
            if (this.Status != ScrriptStatus.NotReady && this.Status != ScrriptStatus.CompileError)
            {
                throw new Exception("Wrong state");
            }

            bool canCompile = (this.Options.CompileKind & CompileKind.Compile) == CompileKind.Compile;
            bool canLoadAssembly = (this.Options.CompileKind & CompileKind.LoadAssembly) == CompileKind.LoadAssembly;
            ICompileResult result = null;
            if (canCompile)
            {

                var parseOptions = CSharpParseOptions.Default;
                var symbols = new List<String>();
                if (this.Options.Mode == ScriptRunMode.Debug) symbols.Add("DEBUG");
                if (this.Options.UseDebugger) symbols.Add("USE_DEBUGGER");
                if (this.Options.CompileSymbols != null && this.Options.CompileSymbols.Length > 0) symbols.AddRange(this.Options.CompileSymbols);
                if (symbols.Count > 0)
                {
                    parseOptions = parseOptions.WithPreprocessorSymbols(symbols);
                }
                var rootDir = Path.GetFullPath(this.Options.ScanDirectory);
                var scriptFiles = Directory.GetFiles(rootDir, this.Options.ScriptFilePattern, SearchOption.AllDirectories);
                var parseTasks = scriptFiles.Select(file => ParseSyntaxTree(Path.GetFullPath(file), parseOptions)).ToArray();
                var syntaxTrees = new List<SyntaxTree>(Task.WhenAll(parseTasks).Result);
                var assemblyInfo = $"[assembly: System.Reflection.AssemblyTitle(\"{this.Name}\")]\n[assembly: Magnet.Core.ScriptAssembly({this.UniqueId})]\n[assembly: System.Reflection.AssemblyVersion(\"1.0.0.0\")]\n[assembly: System.Reflection.AssemblyFileVersion(\"1.0.0.0\")]";
                var targetFrameworkAttribute = (TargetFrameworkAttribute)typeof(Assembly).Assembly.GetCustomAttribute(typeof(TargetFrameworkAttribute));
                if (targetFrameworkAttribute != null)
                {
                    assemblyInfo += $"\n[assembly: System.Runtime.Versioning.TargetFramework(\"{targetFrameworkAttribute.FrameworkName}\", FrameworkDisplayName = \"{targetFrameworkAttribute.FrameworkDisplayName}\")]";
                }
                SyntaxTree assemblyAttribute = CSharpSyntaxTree.ParseText(assemblyInfo);
                syntaxTrees.Insert(0, assemblyAttribute);
                this.assemblyStream = new MemoryStream();
                this.pdbStream = (this.Options.Mode == ScriptRunMode.Debug) ? new MemoryStream() : null;
                result = CompileSyntaxTree(syntaxTrees, canLoadAssembly);
                if (result.Success && assemblyStream != null && !String.IsNullOrEmpty(Options.OutPutFile))
                {
                    this.assemblyStream.Seek(0, SeekOrigin.Begin);
                    File.WriteAllBytes(Options.OutPutFile, assemblyStream.ToArray());
                }
            }
            if (canLoadAssembly)
            {
                if (canCompile)
                {
                    if (result.Success)
                    {
                        this.assemblyStream?.Seek(0, SeekOrigin.Begin);
                        this.pdbStream?.Seek(0, SeekOrigin.Begin);
                        var assembly = scriptLoadContext.LoadFromStream(assemblyStream, pdbStream);
                        this.AssemblyLoaded(assembly);
                    }
                }
                else
                {
                    var assemblyFullPath = Path.GetFullPath(Path.Combine(this.Options.ScanDirectory, this.Options.AssemblyFileName));
                    if (!File.Exists(assemblyFullPath)) throw new Exception($"Assembly file '{assemblyFullPath}' does not exist!");
                    var assembly = scriptLoadContext.LoadFromAssemblyPath(assemblyFullPath);
                    this.AssemblyLoaded(assembly);
                    result = new CompileResult(true, []);
                }
            }
            assemblyStream?.Dispose();
            pdbStream?.Dispose();
            return result;
        }

        private void AssemblyLoaded(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var baseType = typeof(AbstractScript);
            this.scriptMetaInfos = types.Where(type => type.IsPublic && !type.IsAbstract && type.IsSubclassOf(baseType) && type.GetCustomAttribute<ScriptAttribute>() != null)
                                    .Select(type =>
                                    {
                                        var attribute = type.GetCustomAttribute<ScriptAttribute>();
                                        var scriptConfig = new ScriptMetadata(type, String.IsNullOrEmpty(attribute.Alias) ? type.Name : attribute.Alias);
                                        ParseScriptAutowriredFields(scriptConfig);
                                        ParseScriptMethods(scriptConfig);
                                        this.Analyzers.DefineType(type);
                                        //Console.WriteLine($"Found Script：{type.Name}");
                                        return scriptConfig;
                                    }).ToImmutableList();

            this.scriptAssembly.SetTarget(assembly);
            this.Status = ScrriptStatus.Loaded;
            this.Analyzers.ConnectTo(this);
            this.Analyzers.DefineAssembly(assembly);
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
                        var autowrired = new AutowriredField(fieldInfo, attribute.Type, attribute.ProviderName);
                        metaInfo.AddAutowriredField(autowrired);
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
                        var exportMethod = new ScriptExportMethod(methodInfo, String.IsNullOrEmpty(attribute.Alias) ? methodInfo.Name : attribute.Alias);
                        metaInfo.AddExportMethod(exportMethod.Alias, exportMethod);
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
        public IMagnetState CreateState(StateOptions options = null)
        {
            if (this.Status != ScrriptStatus.Loaded || !this.scriptAssembly.TryGetTarget(out _))
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
        private ICompileResult CompileSyntaxTree(List<SyntaxTree> syntaxTrees, Boolean loadAssembly)
        {
            var libs = ImportantAssemblies.Concat(Options.References);
            var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(e => !e.IsDynamic && libs.Contains(e))
            .Distinct()
            .Select(a => MetadataReference.CreateFromFile(a.Location));

            var compilation = CSharpCompilation.Create(
                assemblyName: this.Name,
                syntaxTrees: syntaxTrees,
                references: references,
                options: this.compilationOptions
            );

            // 检查是否有不允许的 API 调用
            var diagnostics = new List<Diagnostic>();
            var typeResolver = new TypeResolver(this.Options);
            var walker = new SyntaxTreeWalker(this.Options);
            var rewriter = new SyntaxTreeRewriter(typeResolver);
            // 从1开始 跳过程序集属性定义脚本树
            for (int i = 1; i < syntaxTrees.Count; i++)
            {
                var syntaxTree = syntaxTrees[i];
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                // rewriter
                var newRoot = rewriter.VisitWith(semanticModel, syntaxTree.GetRoot());
                var newSyntaxTree = newRoot.SyntaxTree;
                if (String.IsNullOrEmpty(newSyntaxTree.FilePath))
                {
                    newSyntaxTree = CSharpSyntaxTree.Create((CSharpSyntaxNode)newRoot, (CSharpParseOptions)syntaxTree.Options, syntaxTree.FilePath, Encoding.UTF8);
                }
                compilation = compilation.ReplaceSyntaxTree(syntaxTree, newSyntaxTree);
                semanticModel = compilation.GetSemanticModel(newSyntaxTree);
                // walker
                walker.VisitWith(semanticModel, newSyntaxTree.GetRoot());
            }
            if (walker.Diagnostics.Count > 0)
            {
                diagnostics.AddRange(walker.Diagnostics.Where(e => e.Severity != DiagnosticSeverity.Hidden));
            }
            if (diagnostics.Any(e => e.Severity == DiagnosticSeverity.Error))
            {
                return new CompileResult(false, diagnostics);
            }

            //Console.WriteLine("===============================================================");
            //Console.WriteLine(compilation.SyntaxTrees.FirstOrDefault()?.GetRoot().ToFullString());
            //Console.WriteLine("===============================================================");
            //var result = emitStream(compilation, assemblyStream, pdbStream);

            var emitOptions = new EmitOptions();
            if (this.Options.Mode == ScriptRunMode.Debug)
            {
                emitOptions = emitOptions.WithDebugInformationFormat(DebugInformationFormat.PortablePdb);
            }
            var result = compilation.Emit(assemblyStream, pdbStream, options: emitOptions);
            if (result.Success)
            {
                this.Status = ScrriptStatus.CompileComplete;
                this.CompileComplete?.Invoke(this, null, assemblyStream, pdbStream);
            }
            else
            {
                this.Status = ScrriptStatus.CompileError;
                this.CompileError?.Invoke(this, result.Diagnostics);
            }
            diagnostics.AddRange(result.Diagnostics.Where(e => e.Severity != DiagnosticSeverity.Hidden));
            return new CompileResult(result.Success, diagnostics);
        }


    }
}