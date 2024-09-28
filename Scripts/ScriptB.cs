
using Magnet.Context;
using System.Runtime.InteropServices;



[Script("ScriptB")]
public class ScriptB : BaseScript
{
    [DllImport("demo.dll")]
    public static extern bool OpenDemo();


    public static Double Value = Math.PI;


    protected override void Initialize()
    {
        DEBUG("ScriptB.Initialize");
    }



    public void PrintMessage(string message)
    {
        Console.WriteLine(message);
        //Assembly.Load("System.Drawing.dll");
    }
}
