using App.Core;
using Magnet.Core;

using System.IO;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


[Script(nameof(ScriptA))]
public class ScriptA : MyScript
{
    [Autowired]
    private IKilledContext KilledContext;

    //[Autowired]
    private IKilledContext KilledContext2 { get; set; }


    protected override void Initialize()
    {
        //DEBUG("ScriptA.Initialize");
    }




    [Function]
    public void Login(LoginContext context)
    {

        this.DEBUG($"SCRIPT GLOBAL.VAR = {GLOBAL.S[1]}");
        GLOBAL.S[1] = "Hello Wrold";

        ENABLED_TIMER(0, Initialize, 5);

        this.Test(context.UserName);

        var typed = typeof(Thread);

        Console.WriteLine(typed.FullName);


        var list = new List<string>();

        debugger();

        PRINT("System.Drawing.dll");

        CALL("ScriptB", "PrintMessage", "Help");


        SCRIPT<ScriptB>().PrintMessage("aaa");

        SCRIPT<ScriptB>((script) =>
        {
            script.PrintMessage("");
        });

        //ScriptB.PrintMessage(message);
    }
}
