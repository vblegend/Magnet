
using App.Core;
using Magnet.Core;


[Script("ScriptB")]
public class ScriptB : GameScript
{


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
