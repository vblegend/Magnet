
using App.Core;
using Magnet.Core;


[Script("ScriptB")]
public class ScriptB : GameScript
{


    public void Test()
    {
        this.Print("无参数函数");

    }

    public void PrintMessage(string message)
    {
        this.Print(message);
        //Assembly.Load("System.Drawing.dll");
    }
}
