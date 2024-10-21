using App.Core;
using Magnet.Core;
using System.Threading;
using System.Collections.Generic;
using App.Core.UserInterface;


[Script(nameof(ScriptA))]
public class ScriptA : MyScript
{
    [Autowired]
    private IKilledContext KilledContext;


    [Function]
    public void Login(LoginContext context)
    {
        this.PRINT(KilledContext.ObjectId);
        Script<Raffler>().Draw(context);

        //if (this is IScriptInstance instance)
        //{
        //    this.PRINT(instance.GetState());
        //}
        //var s = this as IScriptInstance;
        //this.PRINT(s.GetState());
        //var s2 = (IScriptInstance)this;
        //this.PRINT(s2.GetState());



        UI.DIALOG(120).TO(SELF)
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

        this.DEBUG($"SCRIPT GLOBAL.VAR = {GLOBAL.S[1]}");
        GLOBAL.S[1] = "Hello Wrold";

        ENABLE_TIMER(0);


        var typed = typeof(Thread);

        this.PRINT(typed.FullName);


        List<string> list = [];

        debugger();

        this.PRINT("System.Drawing.dll");
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
