using Magnet.Analysis;
using Magnet.Core;
using Magnet.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.AccessControl;

namespace Magnet
{
    /// <summary>
    /// script assembly loads the callback delegate
    /// </summary>
    /// <param name="context"></param>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public delegate Assembly AssemblyLoadDelegate(AssemblyLoadContext context, AssemblyName assemblyName);

    internal class ObjectProvider
    {
        public Type Type;
        public String SlotName;
        public Object Instance;
    }

    /// <summary>
    /// script compile kind
    /// </summary>
    [Flags]
    public enum CompileKind
    {
        /// <summary>
        /// Compile only scripts and output assembly files
        /// </summary>
        Compile = 1,
        /// <summary>
        /// Load the script from the assembly file
        /// </summary>
        LoadAssembly = 2,
        /// <summary>
        /// Compile the script file from the script directory and load it
        /// </summary>
        CompileAndLoadAssembly = Compile | LoadAssembly,

    }



    /// <summary>
    /// Script customization options
    /// </summary>
    public class ScriptOptions
    {
        /// <summary>
        /// Provides basic scripting options
        /// </summary>
        public static ScriptOptions Default
        {
            get
            {
                return new ScriptOptions();
            }
        }

        /// <summary>
        /// Provides a secure scripting option that disables the use of some thread, reflection, GC, process, file, socket, and other types
        /// </summary>
        public static ScriptOptions Safety
        {
            get
            {
                return new ScriptOptions().DisableInsecureTypes();
            }
        }

        internal Boolean JustInTimePrewarming { get; private set; } = true;

        internal String Name { get; private set; } = "Magnet.Script";
        internal String OutPutFile { get; private set; }
        internal OptimizationLevel Optimization { get; private set; } = OptimizationLevel.Release;
        internal String ScanDirectory { get; private set; }
        internal Boolean RecursiveScanning { get; private set; }
        internal String ScriptFilePattern { get; private set; } = "*.cs";
        internal AssemblyLoadDelegate AssemblyLoad { get; private set; }
        internal List<String> Using { get; private set; } = [];
        internal List<Assembly> References { get; private set; } = [];
        internal Boolean UseDebugger { get; private set; }
        internal Boolean AllowAsync { get; private set; } = false;

        internal CompileKind CompileKind { get; private set; } = CompileKind.CompileAndLoadAssembly;

        internal Platform TargetPlatform { get; private set; } = Platform.AnyCpu;

        internal String AssemblyFileName { get; private set; } = null;

        internal Boolean AllowUnsafe { get; private set; } = false;

        internal IOutput Output { get; private set; } = new ConsoleOutput();
        internal readonly List<IAnalyzer> Analyzers = new List<IAnalyzer>();

        internal readonly HashSet<String> DisabledNamespaces = new HashSet<String>();

        internal readonly HashSet<String> DisabledTypes = new HashSet<String>();

        internal String[] CompileSymbols { get; private set; } = [];
        internal readonly Dictionary<String, String> ReplaceTypes = new Dictionary<string, string>();

        internal readonly Dictionary<String, ReportDiagnostic> diagnosticSeveritys = new();
        internal ITypeRewriter typeRewriter;

        /// <summary>
        /// Set the script type rewriter, which is used to replace or check the type of a script.
        /// When ReplaceType fails, this object method is called to rewrite the type
        /// </summary>
        /// <param name="typeRewriter"></param>
        /// <returns></returns>
        public ScriptOptions WithTypeRewriter(ITypeRewriter typeRewriter)
        {
            this.typeRewriter = typeRewriter;
            return this;
        }


        /// <summary>
        /// Set the script compilation kind option(Default: CompileAndLoadAssembly)
        /// <code>
        /// //#1 仅编译，可输出
        /// options.WithCompileKind(CompileKind.Compile);
        /// options.WithOutPutFile("123.dll");
        /// //#2 从程序集文件加载
        /// options.WithCompileKind(CompileKind.LoadAssembly);
        /// options.WithScanDirectory("./");
        /// options.WithAssemblyFileName("123.dll");
        /// //#3 从脚本文件编译并加载
        /// options.WithCompileKind(CompileKind.CompileAndLoadAssembly);
        /// options.WithScanDirectory("../../../../Scripts");
        /// </code>
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public ScriptOptions WithCompileKind(CompileKind kind)
        {
            this.CompileKind = kind;
            return this;
        }


