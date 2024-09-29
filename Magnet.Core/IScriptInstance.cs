using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet.Core
{
    public interface IScriptInstance
    {
        void InjectedContext(IStateContext stateContext); 
        void Initialize();
    }
}
