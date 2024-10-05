
using App.Core;
using Magnet.Core;
using System.Runtime.InteropServices;



[Script("ScriptB")]
public class ScriptB : MyScript
{

    public static Double Value = Math.PI;


    protected override void Initialize()
    {
        //DEBUG("ScriptB.Initialize");
    }



    public void PrintMessage(string message)
    {
        Console.WriteLine(message);
        //Assembly.Load("System.Drawing.dll");
    }
}
