// See https://aka.ms/new-console-template for more information


using App.Core;
using App.Core.Events;
using App.Core.Timer;
using Magnet;

using Microsoft.CodeAnalysis;
using QuadTrees;
using QuadTrees.QTreePoint;
using ScriptRuner;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;





public static class Program
{


    private static GlobalVariableStore GLOBAL = new GlobalVariableStore();

    private static ScriptOptions Options(String name)
    {
        ScriptOptions options = new ScriptOptions();
        options.WithName(name);
        options.WithOutPutFile("123.dll");
        options.WithDebug(false);

        //options.WithRelease();
        options.WithAllowAsync(false);
        options.AddReferences<LoginContext>();
        options.WithDirectory("../../../../Scripts");
        options.WithPreprocessorSymbols("USE_FILE");

        var timerProvider = new TimerProvider();
        options.AddAnalyzer(timerProvider);
        options.RegisterProvider(timerProvider);

        // Insecure
        options.DisabledInsecureTypes();
        options.WithTypeRewriter(new TypeRewriter());
        options.UseDefaultSuppressDiagnostics();
        //
        options.SetAssemblyLoadCallback(AssemblyLoad);
        options.RegisterProvider<ObjectKilledContext>(new ObjectKilledContext());
        options.RegisterProvider(GLOBAL);
        options.RegisterProvider<IObjectContext>(new HumContext(), "SELF");

        return options;
    }



    static Assembly AssemblyLoad(ScriptLoadContext context, AssemblyName assemblyName)
    {
        return null;
    }


    class Hum : IPointQuadStorable
    {
        public Point Point { get; set; } = new Point(650, 22);
    }

    public static void Main()
    {
        GLOBAL.S[1] = "This is Global String Variable.";
        RemoveDir("../../../../Scripts/obj");
        RemoveDir("../../../../Scripts/bin");

        QuadTreePoint<Hum> quadTree = new QuadTreePoint<Hum>(1, 1, 1024, 1024);
        var hum = new Hum() { Point = new Point(100, 240) };
        quadTree.Add(hum);
        var objs1 = quadTree.GetObjects(888, 888);
        var objs2 = quadTree.GetObjects(100, 240);
        hum.Point = new Point(888, 888);
        quadTree.Move(hum);
        var objs3 = quadTree.GetObjects(888, 888);
        var objs4 = quadTree.GetObjects(100, 240);
        quadTree.Remove(hum);
        Console.WriteLine();


        //var lottery = Lottery<String>.Load("lotterys/unlimited.txt");
        //var lootGenerator = LootGenerator<String>.Load("loots/default.loot");
        //TestSccriptUnload();


        //using (new WatchTimer("Loot Generate 100000"))
        //{
        //    for (int i = 0; i < 100000; i++)
        //    {
        //        lootGenerator.Generate();
        //    }
        //}


        //using (new WatchTimer("Loot Generate 1"))
        //{
        //    var loots = lootGenerator.Generate(1);
        //    if (loots.Count() > 0)
        //    {
        //        foreach (var loot in loots)
        //        {
        //            Console.WriteLine($"Drop Item: {loot}");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("Not Dorp Items。");
        //    }
        //}

        //using (new WatchTimer("Draw SS With"))
        //{
        //    var count = 0;
        //    while (true)
        //    {
        //        count++;
        //        var drops = lootGenerator.Generate(1.0);
        //        var Count = drops.Where(e => e.Value == "SSS").Count();

        //        if (Count > 0) break;
        //    }
        //    Console.WriteLine(count);
        //}

        //using (new WatchTimer("Draw Minimum Guarantee 75"))
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        var drawItem = lottery.Draw();
        //        if (drawItem == null) break;
        //        Console.Write($"Draw Item ");
        //        if (drawItem[0] == 'S') Console.BackgroundColor = ConsoleColor.Red;
        //        Console.Write(drawItem);
        //        if (drawItem[0] == 'S') Console.BackgroundColor = ConsoleColor.Black;
        //        Console.WriteLine($" With {i + 1} Count.");
        //    }
        //}
        //using (new WatchTimer("Draw SSS With"))
        //{
        //    var count = 0;
        //    while (true)
        //    {
        //        count++;
        //        var lottery2 = lottery.Clone();
        //        var drawItem = lottery2.Draw();
        //        if (drawItem == "SSS") break;
        //    }
        //    Console.WriteLine(count);
        //}


        //var weakLogin = TestSccriptUnload();

        MagnetScript scriptManager = new MagnetScript(Options("My.Raffler"));
        scriptManager.Unloading += ScriptManager_Unloading;
        scriptManager.Unloaded += ScriptManager_Unloaded;


        var result = scriptManager.Compile();
        foreach (var diagnostic in result.Diagnostics.Where(e => e.Severity != DiagnosticSeverity.Hidden))
        {
            Console.WriteLine(diagnostic.ToString());
        }
        if (result.Success)
        {
            var stateOptions = StateOptions.Default;
            stateOptions.Identity = 666;
            stateOptions.RegisterProvider(new TimerService());
            var stateTest = scriptManager.CreateState(stateOptions);
            var weakMain = stateTest.MethodDelegate<Action>("ScriptA", "Main");
            if (weakMain != null && weakMain.TryGetTarget(out var main))
            {
                using (new WatchTimer("With Call Main()")) main();
                main = null;
            }

            var weakPlayerLife = stateTest.ScriptAs<IPlayLifeEvent>();
            if (weakPlayerLife != null && weakPlayerLife.TryGetTarget(out var lifeEvent))
            {
                using (new WatchTimer("With Call OnOnline()")) lifeEvent.OnOnline(null);
                lifeEvent = null;
            }
            stateTest = null;
            scriptManager.Unload(true);
        }

        while (scriptManager.Status == ScrriptStatus.Unloading && scriptManager.IsAlive)
        {
            var obj = new byte[1024 * 1024];
            Thread.Sleep(10);
        }
        GC.Collect();
        Console.WriteLine("=====================================================================================");
        Console.ReadKey();
    }

    private static void ScriptManager_Unloaded(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}({obj.UniqueId})]卸载完毕.");
    }

    private static void ScriptManager_Unloading(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}({obj.UniqueId})]卸载请求.");
    }








    private static void RemoveDir(String dirPath)
    {
        var rootDir = Path.GetFullPath(dirPath);
        if (Directory.Exists(rootDir))
        {
            Directory.Delete(rootDir, true);
        }
    }



    private static void CallLogin(MagnetState state)
    {
        var login = state.MethodDelegate<Action>("ScriptA", "Main");
        if (login.TryGetTarget(out var target))
        {
            target();
            target = null;
        }
        state = null;
    }




}


