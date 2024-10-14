using App.Core;
using Magnet.Core;

using System.IO;
using System.Threading;
using System;
using System.Collections.Generic;

using App.Core.UserInterface;
using App.Core.Probability;


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




        var lottery = Lottery<String>.Load("");

        lottery.Add(45.0, "F");
        lottery.Add(35.0, "E");
        lottery.Add(8.0, "D");
        lottery.Add(5.0, "C");
        lottery.Add(3.0, "B");
        lottery.Add(2.0, "A");
        lottery.Add(1.0, "S");
        lottery.Add(0.5, "SS");
        lottery.Add(0.1, "SSS");





        var result = lottery.Draw();



        switch (RANDOM(5))
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;



            default:
                break;
        }







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
