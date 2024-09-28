using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet.Context
{
    public interface IScriptInstance
    {
        void InjectedContext(IScriptCollection scriptCollection); 
        void Initialize();
    }
}
