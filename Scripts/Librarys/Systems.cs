using Magnet.Core;
using System;
using System.Runtime.CompilerServices;



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

    [ModuleInitializer]
    public static void Module()
    {
  
    }
}
