using App.Core;
using Magnet.Context;
using TTTTT = System.Threading.Thread;


[Script(nameof(ScriptA))]
public class ScriptA : BaseScript
{


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
            
            TTTTT.Sleep(0);
 

            typeof(ScriptA).GetNestedTypes();
            ThreadPool.QueueUserWorkItem((e) => { });


            new TTTTT(new ThreadStart(() => { }));


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
