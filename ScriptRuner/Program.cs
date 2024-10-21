// See https://aka.ms/new-console-template for more information


using App.Core;
using App.Core.Events;

using Magnet;
using QuadTrees;
using QuadTrees.QTreePoint;
using ScriptRuner;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;

public static class Program
{
    public delegate void LoginHandler(LoginContext context);

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

        // Insecure
        options.DisabledInsecureTypes();
        //
        options.SetAssemblyLoadCallback(AssemblyLoad);
        options.AddInjectedObject<ObjectKilledContext>(new ObjectKilledContext());
        options.AddInjectedObject(GLOBAL);
        options.AddInjectedObject<IObjectContext>(new HumContext(), "SELF");

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
        hum.Point = new Point(888,888);
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
        if (result.Success)
        {

            List<MagnetState> states = new List<MagnetState>();
            using (new WatchTimer("Create State 100000"))
            {
                for (int i = 0; i < 100000; i++)
                {
                    var state = scriptManager.CreateState();
                    states.Add(state);
                }
            }

            using (new WatchTimer("Create Delegate 100000"))
            {
                var state = scriptManager.CreateState(345);
                for (int i = 0; i < 100000; i++)
                {
                    state.MethodDelegate<LoginHandler>("ScriptA", "Login");
                }
                state = null;
            }

            using (new WatchTimer("Dispose State 10000"))
            {
                foreach (var state in states)
                {
                    state.Dispose();
                }
            }
            states.Clear();

            var stateTest = scriptManager.CreateState(123);


            var weak = stateTest.MethodDelegate<LoginHandler>("ScriptA", "Login");
            if (weak != null && weak.TryGetTarget(out var handler2))
            {
                handler2(null);
                handler2 = null;
                //
            }


            var weakSetter = stateTest.PropertySetterDelegate<Double>("ScriptExample", "Target");
            if (weakSetter != null && weakSetter.TryGetTarget(out var setter))
            {
                setter(123.45);
                setter = null;
            }

            var weakGetter = stateTest.PropertyGetterDelegate<Double>("ScriptExample", "Target");
            if (weakGetter != null && weakGetter.TryGetTarget(out var getter))
            {
                Console.WriteLine(getter());
                getter = null;
            }


            var weakAttackEvent = stateTest.ScriptAs<IPlayerGameEvent>();
            if (weakAttackEvent != null && weakAttackEvent.TryGetTarget(out var attackEvent))
            {
                attackEvent.OnAttack(null);
                attackEvent = null;
            }

            try
            {
                CallLogin(stateTest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            stateTest = null;
            scriptManager.Unload(true);
            //status = GC.WaitForFullGCComplete();
            //if (weak.TryGetTarget(out var handler))
            //{
            //    handler(null);
            //}
            //ArrayPool<Char>.Shared.Rent(1);
        }
        else
        {
            // 处理编译错误
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine(diagnostic.ToString());
            }
        }


        while (scriptManager.IsAlive)
        {
            var obj = new byte[1024 * 1024];
            //GC.Collect();
            Thread.Sleep(10);
        }

        Console.WriteLine("OK");

        while (true)
        {
            GC.Collect();
            Thread.Sleep(1000);
        }

        Console.WriteLine("=====================================================================================");
        Console.ReadKey();
    }

    private static void ScriptManager_Unloaded(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}]卸载完毕.");
    }

    private static void ScriptManager_Unloading(MagnetScript obj)
    {
        Console.WriteLine($"收到脚本[{obj.Name}]卸载请求.");
    }

    private static WeakReference<LoginHandler> TestSccriptUnload()
    {
        MagnetScript scriptManager = new MagnetScript(Options("Unload.Test"));
        scriptManager.Unloading += ScriptManager_Unloading;
        scriptManager.Unloaded += ScriptManager_Unloaded;

        var result = scriptManager.Compile();
        if (!result.Success)
        {
            foreach (var item in result.Diagnostics)
            {
                Console.WriteLine(item.ToString());
            }
            return null;
        }
        List<MagnetState> states = new List<MagnetState>();
        var state = scriptManager.CreateState(999);
        var weak = state.MethodDelegate<LoginHandler>("ScriptA", "Login");
        state.Dispose();
        scriptManager.Unload();
        return weak;
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
        var login = state.MethodDelegate<LoginHandler>("ScriptA", "Login");
        var context = new LoginContext();
        context.UserName = "Administrator";
        if (login.TryGetTarget(out var target))
        {
            target(context);
            target = null;
        }
        state = null;
    }




}


