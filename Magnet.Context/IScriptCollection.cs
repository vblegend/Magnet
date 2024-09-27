using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet.Context
{
    public interface IScriptCollection
    {
        public T TypeOf<T>() where T: BaseScript;

        public BaseScript TypeOf(Type type);



        public BaseScript NameOf(String scriptName);




    }
}
