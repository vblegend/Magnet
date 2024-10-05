using Magnet.Core;

public static class DebuggerExtends
{

    public static void DEBUG(this AbstractScript script, string message)
    {
        Console.WriteLine(message);
    }

}