
using Magnet.Context;
using System;
using System.Reflection;
using System.Runtime.InteropServices;



[Script("ScriptB")]
public class ScriptB : BaseScript
{
    [DllImport("demo.dll")]
    public static extern bool OpenDemo();
    public void PrintMessage(string message)
    {
        Console.WriteLine(message);
        //Assembly.Load("System.Drawing.dll");
    }
}
