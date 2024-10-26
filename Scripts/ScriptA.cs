using App.Core;
using Magnet.Core;
using System.Collections.Generic;
using App.Core.UserInterface;
using App.Core.Events;



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

    public event Action callback;

    [Script(nameof(ClassB))]
    class ClassB : GameScript
    {
        [Autowired]
        private readonly ScriptExample scriptExample;

        private Thread thread;



    }



    [Function]
    public void Main()
    {
        this.Print(KilledContext.Target);

        //if (this is IScriptInstance instance)
        //{
        //    this.PRINT(instance.GetState());
        //}
        //var s = this as IScriptInstance;
        //this.PRINT(s.GetState());

        var s = Thread.CurrentThread?.Priority;
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
