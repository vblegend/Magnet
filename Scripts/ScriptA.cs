using App.Core;
using Magnet.Core;
using System.Threading;
using System.Collections.Generic;
using App.Core.UserInterface;
using App.Core.Events;


[Script(nameof(ScriptA))]
public class ScriptA : GameScript
{
    [Autowired]
    private IKillContext KilledContext;


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

        ENABLE_TIMER(0);

        ENABLE_TIMER(1);

        var typed = typeof(Thread);

        Print(typed.FullName);

        Assert(typed.FullName == "Magnet.Safety.Thread");

        DISABLE_TIMER(0);
        DISABLE_TIMER(1);
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
