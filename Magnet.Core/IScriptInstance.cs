

namespace Magnet.Core
{
    public interface IScriptInstance
    {
        void InjectedContext(IStateContext stateContext);
        IStateContext GetState();
        void Initialize();
        void UnInitialize();
    }
}