        /// <summary>
        /// Set the target platform for script compilation(Default: AnyCpu)
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public ScriptOptions WithTargetPlatform(Platform platform)
        {
            this.TargetPlatform = platform;
            return this;
        }

        /// <summary>
        /// Whether to allow unsafe code(default: false)
        /// </summary>
        /// <param name="allowed"></param>
        /// <returns></returns>
        public ScriptOptions WithAllowUnsafe(Boolean allowed)
        {
            this.AllowUnsafe = allowed;
            return this;
        }



        /// <summary>
        /// Add compiler diagnostic Suppress
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reportDiagnostic"></param>
        /// <returns></returns>
        public ScriptOptions AddDiagnosticSuppress(String code, ReportDiagnostic reportDiagnostic = ReportDiagnostic.Suppress)
        {
            diagnosticSeveritys.Add(code, reportDiagnostic);
            return this;
        }


        /// <summary>
        /// Disable the exact type
        /// </summary>
        /// <param name="_namespace"></param>
        /// <returns></returns>
        public ScriptOptions DisableType(String _namespace)
        {
            this.DisabledTypes.Add(_namespace);
            return this;
        }

        /// <summary>
        /// Disable the type where the type resides
        /// </summary>
        /// <param name="disabledType"></param>
        /// <returns></returns>
        public ScriptOptions DisableType(Type disabledType)
        {
            this.DisabledTypes.Add(disabledType.FullName);
            return this;
        }

        /// <summary>
        /// Disable the type where the type resides
        /// </summary>
        /// <param name="disabledType"></param>
        /// <returns></returns>
        public ScriptOptions DisableGenericBaseType(Type disabledType)
        {
            var baseTypes = disabledType.FullName.Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries);
            this.DisabledTypes.Add(baseTypes[0]);
            return this;
        }

        /// <summary>
        /// Disable the type where the type resides
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ScriptOptions DisableType<T>()
        {
            this.DisabledTypes.Add(typeof(T).FullName);
            return this;
        }



        /// <summary>
        /// Disable the exact namespace
        /// </summary>
        /// <param name="_namespace"></param>
        /// <returns></returns>
        public ScriptOptions DisableNamespace(String _namespace)
        {
            this.DisabledNamespaces.Add(_namespace);
            return this;
        }

        /// <summary>
        /// Disable the namespace where the type resides
        /// </summary>
        /// <param name="disabledType"></param>
        /// <returns></returns>
        public ScriptOptions DisableNamespace(Type disabledType)
        {
            this.DisabledNamespaces.Add(disabledType.Namespace);
            return this;
        }
        /// <summary>
        /// Disable the namespace where the type resides
        /// </summary>
        /// <returns></returns>
        public ScriptOptions DisableNamespace<T>()
        {
            this.DisabledNamespaces.Add(typeof(T).Namespace);
            return this;
        }

        /// <summary>
        /// Set the output stream of the script
        /// </summary>
        /// <param name="messagePrinter"></param>
        /// <returns></returns>
        public ScriptOptions WithOutput(IOutput messagePrinter)
        {
            this.Output = messagePrinter;
            return this;
        }

        /// <summary>
        /// Sets the script's assembly load interceptor
        /// </summary>
        /// <param name="assemblyLoad"></param>
        /// <returns></returns>
        public ScriptOptions WithAssemblyLoadCallback(AssemblyLoadDelegate assemblyLoad)
        {
            this.AssemblyLoad = assemblyLoad;
            return this;
        }


        /// <summary>
        /// Add a script type analyzer <br/>
        /// The analyzer contains events such as assembly loading, script type loading, script object creation, and so on
        /// </summary>
        /// <param name="analyzer"></param>
        /// <returns></returns>
        public ScriptOptions AddAnalyzer(IAnalyzer analyzer)
        {
            this.Analyzers.Add(analyzer);
            return this;
        }

