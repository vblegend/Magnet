using Magnet.Analysis;
using Magnet.Core;
using Magnet.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

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
                return new ScriptOptions().DisabledInsecureTypes();
            }
        }



        internal String Name { get; private set; } = "Magnet.Script";
        internal String OutPutFile { get; private set; }
        internal ScriptRunMode Mode { get; private set; } = ScriptRunMode.Release;
        internal String BaseDirectory { get; private set; }
        internal String ScriptFilePattern { get; private set; } = "*.cs";
        internal AssemblyLoadDelegate AssemblyLoad { get; private set; }
        internal List<String> Using { get; private set; } = [];
        internal List<Assembly> References { get; private set; } = [];
        internal Boolean UseDebugger { get; private set; }
        internal Boolean AllowAsync { get; private set; } = false;
        internal IOutput Output { get; private set; } = new ConsoleOutput();
        internal readonly List<IAnalyzer> Analyzers = new List<IAnalyzer>();


        internal readonly List<String> DisabledNamespace = new List<String>();

        internal String[] PreprocessorSymbols { get; private set; } = [];
        internal readonly Dictionary<String, String> ReplaceTypes = new Dictionary<string, string>();

        internal List<String> suppressDiagnostics = new List<string>();
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
        /// Add compiler suppress diagnostic 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ScriptOptions AddSuppressDiagnostic(String code)
        {
            suppressDiagnostics.Add(code);
            return this;
        }


        /// <summary>
        /// Disable the exact namespace
        /// </summary>
        /// <param name="_namespace"></param>
        /// <returns></returns>
        public ScriptOptions DisableNamespace(String _namespace)
        {
            if (!this.DisabledNamespace.Contains(_namespace))
            {
                this.DisabledNamespace.Add(_namespace);
            }
            return this;
        }

        /// <summary>
        /// Disable the namespace where the type resides
        /// </summary>
        /// <param name="disabledType"></param>
        /// <returns></returns>
        public ScriptOptions DisableNamespace(Type disabledType)
        {
            if (!this.DisabledNamespace.Contains(disabledType.Namespace))
            {
                this.DisabledNamespace.Add(disabledType.Namespace);
            }
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
        /// Add a script type processor
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
            ReplaceTypes.Add(sourceType, newType);
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
            ReplaceTypes.Add(sourceType.FullName, newType.FullName);
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
        /// Set the scan directory for the script
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public ScriptOptions WithDirectory(String baseDirectory)
        {
            this.BaseDirectory = baseDirectory;
            return this;
        }

        /// <summary>
        /// A SYMBOL that declares the compiler for [Conditional("SYMBOL")] and the #if macro definition within the script
        /// </summary>
        /// <param name="preprocessorSymbols"></param>
        /// <returns></returns>
        public ScriptOptions WithPreprocessorSymbols(params string[] preprocessorSymbols)
        {
            this.PreprocessorSymbols = preprocessorSymbols;
            return this;
        }


        /// <summary>
        /// Whether to compile the debug version
        /// </summary>
        /// <param name="useDebuggerBreak"></param>
        /// <returns></returns>
        public ScriptOptions WithDebug(Boolean useDebuggerBreak = true)
        {
            this.Mode = ScriptRunMode.Debug;
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
            this.Mode = ScriptRunMode.Release;
            this.UseDebugger = useDebuggerBreak;
            return this;
        }





        /// <summary>
        /// Use the default suppression diagnostics
        /// </summary>
        /// <returns></returns>
        public ScriptOptions UseDefaultSuppressDiagnostics()
        {
            suppressDiagnostics.Add("CA1050");
            suppressDiagnostics.Add("CA1822");
            suppressDiagnostics.Add("CS1701");
            suppressDiagnostics.Add("CS1702");
            suppressDiagnostics.Add("CS1705");
            suppressDiagnostics.Add("CS2008");
            suppressDiagnostics.Add("CS8019");
            suppressDiagnostics.Add("CS162"); //- Unreachable code detected.
            suppressDiagnostics.Add("CS0219");// - The variable 'V' is assigned but its value is never used.
            suppressDiagnostics.Add("CS0414");// - The private field 'F' is assigned but its value is never used.
            suppressDiagnostics.Add("CS0616");// - Member is obsolete.
            suppressDiagnostics.Add("CS0649");// - Field 'F' is never assigned to, and will always have its default value.
            suppressDiagnostics.Add("CS0693");// - Type parameter 'type parameter' has the same name as the type parameter from outer type 'T'
            suppressDiagnostics.Add("CS1591");// - Missing XML comment for publicly visible type or member 'Type_or_Member'
            suppressDiagnostics.Add("CS1998");// - This async method lacks 'await' operators and will run synchronously
            return this;
        }





        /// <summary>
        /// Disable some insecure script types
        /// </summary>
        /// <returns></returns>
        public ScriptOptions DisabledInsecureTypes()
        {

            this.AddReplaceType(typeof(System.GC), typeof(Magnet.Safety.GC));
            this.AddReplaceType(typeof(System.Threading.Thread), typeof(Magnet.Safety.Thread));
            this.AddReplaceType(typeof(System.Threading.ThreadPool), typeof(Magnet.Safety.ThreadPool));
            this.AddReplaceType(typeof(System.Threading.Tasks.Task), typeof(Magnet.Safety.Task));
            this.AddReplaceType(typeof(System.AppDomain), typeof(Magnet.Safety.AppDomain));


            // code safe
            this.AddReplaceType(typeof(System.Activator), typeof(Magnet.Safety.Activator));
            this.AddReplaceType(typeof(System.Type), typeof(Magnet.Safety.Type));
            this.AddReplaceType(typeof(System.Reflection.Assembly), typeof(Magnet.Safety.Assembly));
            this.AddReplaceType(typeof(System.Reflection.Emit.DynamicMethod), typeof(Magnet.Safety.DynamicMethod));
            this.AddReplaceType(typeof(System.Linq.Expressions.DynamicExpression), typeof(Magnet.Safety.DynamicMethod));
            this.AddReplaceType(typeof(System.Linq.Expressions.Expression), typeof(Magnet.Safety.DynamicMethod));
            this.AddReplaceType(typeof(System.Runtime.CompilerServices.CallSite), typeof(Magnet.Safety.DynamicMethod));

            //
            this.AddReplaceType(typeof(System.Environment), typeof(Magnet.Safety.Environment));
            this.AddReplaceType(typeof(System.Diagnostics.Process), typeof(Magnet.Safety.Process));
            this.AddReplaceType(typeof(System.Runtime.InteropServices.Marshal), typeof(Magnet.Safety.Marshal));

            // IO
            this.AddReplaceType(typeof(System.IO.File), typeof(Magnet.Safety.File));
            this.AddReplaceType(typeof(System.IO.Directory), typeof(Magnet.Safety.Directory));
            this.AddReplaceType(typeof(System.IO.FileStream), typeof(Magnet.Safety.FileStream));
            this.AddReplaceType(typeof(System.IO.StreamWriter), typeof(Magnet.Safety.StreamWriter));
            this.AddReplaceType(typeof(System.IO.StreamReader), typeof(Magnet.Safety.StreamReader));



            // NET
            this.AddReplaceType(typeof(System.Net.Sockets.Socket), typeof(Magnet.Safety.Socket));
            this.AddReplaceType(typeof(System.Net.WebClient), typeof(Magnet.Safety.WebClient));
            this.AddReplaceType(typeof(System.Net.Http.HttpClient), typeof(Magnet.Safety.HttpClient));


            //
            this.AddReplaceType(typeof(System.Runtime.InteropServices.DllImportAttribute), typeof(Magnet.Safety.DllImportAttribute));
            this.AddReplaceType(typeof(System.Runtime.CompilerServices.ModuleInitializerAttribute), typeof(Magnet.Safety.ModuleInitializerAttribute));
            this.AddReplaceType(typeof(System.Runtime.InteropServices.LibraryImportAttribute), typeof(Magnet.Safety.LibraryImportAttribute));
            this.AddReplaceType(typeof(System.Runtime.InteropServices.ComImportAttribute), typeof(Magnet.Safety.ComImportAttribute));


            this.AddReplaceType(typeof(Magnet.Core.IScriptInstance), typeof(Magnet.Safety.IScriptInstance));



            return this;
        }







    }
}
