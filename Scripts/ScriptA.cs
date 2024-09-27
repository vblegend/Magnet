using Magnet.Context;
//using System;
//using System.Threading;


[Script(nameof(ScriptA))]
public class ScriptA : BaseScript
{


    protected override void Initialize()
    {
        DEBUG("ScriptA.Initialize");
    }


    [Function("Login")]
    public void Login(string message)
    {
        try
        {
            //File.Create("");
            this.Test("this.Test");
            new Thread(new ThreadStart(() => { }));

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
