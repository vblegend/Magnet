
using System;

public static class Extends
{

    public static void Test(this ScriptA script, string message)
    {
        Console.WriteLine(message);
    }
}
