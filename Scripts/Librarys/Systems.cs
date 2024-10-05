using App.Core;
using Magnet.Core;
using System;
using System.Runtime.CompilerServices;



[Script(nameof(Systems))]
public class Systems : MyScript
{
    public static AttributeTargets Target1 { get; set; }

    public static AttributeTargets Target2;

    public static AttributeTargets Target3
    {
        get
        {
            return AttributeTargets.Class;
        }
        set
        {

        }
    }

    [Function("Login")]
    public void SystemInitialize(Object args)
    {
        Console.WriteLine(args);
    }


    protected override void Initialize()
    {
        //DEBUG("Systems.Initialize");
    }

    [ModuleInitializer]
    public static void Module()
    {
        Console.WriteLine("ModuleInitializer");
    }
}
