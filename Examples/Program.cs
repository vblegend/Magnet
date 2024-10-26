﻿// See https://aka.ms/new-console-template for more information


using App.Core;
using App.Core.Events;
using App.Core.Timer;
using Magnet;
using ScriptRuner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;





public static class Program
{


    private static GlobalVariableStore GLOBAL = new GlobalVariableStore();

    private static ScriptOptions Options(String name)
    {
        ScriptOptions options = ScriptOptions.Default;
        // 脚本名称
        options.WithName(name);
        // 调试模式 不启用脚本内置debugger()函数
        options.WithDebug(false);
        // 发布模式 编译优化
        //options.WithRelease();


        // #1 仅编译，可输出
        options.WithCompileKind(CompileKind.Compile);
        options.WithOutPutFile("123.dll");

        // #2 从程序集文件加载
        options.WithCompileKind(CompileKind.LoadAssembly);
        options.WithScanDirectory("./");
        options.WithAssemblyFileName("123.dll");

        // #3 从脚本文件编译并加载
        options.WithCompileKind(CompileKind.CompileAndLoadAssembly);
        options.WithScanDirectory("../../../../Scripts");

        // 定义自定义的编译宏符号
        options.WithCompileSymbols("USE_FILE");

        // 是否支持异步
        options.WithAllowAsync(false);

        // 添加程序集引用
        options.AddReferences<GameScript>();

        var timerProvider = new TimerProvider();
        // 增加一个分析器
        options.AddAnalyzer(timerProvider);

        // 是否支持不安全代码
        options.WithAllowUnsafe(true);

        // 替换类型
        // options.AddReplaceType(typeof(Task), typeof(MyTask));

        //禁用类型
        options.DisableType(typeof(Task));

        // 禁用泛类型的严格类型
        //options.DisableType("System.Collections.Generic.List<string>");
        //options.DisableType(typeof(List<String>));

        // 禁用范类型的基础类型
        //options.DisableType("System.Collections.Generic.List");
        //options.DisableGenericBaseType(typeof(List<>));

        // 禁用命名空间
        //options.DisableNamespace(typeof(Thread));

        //禁用不安全类型与命名空间
        //options.DisableInsecureTypes();

        // 脚本类型重写器
        options.WithTypeRewriter(new TypeRewriter());
        // 使用默认的抑制诊断
        options.UseDefaultSuppressDiagnostics();
        // 脚本上下文依赖程序集加载Hook
        options.WithAssemblyLoadCallback(AssemblyLoad);
        // 注册依赖注入
        options.RegisterProvider(timerProvider);
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
        foreach (var diagnostic in result.Diagnostics)
        {
            if (diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
            if (diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error) Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(diagnostic.ToString());
            Console.ResetColor();
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



    private static void CallLogin(IMagnetState state)
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


