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
    [Autowired("A")]
    private readonly ScriptExample scriptExample11;

    [Autowired(typeof(ScriptExample))]
    private readonly ScriptExample scriptExample22;

    [Autowired(typeof(ScriptExample), "B")]
    private readonly ScriptExample scriptExample33;

    [Autowired]
    private readonly IKillContext KilledContext;

    [Autowired(typeof(HumContext))]
    private readonly IObjectContext objectContext;

    public event Action? callback;

    [Autowired("Script-Context")]
    private readonly IStateContext _stateContext;
    public System.Threading.Thread saaaa { get; set; } = new System.Threading.Thread(() => { });

    class ClassA<t>
    {
        
    }


    protected override void Initialize()
    {
        var context = base.GetStateContext();
        saaaa = context.GetProvider<System.Threading.Thread>();
        base.Initialize();
    }



    class ClassB : ClassA<Thread>
    {
        [Autowired]
        private readonly ScriptExample scriptExample;

        private Thread thread;
    }

    public delegate void NewThreadDelegate(Thread a, Thread b);



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

        (Int32, Boolean) ddd = (123, true);

        var s33 = new List<String>();

        Thread saaaa = new Thread(() => { Print("123"); });
        Thread[] ffff = new Thread[0];

        System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(() => { });
        

        ABCD<Thread> esd1 = new ABCD<Thread>();
        List<Thread> esd2 = new List<Thread>();
        esd2.Add(new Thread(() => { }));

        var s = System.Threading.Thread.CurrentThread?.Priority;

        var t = typeof(System.Threading.Thread);
        var n = nameof(System.Threading.Thread);


        var ssss = esd1.GetType();
        Print(ssss);

        Print("typeof:{0} nameof:{1}", [t, n]);
        scriptExample11?.Hello("Wrold");
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
        //ScriptB.PrintMessage(message);
    }
}
