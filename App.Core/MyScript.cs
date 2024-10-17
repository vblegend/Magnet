using App.Core.Probability;
using Magnet.Core;
using System;


namespace App.Core
{
    public abstract class MyScript : AbstractScript
    {

        [Autowired]
        protected readonly GlobalVariableStore? GLOBAL;

        [Autowired]
        protected readonly Object? SELF;







        protected void ENABLED_TIMER(Int32 timerIndex, Action callback, UInt32 intervalSecond)
        {
            Int64 combined = ((Int64)timerIndex << 32) | (uint)intervalSecond;
            Int32 aRecovered = (Int32)(combined >> 32);
            UInt32 bRecovered = (UInt32)(combined & 0xFFFFFFFF);
        }


        protected void DISABLE_TIMER(Int32 timerIndex)
        {

        }



        protected Int32 RANDOM(Int32 maxValue)
        {
            return Random.Shared.Next(maxValue);
        }





        protected TValue RANDOM<TValue>(Lottery<TValue> items)
        {

           //var value = Random.Shared.NextDouble() * items.TotalProbability;

           // items.Random(value);



            return default;
        }


    }
}
