using App.Core;
using Magnet.Context;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;


[Script(nameof(ScriptA))]
public class ScriptA : BaseScript
{
    [Autowired]
    private Int32 value;



    protected override void Initialize()
    {
        DEBUG("ScriptA.Initialize");
    }


    [Function("Login")]
    public void Login(LoginContext context)
    {
        try
        {
            //File.Create("");
            this.Test(context.UserName);

            File.WriteAllText("1","1");
            Directory.EnumerateFiles("..");
            var s = new Thread(() => { });
            s.Start();
            Thread.Sleep(0);            
            var t = Thread.CurrentThread;
            Console.WriteLine($"GetCurrentProcessorId = {t};");
            typeof(ScriptA).GetNestedTypes();
            ThreadPool.QueueUserWorkItem((e) => { });

            var list = new List<string>();

            //new TTTTT(() => { });
            debugger();

            PRINT("System.Drawing.dll");

            CALL("ScriptB", "PrintMessage", "Help");
            
            
            SCRIPT<ScriptB>().PrintMessage("aaa");

            SCRIPT<ScriptB>((script) =>
            {
                script.PrintMessage("");
            });


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        //ScriptB.PrintMessage(message);
    }
}
