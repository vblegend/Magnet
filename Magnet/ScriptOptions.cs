using Magnet.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Magnet
{
    public class ScriptOptions
    {

        public String  Name { get; set; }


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
        public  List<Assembly> References { get; private set; } = [];






        public ScriptOptions AddUsings(params String[] nameSpaces)
        {
            foreach (var name in nameSpaces)
            {
                this.Using.Add(name);
            }
            return this;
        }




        public ScriptOptions AddReferences( params Assembly[] assemblies)
        {
            this.References.AddRange(assemblies);
            return this;
        }


        public ScriptOptions AddReferences(params Type[] typeOfAssembles)
        {
            foreach (var type in typeOfAssembles) {
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

        public ScriptOptions WithDebug()
        {
            this.Mode = ScriptRunMode.Debug;
            return this;
        }


        public ScriptOptions WithRelease()
        {
            this.Mode = ScriptRunMode.Release;
            return this;
        }


    }
}
