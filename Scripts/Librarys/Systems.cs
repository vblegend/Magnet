using App.Core;
using Magnet.Core;
using System;


[Script(nameof(Systems))]
public class Systems : GameScript
{

    public static Double Value = Math.PI;
    public static AttributeTargets Target1 { get; set; }

    [Global]
    public static AttributeTargets Target2;


    [Global]
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
        this.Print(args);
    }


    protected override void Initialize()
    {
        //DEBUG("Systems.Initialize");
    }
}
