using Magnet.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Magnet
{

    public delegate Assembly AssemblyLoadDelegate(ScriptLoadContext context, AssemblyName assemblyName);

    public class ScriptOptions
    {

        public String Name { get; private set; } = "Magnet.Script";


        public String OutPutFile { get; private set; }

        public ScriptRunMode Mode { get; private set; } = ScriptRunMode.Release;

        public String BaseDirectory { get; private set; }

        public String ScriptFilePattern { get; private set; } = "*.cs";


        public AssemblyLoadDelegate AssemblyLoad { get; private set; }

        /// <summary>
        /// add using xxxx;
        /// </summary>
        public List<String> Using { get; private set; } = [];

        /// <summary>
        /// import Assemblys
        /// </summary>
        public List<Assembly> References { get; private set; } = [];

        public Boolean UseDebugger { get; private set; }

        public Boolean AllowAsync { get; private set; } = false;

        public IOutput Output { get; private set; } = new ConsoleOutput();



        internal readonly Dictionary<String, String> ReplaceTypes = new Dictionary<string, string>();



        public ScriptOptions SetOutput(IOutput messagePrinter)
        {
            this.Output = messagePrinter;
            return this;
        }


        public ScriptOptions SetAssemblyLoadCallback(AssemblyLoadDelegate assemblyLoad)
        {
            this.AssemblyLoad = assemblyLoad;
            return this;
        }


        public ScriptOptions AddUsings(params String[] nameSpaces)
        {
            foreach (var name in nameSpaces)
            {
                this.Using.Add(name);
            }
            return this;
        }

        internal ConcurrentDictionary<Type, Object> InjectedObjectMap { get; private set; } = [];

        public ScriptOptions AddInjectedObject<T>(T value)
        {
            var type = typeof(T);
            if (InjectedObjectMap.ContainsKey(type))
            {
                throw new InvalidOperationException();
            }
            InjectedObjectMap.TryAdd(type, value);
            return this;
        }




        public ScriptOptions AddReferences(params Assembly[] assemblies)
        {
            this.References.AddRange(assemblies);
            return this;
        }


        public ScriptOptions AddReferences(params Type[] typeOfAssembles)
        {
            foreach (var type in typeOfAssembles)
            {
                this.References.Add(type.Assembly);
            }
            return this;
        }



        public ScriptOptions ReplaceType(String sourceType, String newType)
        {
            ReplaceTypes.Add(sourceType, newType);
            return this;
        }


        public ScriptOptions ReplaceType(Type sourceType, Type newType)
        {
            ReplaceTypes.Add(sourceType.FullName, newType.FullName);
            return this;
        }


        public ScriptOptions AddReferences<T>() where T : class
        {
            this.References.Add(typeof(T).Assembly);
            return this;
        }



        public ScriptOptions AddReferences(params String[] assemblies)
        {
            foreach (var name in assemblies)
            {
                this.References.Add(Assembly.Load(name));
            }
            return this;
        }




        public ScriptOptions WithOutPutFile(String name)
        {
            this.OutPutFile = name;
            return this;
        }


        public ScriptOptions WithName(String name)
        {
            this.Name = name;
            return this;
        }

        public ScriptOptions WithAllowAsync(Boolean allowAsync)
        {
            this.AllowAsync = allowAsync;
            return this;
        }



        public ScriptOptions WithFilePattern(String filePattern)
        {
            this.ScriptFilePattern = filePattern;
            return this;
        }


        public ScriptOptions WithDirectory(String baseDirectory)
        {
            this.BaseDirectory = baseDirectory;
            return this;
        }

        public ScriptOptions WithDebug(Boolean useDebuggerBreak = true)
        {
            this.Mode = ScriptRunMode.Debug;
            this.UseDebugger = useDebuggerBreak;
            return this;
        }


        public ScriptOptions WithRelease(Boolean useDebuggerBreak = false)
        {
            this.Mode = ScriptRunMode.Release;
            this.UseDebugger = useDebuggerBreak;
            return this;
        }








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
            return this;
        }







    }
}
