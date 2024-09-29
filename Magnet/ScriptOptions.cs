using Magnet.Core;
using System.Collections.Concurrent;
using System.Reflection;

namespace Magnet
{
    public class ScriptOptions
    {

        public String Name { get; set; }

        public ScriptRunMode Mode { get; set; } = ScriptRunMode.Release;

        public String BaseDirectory { get; set; }

        public String ScriptFilePattern { get; set; } = "*.cs";

        /// <summary>
        /// add using xxxx;
        /// </summary>
        public List<String> Using { get; private set; } = [];

        /// <summary>
        /// import Assemblys
        /// </summary>
        public List<Assembly> References { get; private set; } = [];

        public Boolean UseDebugger { get; set; }





        public ScriptOptions AddUsings(params String[] nameSpaces)
        {
            foreach (var name in nameSpaces)
            {
                this.Using.Add(name);
            }
            return this;
        }

        public ConcurrentDictionary<Type, Object> InjectedObjectMap { get; private set; } = [];

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


    }
}
