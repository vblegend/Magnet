using Magnet.Analysis;
using Magnet.Core;
using Magnet.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Magnet
{

    public delegate Assembly AssemblyLoadDelegate(ScriptLoadContext context, AssemblyName assemblyName);

    public class ObjectProvider
    {
        public Type Type;
        public String SlotName;
        public Object Instance;
    }


    public class ScriptOptions
    {
        public String Name { get; private set; } = "Magnet.Script";
        public String OutPutFile { get; private set; }
        public ScriptRunMode Mode { get; private set; } = ScriptRunMode.Release;
        public String BaseDirectory { get; private set; }
        public String ScriptFilePattern { get; private set; } = "*.cs";
        public AssemblyLoadDelegate AssemblyLoad { get; private set; }
        public List<String> Using { get; private set; } = [];
        public List<Assembly> References { get; private set; } = [];
        public Boolean UseDebugger { get; private set; }
        public Boolean AllowAsync { get; private set; } = false;
        public IOutput Output { get; private set; } = new ConsoleOutput();
        public readonly List<IAnalyzer> Analyzers = new List<IAnalyzer>();
        public String[] PreprocessorSymbols { get; private set; } = [];
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
        /// Set the output stream of the script
        /// </summary>
        /// <param name="messagePrinter"></param>
        /// <returns></returns>
        public ScriptOptions SetOutput(IOutput messagePrinter)
        {
            this.Output = messagePrinter;
            return this;
        }

        /// <summary>
        /// Sets the script's assembly load interceptor
        /// </summary>
        /// <param name="assemblyLoad"></param>
        /// <returns></returns>
        public ScriptOptions SetAssemblyLoadCallback(AssemblyLoadDelegate assemblyLoad)
        {
            this.AssemblyLoad = assemblyLoad;
            return this;
        }


        /// <summary>
        /// Add a script type processor
        /// </summary>
        /// <param name="processor"></param>
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
        public ScriptOptions ReplaceType(String sourceType, String newType)
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
        public ScriptOptions ReplaceType(Type sourceType, Type newType)
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
        public ScriptOptions WithPreprocessorSymbols(params string[]? preprocessorSymbols)
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

            this.ReplaceType(typeof(System.GC), typeof(Magnet.Safety.GC));
            this.ReplaceType(typeof(System.Threading.Thread), typeof(Magnet.Safety.Thread));
            this.ReplaceType(typeof(System.Threading.ThreadPool), typeof(Magnet.Safety.ThreadPool));
            this.ReplaceType(typeof(System.Threading.Tasks.Task), typeof(Magnet.Safety.Task));
            this.ReplaceType(typeof(System.AppDomain), typeof(Magnet.Safety.AppDomain));


            // code safe
            this.ReplaceType(typeof(System.Activator), typeof(Magnet.Safety.Activator));
            this.ReplaceType(typeof(System.Type), typeof(Magnet.Safety.Type));
            this.ReplaceType(typeof(System.Reflection.Assembly), typeof(Magnet.Safety.Assembly));
            this.ReplaceType(typeof(System.Reflection.Emit.DynamicMethod), typeof(Magnet.Safety.DynamicMethod));
            this.ReplaceType(typeof(System.Linq.Expressions.DynamicExpression), typeof(Magnet.Safety.DynamicMethod));
            this.ReplaceType(typeof(System.Linq.Expressions.Expression), typeof(Magnet.Safety.DynamicMethod));
            this.ReplaceType(typeof(System.Runtime.CompilerServices.CallSite), typeof(Magnet.Safety.DynamicMethod));

            //
            this.ReplaceType(typeof(System.Environment), typeof(Magnet.Safety.Environment));
            this.ReplaceType(typeof(System.Diagnostics.Process), typeof(Magnet.Safety.Process));
            this.ReplaceType(typeof(System.Runtime.InteropServices.Marshal), typeof(Magnet.Safety.Marshal));

            // IO
            this.ReplaceType(typeof(System.IO.File), typeof(Magnet.Safety.File));
            this.ReplaceType(typeof(System.IO.Directory), typeof(Magnet.Safety.Directory));
            this.ReplaceType(typeof(System.IO.FileStream), typeof(Magnet.Safety.FileStream));
            this.ReplaceType(typeof(System.IO.StreamWriter), typeof(Magnet.Safety.StreamWriter));
            this.ReplaceType(typeof(System.IO.StreamReader), typeof(Magnet.Safety.StreamReader));



            // NET
            this.ReplaceType(typeof(System.Net.Sockets.Socket), typeof(Magnet.Safety.Socket));
            this.ReplaceType(typeof(System.Net.WebClient), typeof(Magnet.Safety.WebClient));
            this.ReplaceType(typeof(System.Net.Http.HttpClient), typeof(Magnet.Safety.HttpClient));


            //
            this.ReplaceType(typeof(System.Runtime.InteropServices.DllImportAttribute), typeof(Magnet.Safety.DllImportAttribute));
            this.ReplaceType(typeof(System.Runtime.CompilerServices.ModuleInitializerAttribute), typeof(Magnet.Safety.ModuleInitializerAttribute));
            this.ReplaceType(typeof(System.Runtime.InteropServices.LibraryImportAttribute), typeof(Magnet.Safety.LibraryImportAttribute));
            this.ReplaceType(typeof(System.Runtime.InteropServices.ComImportAttribute), typeof(Magnet.Safety.ComImportAttribute));


            this.ReplaceType(typeof(Magnet.Core.IScriptInstance), typeof(Magnet.Safety.IScriptInstance));



            return this;
        }







    }
}
