// See https://aka.ms/new-console-template for more information


using App.Core;
using App.Core.Probability;
using Magnet;
using ScriptRuner;
using ScriptRuner.Loot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class Program
{
    public delegate void LoginHandler(LoginContext context);

    private static GlobalVariableStore GLOBAL = new GlobalVariableStore();

    private static ScriptOptions Options(String name)
    {
        GLOBAL.S[1] = "This is Global String Variable.";
        ScriptOptions options = new ScriptOptions();
        options.WithName(name);
        options.WithOutPutFile("123.dll");
        options.WithDebug(true);

        options.WithRelease();

        options.WithAllowAsync(false);
        options.AddReferences("System.Threading.Thread");
        options.AddReferences("System.Console");
        //options.AddReferences<MagnetScript>();
        options.AddReferences<LoginContext>();
        options.WithDirectory("../../../../Scripts");
        // Insecure
        options.DisabledInsecureTypes();
        //
        options.SetAssemblyLoadCallback(AssemblyLoad);
        options.AddInjectedObject<ObjectKilledContext>(new ObjectKilledContext());
        options.AddInjectedObject(GLOBAL);

        return options;
    }



    static Assembly AssemblyLoad(ScriptLoadContext context, AssemblyName assemblyName)
    {
        return null;
    }




    public static void Main()
    {
        RemoveDir("../../../../Scripts/obj");
        RemoveDir("../../../../Scripts/bin");

        var lottery = Lottery<String>.Load("lotterys/unlimited.txt");

        var lootGenerator = LootGenerator<String>.Load("loots/default.loot");


        using (new WatchTimer("Loot Generate 100000"))
        {
            for (int i = 0; i < 100000; i++)
            {
                lootGenerator.Generate();
            }
        }


        using (new WatchTimer("Loot Generate 1"))
        {
            var loots = lootGenerator.Generate(1);
            if (loots.Count() > 0)
            {
                foreach (var loot in loots)
                {
                    Console.WriteLine($"Drop Item: {loot}");
                }
            }
            else
            {
                Console.WriteLine("Not Dorp Items。");
            }

        }

        using (new WatchTimer("Draw SS With"))
        {
            var count = 0;
            while (true)
            {
                count++;
                var drops = lootGenerator.Generate(1.0);
                var Count = drops.Where(e => e.Value == "SSS").Count();

                if (Count > 0) break;
            }
            Console.WriteLine(count);
        }

        using (new WatchTimer("Draw Minimum Guarantee 75"))
        {
            for (int i = 0; i < 100; i++)
            {
                var drawItem = lottery.Draw();
                if (drawItem == null) break;
                Console.Write($"Draw Item ");
                if (drawItem[0] == 'S') Console.BackgroundColor = ConsoleColor.Red;
                Console.Write(drawItem);
                if (drawItem[0] == 'S') Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($" With {i + 1} Count.");
            }
        }
        using (new WatchTimer("Draw SSS With"))
        {
            var count = 0;
            while (true)
            {
                count++;
                var lottery2 = lottery.Clone();
                var drawItem = lottery2.Draw();
                if (drawItem == "SSS") break;
            }
            Console.WriteLine(count);
        }

        var weakLogin = TestSccriptUnload();

        MagnetScript scriptManager = new MagnetScript(Options("My.Raffler"));
        var result = scriptManager.Compile();
        if (result.Success)
        {
            Console.WriteLine();

            using (new WatchTimer("Set ScriptB.Value = {1.23456};"))
            {
                var state = scriptManager.CreateState();
                state.SetFieldValue("ScriptB", "Value", 1.23456);
                var value = state.GetFieldValue("ScriptB", "Value");
            }

            using (new WatchTimer("Create Script State 1"))
            {
                var state = scriptManager.CreateState();
            }


            using (new WatchTimer("Create Delegate 100000"))
            {
                var state = scriptManager.CreateState();
                for (int i = 0; i < 100000; i++)
                {
                    state.MethodDelegate<LoginHandler>("ScriptA", "Login");
                }
            }


            List<MagnetState> states = new List<MagnetState>();
            using (new WatchTimer("Create State 100000"))
            {
                for (int i = 0; i < 100000; i++)
                {
                    var state = scriptManager.CreateState();
                    states.Add(state);
                    state.Dispose();
                }
            }


            using (new WatchTimer("Dispose State 100000"))
            {
                foreach (var state in states)
                {
                    state.Dispose();
                }
            }


            var stateTest = scriptManager.CreateState();

            var weak = stateTest.MethodDelegate<LoginHandler>("ScriptA", "Login");
            using (new WatchTimer("Try GetTarget 100000"))
            {
                for (int i = 0; i < 100000; i++)
                {
                    if (weak.TryGetTarget(out var ss))
                    {
                        ss = null;
                    }
                }
            }
            try
            {
                CallLogin(stateTest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            scriptManager.Unload(true);

            if (weak.TryGetTarget(out var target))
            {
                target(null);
            }



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
        Console.WriteLine("=====================================================================================");
        Console.ReadKey();
    }



    private static WeakReference<LoginHandler> TestSccriptUnload()
    {
        MagnetScript scriptManager = new MagnetScript(Options("Unload.Test"));
        var result = scriptManager.Compile();
        if (!result.Success)
        {
            foreach (var item in result.Diagnostics)
            {
                Console.WriteLine(item.ToString());
            }
        }
        List<MagnetState> states = new List<MagnetState>();
        var state = scriptManager.CreateState();
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
        }

    }




}


