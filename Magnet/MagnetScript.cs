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
using System.Linq.Expressions;
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
    public delegate void CompileCompleteHandler(MagnetScript magnetScript, Assembly assembly, Stream assemblyStream);

    /// <summary>
    /// Magnet script compiler
    /// </summary>
    public sealed partial class MagnetScript
    {
        private static GCEventListener _gcEventListener = new GCEventListener();
        private static readonly Assembly[] _importantAssemblies = [Assembly.Load("System.Runtime"), Assembly.Load("System.Private.CoreLib"), typeof(ScriptAttribute).Assembly];
        private static readonly String[] _baseUsing = ["System", "Magnet.Core"];

        internal ScriptOptions Options { get; private set; }
        internal IReadOnlyList<ScriptMetaTable> scriptMetaTables = new List<ScriptMetaTable>();
        internal readonly AnalyzerCollection Analyzers;
        internal TrackerColllection ReferenceTrackers = new TrackerColllection();

        private readonly WeakReference<Assembly> _scriptAssembly = new WeakReference<Assembly>(null);
        private CSharpCompilationOptions _compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        private ScriptLoadContext _scriptLoadContext;
        private Dictionary<Int64, MagnetState> _survivalStates = new();

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
                var keys = _survivalStates.Keys;
                foreach (var key in keys)
                {
                    var state = _survivalStates[key];
                    state?.Dispose();
                    state = null;
                }
            }
            this._compilationOptions = null;
            this.Unloading?.Invoke(this);
            this.Unloading = null;
            this.scriptMetaTables = [];
            this._scriptLoadContext?.Unload();
            this.Status = ScrriptStatus.Unloading;
        }

        private void GcEventListener_OnGCFinalizers()
        {
            ReferenceTrackers?.AliveObjects();
            if (this.Status == ScrriptStatus.Unloading && !this.IsAlive)
            {
                _gcEventListener.OnGCFinalizers -= GcEventListener_OnGCFinalizers;
                this.Unloaded?.Invoke(this);
                this.ReferenceTrackers?.Dispose();
                this.ReferenceTrackers = null;
                this._scriptLoadContext = null;
                this.Unloaded = null;
                this.Options = null;
                this.Status = ScrriptStatus.Unloaded;
                this._survivalStates = null;
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
            this._scriptLoadContext = new ScriptLoadContext(options);
            this._compilationOptions = this._compilationOptions.WithAllowUnsafe(this.Options.AllowUnsafe);
            this._compilationOptions = this._compilationOptions.WithConcurrentBuild(true);
            //this.compilationOptions = this.compilationOptions.WithDebugPlusMode(true);
            this._compilationOptions = this._compilationOptions.WithPlatform(this.Options.TargetPlatform);
            this._compilationOptions = this._compilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);
            this._compilationOptions = this._compilationOptions.WithOptimizationLevel((OptimizationLevel)this.Options.Optimization);
            this._compilationOptions = this._compilationOptions.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
            this._compilationOptions = this._compilationOptions.WithModuleName(this.Name);

            //var SetDebugPlusModeDelegate = (Action<CSharpCompilationOptions, bool>)Delegate.CreateDelegate(typeof(Action<CSharpCompilationOptions, bool>), typeof(CSharpCompilationOptions).GetProperty("DebugPlusMode", BindingFlags.Instance | BindingFlags.NonPublic)!.SetMethod!);
            //SetDebugPlusModeDelegate(this.compilationOptions, true);

            this._compilationOptions = this._compilationOptions.WithSpecificDiagnosticOptions(options.diagnosticSeveritys);
            _gcEventListener.OnGCFinalizers += GcEventListener_OnGCFinalizers;
            this.Analyzers = new AnalyzerCollection(Options.Analyzers);

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

            bool needCompile = (this.Options.CompileKind & CompileKind.Compile) == CompileKind.Compile;
            bool needLoadAssembly = (this.Options.CompileKind & CompileKind.LoadAssembly) == CompileKind.LoadAssembly;

            ICompileResult result = null;

            using (MemoryStream _assemblyStream = new MemoryStream())
            {
                if (needCompile)
                {
                    var parseOptions = CSharpParseOptions.Default;
                    var symbols = new List<String>();
                    if (this.Options.Optimization == OptimizationLevel.Debug) symbols.Add("DEBUG");
                    if (this.Options.UseDebugger) symbols.Add("USE_DEBUGGER");
                    if (this.Options.CompileSymbols != null && this.Options.CompileSymbols.Length > 0) symbols.AddRange(this.Options.CompileSymbols);
                    if (symbols.Count > 0)
                    {
                        parseOptions = parseOptions.WithPreprocessorSymbols(symbols);
                    }
                    var rootDir = Path.GetFullPath(this.Options.ScanDirectory);
                    var ScanOptions = this.Options.RecursiveScanning ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var scriptFiles = Directory.GetFiles(rootDir, this.Options.ScriptFilePattern, ScanOptions);
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
                    result = CompileSyntaxTree(syntaxTrees, _assemblyStream, needLoadAssembly);
                    if (result.Success && _assemblyStream != null && !String.IsNullOrEmpty(Options.OutPutFile))
                    {
                        _assemblyStream.Seek(0, SeekOrigin.Begin);
                        File.WriteAllBytes(Options.OutPutFile, _assemblyStream.ToArray());
                    }
                }
                if (needLoadAssembly)
                {
                    if (needCompile)
                    {
                        if (result.Success)
                        {
                            _assemblyStream?.Seek(0, SeekOrigin.Begin);
                            var assembly = _scriptLoadContext.LoadFromStream(_assemblyStream);
                            this.AssemblyLoaded(assembly);
                        }
                    }
                    else
                    {
                        // load assembly from file
                        var assemblyFullPath = Path.GetFullPath(Path.Combine(this.Options.ScanDirectory, this.Options.AssemblyFileName));
                        if (!File.Exists(assemblyFullPath)) throw new Exception($"Assembly file '{assemblyFullPath}' does not exist!");
                        var assembly = _scriptLoadContext.LoadFromAssemblyPath(assemblyFullPath);
                        this.AssemblyLoaded(assembly);
                        result = new CompileResult(true, []);
                    }
                }
            }
            this._compilationOptions = null;
            return result;
        }



        private void PrepareJIT(Assembly assembly)
        {
            var flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic |
              BindingFlags.Public | BindingFlags.Instance |
              BindingFlags.Static;
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(flags))
                {
                    if (method.ContainsGenericParameters ||
                        method.IsGenericMethod ||
                        method.IsGenericMethodDefinition)
                        continue;

                    if ((method.Attributes & MethodAttributes.PinvokeImpl) > 0)
                        continue;
                    if (method.GetMethodBody() == null) continue;
                    //Console.WriteLine(method.Name);
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                }
            }

        }



        private unsafe void AssemblyLoaded(Assembly assembly)
        {
            //this.PrepareJIT(assembly);
            var types = assembly.GetTypes();
            var baseType = typeof(AbstractScript);
            this.scriptMetaTables = types.Where(type => type.IsPublic && !type.IsAbstract && type.IsSubclassOf(baseType) && type.GetCustomAttribute<ScriptAttribute>() != null)
                                    .Select(type =>
                                    {
                                        var generater = ParseGenerateScriptInstanceMethod(type);
                                        var attribute = type.GetCustomAttribute<ScriptAttribute>();
                                        var exportMethods = ParseExportMethods(type);
                                        var autowriredFields = ParseAutowriredFields(type);
                                        var scriptConfig = new ScriptMetaTable(type, String.IsNullOrEmpty(attribute.Alias) ? type.Name : attribute.Alias, generater, exportMethods, autowriredFields);
                                        this.Analyzers.DefineType(type);
                                        //Console.WriteLine($"Found Script：{type.Name}");
                                        return scriptConfig;
                                    }).ToImmutableList();

            this._scriptAssembly.SetTarget(assembly);
            this.Status = ScrriptStatus.Loaded;
            this.Analyzers.ConnectTo(this);
            this.Analyzers.DefineAssembly(assembly);
        }



        private unsafe delegate*<AbstractScript> ParseGenerateScriptInstanceMethod(Type scriptType)
        {
            var generateMethod = scriptType.GetMethod(IdentifierDefine.GENERATE_SCRIPT_INSTANCE_METHOD, BindingFlags.Static | BindingFlags.NonPublic);
            IntPtr pointer = generateMethod.MethodHandle.GetFunctionPointer();
            var methodPointer = (delegate*<AbstractScript>)pointer;
            // 预热
            {
                RuntimeHelpers.PrepareMethod(generateMethod.MethodHandle);
                methodPointer();
            }
            return methodPointer;
        }
 
        private List<AutowriredField> ParseAutowriredFields(Type scriptClassType)
        {
            var fieldList = new List<AutowriredField>();
            var type = scriptClassType;
            while (type != null)
            {
                var _fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
                foreach (var fieldInfo in _fields)
                {
                    var attribute = fieldInfo.GetCustomAttribute<AutowiredAttribute>();
                    if (attribute != null)
                    {
                        var setter = TypeUtils.CreateFieldSetter(fieldInfo);
                        var autowrired = new AutowriredField(fieldInfo, setter, attribute.Type, attribute.ProviderName);
                        fieldList.Add(autowrired);
                    }
                }
                type = type.BaseType;
            }
            return fieldList;
        }

        private Dictionary<String, ExportMethod> ParseExportMethods(Type scriptClassType)
        {
            var methods = new Dictionary<String, ExportMethod>();
            var type = scriptClassType;
            while (type != null)
            {
                var _methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var methodInfo in _methods)
                {
                    var attribute = methodInfo.GetCustomAttribute<FunctionAttribute>();
                    if (attribute != null)
                    {
                        var exportMethod = new ExportMethod(methodInfo, String.IsNullOrEmpty(attribute.Alias) ? methodInfo.Name : attribute.Alias);
                        methods.Add(exportMethod.Alias, exportMethod);
                    }
                }
                type = type.BaseType;
            }
            return methods;
        }

        /// <summary>
        /// Create a script state machine
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IMagnetState CreateState(StateOptions options = null)
        {
            if (this.Status != ScrriptStatus.Loaded || !this._scriptAssembly.TryGetTarget(out _))
            {
                throw new Exception("A copy of the script is unavailable, uncompiled, or failed to compile");
            }
            if (options == null) options = StateOptions.Default;
            if (options.Identity == -1)
            {
                Int64 identity = -1;
                while (identity == -1 || _survivalStates.ContainsKey(identity))
                {
                    identity = Random.Shared.NextInt64();
                }

                options.WithIdentity(identity);
            }
            if (_survivalStates.ContainsKey(options.Identity))
            {
                throw new Exception("The Identity state already exists.");
            }
            var state = new MagnetState(this, options);
            state.Unloading += State_Unloading;
            //this.ReferenceTrackers.Add(state);
            // TODO 性能问题
            _survivalStates.TryAdd(options.Identity, state);
            return state;
        }

        private void State_Unloading(MagnetState state)
        {
            _survivalStates.Remove(state.Identity, out var value);
        }

        private async Task<SyntaxTree> ParseSyntaxTree(String filePath, CSharpParseOptions parseOptions)
        {
            var code = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(text: code, options: parseOptions, path: filePath, encoding: Encoding.UTF8);
            return GlobalUsings(syntaxTree, _baseUsing.Concat(this.Options.Using).ToArray());
        }



        private Task<SyntaxTree> SyntaxTreeTypeRewrite(CSharpCompilation compilation, SyntaxTree syntaxTree, TypeResolver typeResolver)
        {
            return Task.Factory.StartNew(() =>
            {
                SyntaxTreeRewriter rewriter = new SyntaxTreeRewriter(typeResolver);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var newRoot = rewriter.VisitWith(semanticModel, syntaxTree.GetRoot());
                return CSharpSyntaxTree.Create((CSharpSyntaxNode)newRoot, (CSharpParseOptions)syntaxTree.Options, syntaxTree.FilePath, Encoding.UTF8);
            });
        }

        private Task<List<Diagnostic>> SyntaxTreeTypeCheck(CSharpCompilation compilation, SyntaxTree syntaxTree)
        {
            return Task.Factory.StartNew(() =>
             {
                 SyntaxTreeWalker walker = new SyntaxTreeWalker(this.Options);
                 var semanticModel = compilation.GetSemanticModel(syntaxTree);
                 walker.VisitWith(semanticModel, syntaxTree.GetRoot());
                 return walker.Diagnostics;
             });
        }


        // 使用 Roslyn 编译脚本
        private ICompileResult CompileSyntaxTree(List<SyntaxTree> syntaxTrees, Stream _assemblyStream, Boolean loadAssembly)
        {
            var libs = _importantAssemblies.Concat(Options.References);
            var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(e => !e.IsDynamic && libs.Contains(e))
            .Distinct()
            .Select(a => MetadataReference.CreateFromFile(a.Location));

            var compilation = CSharpCompilation.Create(
                assemblyName: this.Name,
                syntaxTrees: syntaxTrees,
                references: references,
                options: this._compilationOptions
            );

            var typeResolver = new TypeResolver(this.Options);
            if (typeResolver.IsCanRewrite)
            {
                // 类型替换
                var newTrees = syntaxTrees.Select(syntaxTree => SyntaxTreeTypeRewrite(compilation, syntaxTree, typeResolver)).ToArray();
                var reWriteTrees = new List<SyntaxTree>(Task.WhenAll(newTrees).Result);
                //替换语法树
                for (int i = 1; i < syntaxTrees.Count; i++)
                {
                    compilation = compilation.ReplaceSyntaxTree(syntaxTrees[i], reWriteTrees[i]);
                    syntaxTrees[i] = reWriteTrees[i];
                }
            }
            // 类型检查，跳过第一个语法树（程序集属性定义）
            var diagnosticsTasks = syntaxTrees.Skip(1).Select(syntaxTree => SyntaxTreeTypeCheck(compilation, syntaxTree)).ToArray();
            var diagnostics = new List<Diagnostic>(Task.WhenAll(diagnosticsTasks).Result.SelectMany(e => e));

            // 如果有错误诊断定为编译失败。
            if (diagnostics.Any(e => e.Severity == DiagnosticSeverity.Error))
            {
                return new CompileResult(false, diagnostics);
            }

            //Console.WriteLine("===============================================================");
            //Console.WriteLine(compilation.SyntaxTrees.FirstOrDefault()?.GetRoot().ToFullString());
            //Console.WriteLine("===============================================================");
            //var result = emitStream(compilation, assemblyStream, pdbStream);

            var emitOptions = new EmitOptions();
            if (this.Options.Optimization == OptimizationLevel.Debug)
            {
                emitOptions = emitOptions.WithDebugInformationFormat(DebugInformationFormat.Embedded);
                emitOptions = emitOptions.WithIncludePrivateMembers(true);
                //emitOptions = emitOptions.WithRuntimeMetadataVersion("1.2.3");
                //emitOptions = emitOptions.WithSubsystemVersion(SubsystemVersion.Create(11, 11));
                //emitOptions = emitOptions.WithTolerateErrors(true);
            }
            var result = compilation.Emit(_assemblyStream, options: emitOptions);
            if (result.Success)
            {
                this.Status = ScrriptStatus.CompileComplete;
                this.CompileComplete?.Invoke(this, null, _assemblyStream);
            }
            else
            {
                this.Status = ScrriptStatus.CompileError;
                this.CompileError?.Invoke(this, result.Diagnostics);
            }
            diagnostics.AddRange(result.Diagnostics.Where(e => e.Severity != DiagnosticSeverity.Hidden));
            return new CompileResult(result.Success, diagnostics);
        }



        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{this.Name}:{this.UniqueId}]";
        }

    }
}