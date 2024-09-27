using Magnet.Context;
using System;



[Script(nameof(Systems))]
public class Systems :BaseScript
{


    [Function("Login")]
    public void SystemInitialize(Object args)
    {
        Console.WriteLine(args);
    }


    protected override void Initialize()
    {
        DEBUG("Systems.Initialize");
    }



}
