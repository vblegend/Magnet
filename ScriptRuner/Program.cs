// See https://aka.ms/new-console-template for more information


using Magnet;


public static class Program
{
    public delegate void LoginHandler(string message);
    public static void Main()
    {


        ScriptOptions options = new ScriptOptions();
        options.Debug = true;
        options.BaseDirectory = "../../../../Scripts";
        options.Using = [];
        ScriptCompiler scriptManager = new ScriptCompiler(options);
        var engine = scriptManager.LoadScriptsFromDirectory();
        engine.Initialize();


        var value = engine.GetVariable("ScriptB", "Value");



        var login = engine.GetDelegate<LoginHandler>("ScriptA", "Login");

        login("Hello from script#");

        //var engine = new ScriptEngine();
        //engine.build("./scripts/test.ts");
        Console.WriteLine("=====================================================================================");
        Console.ReadKey();
    }




}


