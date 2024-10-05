using Magnet.Core;


namespace App.Core
{
    public abstract class MyScript : AbstractScript
    {

        [Autowired]
        private readonly GlobalVariableStore? _global;
        protected GlobalVariableStore GLOBAL => _global!;






        protected Object SELF;







        protected void ENABLED_TIMER(Int32 timerIndex, Action callback, Int32 intervalSecond)
        {

        }


        protected void DISABLE_TIMER(Int32 timerIndex)
        {

        }




    }
}
