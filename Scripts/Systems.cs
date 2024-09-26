using Magnet.Context;



[Script(nameof(Systems))]
internal class Systems
{


    [Function("Login")]
    public void SystemInitialize(Object args)
    {
        Console.WriteLine(args);
    }





}
