﻿using Magnet.Core;


namespace App.Core
{
    public abstract class MyScript : AbstractScript
    {

        [Autowired]
        protected readonly GlobalVariableStore GLOBAL;
 





        protected readonly Object SELF;







        protected void ENABLED_TIMER(Int32 timerIndex, Action callback, Int32 intervalSecond)
        {

        }


        protected void DISABLE_TIMER(Int32 timerIndex)
        {

        }



        protected Int32 RANDOM(Int32 maxValue)
        {
            return Random.Shared.Next(maxValue);
        }



    }
}
