using App.Core;
using Magnet.Core;
using System.Collections.Generic;
using App.Core.UserInterface;
using App.Core.Events;
using System.Collections;


using System.Threading;
using System;

namespace System.Threading
{

}




[Script(nameof(ScriptA))]
public class ScriptA : GameScript
{
    [Autowired]
    private readonly IKillContext KilledContext;

    [Autowired]
    private readonly ScriptExample scriptExample;

    public event Action ?callback;


    public System.Threading.Thread saaaa { get; set; } = new System.Threading.Thread(() => { });

    class ClassA<t>
    {

    }
 
    class ClassB : ClassA<Thread>
    {
        [Autowired]
        private readonly ScriptExample scriptExample;

        private Thread thread;
    }

    public delegate void NewThreadDelegate(Thread a  , Thread b);



    struct ABCD<T>
    {
       public T Value;
    }

    [Function]
    public unsafe void Main()
    {
        this.Print(KilledContext.Target);

        //if (this is IScriptInstance instance)
        //{
        //    this.PRINT(instance.GetState());
        //}
        //var s = this as IScriptInstance;
        //this.PRINT(s.GetState());

        (Int32, Boolean) ddd = (123,true);

        var s33 = new List<String>();

        Thread saaaa = new Thread(() => { Print("123"); });
        Thread[] ffff = new Thread[0];

        System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(() => { });


        ABCD<Thread> esd1 = new ABCD<Thread>();
        NewList<App.Core.Types.NewThread> esd2 = new NewList<App.Core.Types.NewThread>();
        esd2.Add(new App.Core.Types.NewThread(null));

        var s = System.Threading.Thread.CurrentThread?.Priority;

        var t = typeof(System.Threading.Thread);
        var n = nameof(System.Threading.Thread);


        var ssss = esd1.GetType();
        Print(ssss);

        Print(saaaa);
        scriptExample?.Hello("Wrold");
        UI.DIALOG(120)?.TO(Player);

        UI.DIALOG(120).TO(Player)
          .POSITION(11, 22)
          .TEXT([
              "====[<$name>]====",
              "bbbbbb",
              "cccccc",
              "dddddd"
          ])
          .FLOATING([
              UI.GIF("view", "package://item.wix,12", 0, 0),
              UI.IMAGE("view", "package://item.wix,12", 0, 0),
              UI.BUTTTON("ok", "package://item.wix,13", 15, 100),
              UI.BUTTTON("cancel", "package://item.wix,14", 200, 100)
          ])
          .SEND();

        Debug($"SCRIPT GLOBAL.VAR = {GLOBAL.S[1]}");
        GLOBAL.S[1] = "Hello Wrold";

        EnableTimer(0);

        EnableTimer(1);

        //var typed = this.GetType().FullName; 

        //Print(typed);

        //Assert(typed == "Magnet.Safety.Thread");

        DisableTimer(0);
        DisableTimer(1);
        List<string> list = [];

        debugger();

        Print("System.Drawing.dll");


        Call("ScriptB", "Test", []);
        Call("ScriptB", "PrintMessage", ["Help"]);
        TryCall("ScriptB", "PrintMessage1", ["Help"]);


        Script<ScriptB>().PrintMessage("AAA");

        Script<ScriptB>((script) =>
        {
            script.PrintMessage("BBB");
        });

        //ScriptB.PrintMessage(message);
    }
}
