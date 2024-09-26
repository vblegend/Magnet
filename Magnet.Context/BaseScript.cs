using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Magnet.Context
{
    public abstract class BaseScript
    {









        public void CALL(String script, String method, params Object[] objects)
        {

        }

        public T? SCRIPT<T>() where T : BaseScript
        {
            Type type = typeof(T);
            var attribute = type.GetCustomAttribute<ScriptAttribute>();
            if (attribute != null)
            {
                // attribute.Name


                return null;
            }
            return null;
        }







        public void SCRIPT<T>(Action<T> callback) where T : BaseScript
        {
            var script = SCRIPT<T>();
            if(script != null) callback(script);
        }

    }
}
