// See https://aka.ms/new-console-template for more information











using Magnet;


ScriptOptions options = new ScriptOptions();
options.Debug = true;
ScriptManager scriptManager = new ScriptManager(options);
scriptManager.LoadScriptsFromDirectory("../../../../Scripts");
scriptManager.RunScriptMethod("sharps/ScriptA.cs", "ScriptA", "Login", "Hello from script!");
//var engine = new ScriptEngine();
//engine.build("./scripts/test.ts");
Console.WriteLine("=====================================================================================");
Console.ReadKey();