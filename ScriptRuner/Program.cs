// See https://aka.ms/new-console-template for more information


using App.Core;
using App.Core.Events;
using App.Core.Timer;
using Magnet;

using Microsoft.CodeAnalysis;
using ScriptRuner;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;





public static class Program
{


    private static GlobalVariableStore GLOBAL = new GlobalVariableStore();

    private static ScriptOptions Options(String name)
    {
        ScriptOptions options = ScriptOptions.Default;
        options.WithName(name);
        options.WithOutPutFile("123.dll");
        options.WithDebug(false);

        //options.WithRelease();
        options.WithAllowAsync(false);
        options.WithDirectory("../../../../Scripts");
        options.WithPreprocessorSymbols("USE_FILE");

        options.AddReferences<GameScript>();


        var timerProvider = new TimerProvider();
        options.AddAnalyzer(timerProvider);
        options.RegisterProvider(timerProvider);

        // Insecure
        options.DisabledInsecureTypes();
        options.WithTypeRewriter(new TypeRewriter());
        options.UseDefaultSuppressDiagnostics();
        //
        options.WithAssemblyLoadCallback(AssemblyLoad);
        options.RegisterProvider<ObjectKilledContext>(new ObjectKilledContext());
        options.RegisterProvider(GLOBAL);
        options.RegisterProvider<IObjectContext>(new HumContext(), "SELF");

        return options;
    }



    static Assembly AssemblyLoad(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return null;
    }


    public static void Main()
    {
        GLOBAL.S[1] = "This is Global String Variable.";
        RemoveDir("../../../../Scripts/obj");
        RemoveDir("../../../../Scripts/bin");


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
            //GC
            var obj = new byte[1024 * 1024];
            Thread.Sleep(10);
        }
        GC.Collect();
        Console.WriteLine("=====================================================================================");
        Console.ReadKey();
    }

    private static void ScriptManager_Unloaded(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}:{obj.UniqueId}]卸载完毕.");
    }

    private static void ScriptManager_Unloading(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}:{obj.UniqueId}]卸载请求.");
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