        /// <summary>
        /// Add a default using reference for the script
        /// </summary>
        /// <param name="nameSpaces"></param>
        /// <returns></returns>
        public ScriptOptions AddUsings(params String[] nameSpaces)
        {
            foreach (var name in nameSpaces)
            {
                this.Using.Add(name);
            }
            return this;
        }

        internal List<ObjectProvider> Providers { get; private set; } = [];


        /// <summary>
        /// Register the state global provider(injected object)
        /// <code>
        /// //#HOST
        /// options.RegisterProvider&lt;ObjectKilledContext&gt;(new ObjectKilledContext());
        /// options.RegisterProvider(GLOBAL);
        /// options.RegisterProvider&lt;IObjectContext&gt;(new HumContext(), "SELF");
        /// 
        /// //#IN SCRIPTS
        /// [Autowired]
        /// protected readonly GlobalVariableStore GLOBAL;
        /// 
        /// [Autowired("SELF")]
        /// protected readonly IObjectContext Player;
        /// 
        /// [Autowired]
        /// private readonly ITimerManager timerManager;
        /// </code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="slotName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ScriptOptions RegisterProvider<T>(T value, String slotName = null)
        {
            var type = typeof(T);
            foreach (var item in Providers)
            {
                if (((slotName == null && item.SlotName == null) || (slotName == item.SlotName)) && (Object)value == item.Instance) throw new InvalidOperationException();
            }
            var _object = new ObjectProvider() { Type = type, Instance = value, SlotName = slotName };
            if (String.IsNullOrWhiteSpace(slotName))
            {
                Providers.Add(_object);
            }
            else
            {
                Providers.Insert(0, _object);
            }
            return this;
        }




        /// <summary>
        /// To replace the specified type in the script as the new type, you must use the full type name with namespace
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public ScriptOptions AddReplaceType(String sourceType, String newType)
        {
            ReplaceTypes.Add(TypeUtils.CleanTypeName(sourceType), TypeUtils.CleanTypeName(newType));
            return this;
        }

        /// <summary>
        /// To replace the specified type in the script as the new type, you must use the full type name with namespace
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public ScriptOptions AddReplaceType(String sourceType, Type newType)
        {
            ReplaceTypes.Add(TypeUtils.CleanTypeName(sourceType), TypeUtils.CleanTypeName(newType));
            return this;
        }

        /// <summary>
        /// To replace the specified type in the script as the new type, you must use the full type name with namespace
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public ScriptOptions AddReplaceType(Type sourceType, String newType)
        {
            ReplaceTypes.Add(TypeUtils.CleanTypeName(sourceType), TypeUtils.CleanTypeName(newType));
            return this;
        }


        /// <summary>
        /// To replace the specified type in the script as the new type
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public ScriptOptions AddReplaceType(Type sourceType, Type newType)
        {
            ReplaceTypes.Add(TypeUtils.CleanTypeName(sourceType), TypeUtils.CleanTypeName(newType));
            return this;
        }



        /// <summary>
        /// Adds a reference to the assembly in which the generic type resides to the script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ScriptOptions AddReferences<T>() where T : class
        {
            this.References.Add(typeof(T).Assembly);
            return this;
        }



        /// <summary>
        /// Adds a reference to the assembly to the script
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public ScriptOptions AddReferences(params Assembly[] assemblies)
        {
            this.References.AddRange(assemblies);
            return this;
        }

        /// <summary>
        /// A reference that adds the type's owning assembly to the script
        /// </summary>
        /// <param name="typeOfAssembles"></param>
        /// <returns></returns>
        public ScriptOptions AddReferences(params Type[] typeOfAssembles)
        {
            foreach (var type in typeOfAssembles)
            {
                this.References.Add(type.Assembly);
            }
            return this;
        }


