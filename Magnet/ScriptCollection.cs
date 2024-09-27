using Magnet.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet
{
    internal class ScriptCollection : IScriptCollection
    {

        private Dictionary<Type, BaseScript> instances = new Dictionary<Type, BaseScript>();
        private Dictionary<String, BaseScript> instancesByString = new Dictionary<String, BaseScript>();
        public List<BaseScript> scripts = new List<BaseScript>();

        public BaseScript NameOf(string scriptName)
        {
            instancesByString.TryGetValue(scriptName, out BaseScript script);
            return script;
        }

        public T TypeOf<T>() where T : BaseScript
        {
            instances.TryGetValue(typeof(T), out BaseScript script);
            return (T)script;
        }

        public BaseScript TypeOf(Type type)
        {
            instances.TryGetValue(type, out BaseScript script);
            return script;
        }


        public void Add(ScriptAttribute attribute, BaseScript script) {
            instances.Add(script.GetType(), script);
            instancesByString.Add(attribute.Name, script);
            scripts.Add(script);
        }



    }
}
