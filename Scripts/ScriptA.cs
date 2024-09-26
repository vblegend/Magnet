using Magnet.Context;
using System.Reflection;
using System.Runtime.InteropServices;



[Script(nameof(ScriptA))]
public class ScriptA : BaseScript
{





    [Function("Login")]
    public void Login(string message)
    {
        try
        {
            //File.Create("");
            Assembly.GetAssembly(typeof(ScriptA));
            var ms = typeof(Assembly).GetMethods();
            var sss = new ScriptB();
            //var ss = new System.Net.Sockets.Socket();
            sss.PrintMessage(message);

            foreach (var item in ms)
            {
                Console.WriteLine(item.Name);
            }
            this.Test("this.Test");

            Console.WriteLine("System.Drawing.dll");
            //
            //System.Reflection.Assembly.LoadFrom("System.Drawing.dll");
            //Process.Start("xxx");


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
