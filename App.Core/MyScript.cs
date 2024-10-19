using App.Core.Probability;
using App.Core.UserInterface;
using Magnet.Core;
using System;


namespace App.Core
{
    public abstract class MyScript : AbstractScript
    {

        [Autowired]
        protected readonly GlobalVariableStore? GLOBAL;

        [Autowired("SELF")]
        protected readonly IObjectContext? SELF;



        



        protected void ENABLED_TIMER(Int32 timerIndex, Action callback, UInt32 intervalSecond)
        {
            Int64 combined = ((Int64)timerIndex << 32) | (uint)intervalSecond;
            Int32 aRecovered = (Int32)(combined >> 32);
            UInt32 bRecovered = (UInt32)(combined & 0xFFFFFFFF);
        }


        protected void DISABLE_TIMER(Int32 timerIndex)
        {

        }



        protected IItemBuilder MAKE(String item)
        {


            return null;
        }

        protected IItemBuilder MAKE(Int32 itemId)
        {


            return null;
        }


        protected void GIVE(IItemBuilder item)
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
