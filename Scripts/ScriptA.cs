using App.Core;
using Magnet.Core;

using System.IO;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[Script(nameof(ScriptA))]
public class ScriptA : BaseScript
{
    [Autowired]
    private IKilledContext KilledContext;

    //[Autowired]
    private IKilledContext KilledContext2 { get; set; }


    protected override void Initialize()
    {
        //DEBUG("ScriptA.Initialize");
    }


    [Function("Login")]
    public void Login(LoginContext context)
    {

        this.DEBUG($"SCRIPT GLOBAL.VAR = {GLOBAL.STR[1]}");
        GLOBAL.STR.Set(1,"Hello Wrold");
        this.Test(context.UserName);

        File.WriteAllText("1", "1");
        Directory.EnumerateFiles("..");
        var s = new Thread(() => { });
        s.Start();
        Thread.Sleep(0);
        var t = Thread.CurrentThread;
        Console.WriteLine($"GetCurrentProcessorId = {t};");
        typeof(ScriptA).GetNestedTypes();


        ThreadPool.QueueUserWorkItem((e) => { });

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