        /// <summary>
        /// Adds an assembly reference with the specified name to the script
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public ScriptOptions AddReferences(params String[] assemblies)
        {
            foreach (var name in assemblies)
            {
                this.References.Add(Assembly.Load(name));
            }
            return this;
        }



        /// <summary>
        /// The output file name of the script assembly is not output if it is empty
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScriptOptions WithOutPutFile(String name)
        {
            this.OutPutFile = name;
            return this;
        }

        /// <summary>
        /// set Script name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScriptOptions WithName(String name)
        {
            this.Name = name;
            return this;
        }

        /// <summary>
        /// Whether to enable the asynchronous function
        /// </summary>
        /// <param name="allowAsync"></param>
        /// <returns></returns>
        public ScriptOptions WithAllowAsync(Boolean allowAsync)
        {
            this.AllowAsync = allowAsync;
            return this;
        }


        /// <summary>
        /// Set the file name extension of the script. The default is ".cs"
        /// </summary>
        /// <param name="filePattern"></param>
        /// <returns></returns>
        public ScriptOptions WithFilePattern(String filePattern)
        {
            this.ScriptFilePattern = filePattern;
            return this;
        }

        /// <summary>
        /// Set the scan directory of the script or the directory where the input assembly resides
        /// <code>
        /// ScriptOptions options = ScriptOptions.Default;
        /// // #1 从程序集文件加载
        /// options.WithCompileKind(CompileKind.LoadAssembly);
        /// options.WithScanDirectory("./");
        /// options.WithAssemblyFileName("123.dll");
        /// // #2 从脚本文件编译并加载
        /// options.WithCompileKind(CompileKind.CompileAndLoadAssembly);
        /// options.WithScanDirectory("../../../../Scripts");
        /// </code>
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="isRecursiveScanning">是否递归扫描子目录</param>
        /// <returns></returns>
        public ScriptOptions WithScanDirectory(String baseDirectory, Boolean isRecursiveScanning = false)
        {
            this.ScanDirectory = baseDirectory;
            this.RecursiveScanning = isRecursiveScanning;
            return this;
        }

        /// <summary>
        /// Whether to recursively scan all self-directories in the “ScanDirectory” directory when compiling the script
        /// </summary>
        /// <param name="isRecursiveScanning">是否递归扫描子目录</param>
        /// <returns></returns>
        public ScriptOptions WithRecursiveScanning(Boolean isRecursiveScanning = false)
        {
            this.RecursiveScanning = isRecursiveScanning;
            return this;
        }


        /// <summary>
        /// input assembly name（Only used in LoadAssembly）
        /// <code>        
        /// ScriptOptions options = ScriptOptions.Default;
        /// options.WithCompileKind(CompileKind.LoadAssembly);
        /// options.WithScanDirectory("./");
        /// options.WithAssemblyFileName("123.dll");
        /// </code>
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public ScriptOptions WithAssemblyFileName(String assemblyPath)
        {
            this.AssemblyFileName = assemblyPath;
            return this;
        }



        /// <summary>
        /// SYMBOL that declares the compiler for  and the #if macro definition within the script<br/>
        /// built-in [DEBUG, USE_DEBUGGER]
        /// <code>
        /// [Conditional("SYMBOL")]
        /// 
        /// #if SYMBOL
        /// #endif
        /// </code>
        /// </summary>
        /// <param name="preprocessorSymbols"></param>
        /// <returns></returns>
        public ScriptOptions WithCompileSymbols(params string[] preprocessorSymbols)
        {
            this.CompileSymbols = preprocessorSymbols;
            return this;
        }


        /// <summary>
        /// Whether to compile the debug version
        /// </summary>
        /// <param name="useDebuggerBreak"></param>
        /// <returns></returns>
        public ScriptOptions WithDebug(Boolean useDebuggerBreak = true)
        {
            this.Optimization = OptimizationLevel.Debug;
            this.UseDebugger = useDebuggerBreak;
            return this;
        }

