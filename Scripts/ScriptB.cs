
using App.Core;
using Magnet.Core;
using System;


[Script("ScriptB")]
public class ScriptB : MyScript
{

    public static Double Value = Math.PI;


    protected override void Initialize()
    {
        //DEBUG("ScriptB.Initialize");
    }

    public void Test()
    {
        this.PRINT("无参数函数");

    }

    public void PrintMessage(string message)
    {
        this.PRINT(message);
        //Assembly.Load("System.Drawing.dll");
    }
}
