// See https://aka.ms/new-console-template for more information


using App.Core;
using App.Core.Probability;
using Magnet;
using ScriptRuner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


public static class Program
{
    public delegate void LoginHandler(LoginContext context);

    private static GlobalVariableStore GLOBAL = new GlobalVariableStore();

    private static ScriptOptions Options()
    {
        GLOBAL.S[1] = "This is Global String Variable.";
        ScriptOptions options = new ScriptOptions();
        options.WithDebug(false);

        //options.WithRelease();

        options.WithAllowAsync(false);
        options.AddReferences("System.Threading.Thread");
        options.AddReferences("System.Console");
        options.AddReferences<MagnetScript>();
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

        var lottery = Lottery<String>.Load("lotterys/minimum guarantee.txt");




        using (new WatchTimer("Draw Minimum Guarantee 75"))
        {
            for (int i = 0; i < 100; i++)
            {
                var drawItem = lottery.Draw();
                if (drawItem == null) break;

                Console.Write($"Draw Item ");

                if (drawItem[00] == 'S')
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                Console.Write(drawItem);

                if (drawItem[00] == 'S')
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                }


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

        if (weakLogin.TryGetTarget(out var login))
        {
            Console.WriteLine("脚本模块卸载失败！");
            var context = new LoginContext();
            context.UserName = "Administrator";
            login(context);
        }
        //System.Private.Xml, Version = 8.0.0.0, Culture = neutral, PublicKeyToken = cc7b13ffcd2ddd51
        // Magnet.Script, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null
        var scriptModule = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.FullName.StartsWith("Magnet.Script,")).FirstOrDefault();
        if (scriptModule != null)
        {
            Console.WriteLine("脚本模块卸载失败！");
        }
        else
        {
            Console.WriteLine("脚本模块卸载成功！");
        }

        MagnetScript scriptManager = new MagnetScript(Options());
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

                    }
                }
            }


            scriptManager.Unload();


            GC.Collect();
            GC.WaitForPendingFinalizers();


            try
            {
                CallLogin(stateTest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }




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


    static MagnetState scriptState;

    private static WeakReference<LoginHandler> TestSccriptUnload()
    {
        MagnetScript scriptManager = new MagnetScript(Options());
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
        scriptManager.Unload();
        state.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
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