        /// <summary>
        /// Whether to compile the release version
        /// </summary>
        /// <param name="useDebuggerBreak"></param>
        /// <returns></returns>
        public ScriptOptions WithRelease(Boolean useDebuggerBreak = false)
        {
            this.Optimization = OptimizationLevel.Release;
            this.UseDebugger = useDebuggerBreak;
            return this;
        }





        /// <summary>
        /// Use the default suppression diagnostics
        /// </summary>
        /// <returns></returns>
        public ScriptOptions UseDefaultSuppressDiagnostics()
        {
            diagnosticSeveritys.Add("CA1050", ReportDiagnostic.Suppress);
            diagnosticSeveritys.Add("CA1822", ReportDiagnostic.Suppress);
            diagnosticSeveritys.Add("CS1701", ReportDiagnostic.Suppress);
            diagnosticSeveritys.Add("CS1702", ReportDiagnostic.Suppress);
            diagnosticSeveritys.Add("CS1705", ReportDiagnostic.Suppress);
            diagnosticSeveritys.Add("CS2008", ReportDiagnostic.Suppress);
            diagnosticSeveritys.Add("CS8019", ReportDiagnostic.Suppress);


            diagnosticSeveritys.Add("CS0067", ReportDiagnostic.Suppress); // 从不使用事件
            diagnosticSeveritys.Add("CS0169", ReportDiagnostic.Suppress); // 从不使用字段



            diagnosticSeveritys.Add("CS8632", ReportDiagnostic.Suppress);// nullable
            diagnosticSeveritys.Add("CS162", ReportDiagnostic.Suppress); //- Unreachable code detected.
            diagnosticSeveritys.Add("CS0219", ReportDiagnostic.Suppress);// - The variable 'V' is assigned but its value is never used.
            diagnosticSeveritys.Add("CS0414", ReportDiagnostic.Suppress);// - The private field 'F' is assigned but its value is never used.
            diagnosticSeveritys.Add("CS0616", ReportDiagnostic.Suppress);// - Member is obsolete.
            diagnosticSeveritys.Add("CS0649", ReportDiagnostic.Suppress);// - Field 'F' is never assigned to, and will always have its default value.
            diagnosticSeveritys.Add("CS0693", ReportDiagnostic.Suppress);// - Type parameter 'type parameter' has the same name as the type parameter from outer type 'T'
            diagnosticSeveritys.Add("CS1591", ReportDiagnostic.Suppress);// - Missing XML comment for publicly visible type or member 'Type_or_Member'
            diagnosticSeveritys.Add("CS1998", ReportDiagnostic.Suppress);// - This async method lacks 'await' operators and will run synchronously
            return this;
        }





        /// <summary>
        /// Disable some insecure script types
        /// </summary>
        /// <returns></returns>
        public ScriptOptions DisableInsecureTypes()
        {
            this.DisableType(typeof(System.Environment));
            this.DisableType(typeof(System.GC));
            this.DisableType(typeof(System.AppDomain));
            this.DisableType(typeof(System.Activator));
            this.DisableType(typeof(System.Type));


            // Threading
            this.DisableNamespace("System.Threading");
            this.DisableNamespace("System.Threading.Tasks");

            // code safe
            this.DisableNamespace("System.Reflection");
            this.DisableNamespace("System.Reflection.Emit");
            this.DisableNamespace("System.Linq.Expressions");

            //

            this.DisableNamespace("System.Diagnostics");
            this.DisableNamespace("System.Runtime.InteropServices");

            // IO
            this.DisableNamespace("System.IO");

            // NET
            this.DisableType("System.Net.WebClient");

            this.DisableNamespace("System.Net.Sockets");
            this.DisableNamespace("System.Net.Http");
            this.DisableNamespace("System.Net.Mail");
            //
            this.DisableNamespace("System.Runtime.InteropServices");
            this.DisableNamespace("System.Runtime.CompilerServices");

            //script internal
            this.DisableType(typeof(IScriptInstance));
            this.DisableType(typeof(ScriptMetadata));
            this.DisableType(typeof(AutowriredField));
            this.DisableType(typeof(ScriptExportMethod));





            return this;
        }







    }
}
