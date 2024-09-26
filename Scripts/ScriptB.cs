
using Magnet.Context;
using System;
using System.Reflection;



[Script("ScriptB")]
public class ScriptB : BaseScript
{
    public void PrintMessage(string message)
    {
        Console.WriteLine(message);
        //Assembly.Load("System.Drawing.dll");
    }
}
