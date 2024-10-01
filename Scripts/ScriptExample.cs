﻿using App.Core;
using Magnet.Core;


namespace Scripts
{
    // A usable script must meet three requirements.
    // 1. The access must be public
    // 2. The Script Attribute must be marked
    // 3. The BaseScript class must be inherited

    [Script(nameof(ScriptExample))] 
    public class ScriptExample : BaseScript
    {

        [Function("Hello")]
        public void Hello(String name)
        {
            Console.WriteLine($"Hello {name}!");
        }



    }
}
