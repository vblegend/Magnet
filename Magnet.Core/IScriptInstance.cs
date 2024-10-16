

namespace Magnet.Core
{
    public interface IScriptInstance
    {
        void InjectedContext(IStateContext stateContext); 
        void Initialize();
    }
}
