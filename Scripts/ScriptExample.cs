﻿using Magnet.Core;
using System;



// A usable script must meet three requirements.
// 1. The access must be public
// 2. The Script Attribute must be marked
// 3. The AbstractScript class must be inherited

[Script(nameof(ScriptExample))]
public class ScriptExample : AbstractScript
{

    protected override void Initialize()
    {
        //DEBUG("ScriptExample.Initialize");
    }



    [Function("Hello")]
    public void Hello(String name)
    {
        this.PRINT($"Hello {name}!");
    }



    public Double Target
    {
        get
        {
            return 3.14;
        }
        set
        {
            this.PRINT(value);
        }
    }
}

