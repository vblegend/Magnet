// See https://aka.ms/new-console-template for more information


using App.Core;
using App.Core.Events;
using App.Core.Timer;
using Magnet;
using Magnet.Core;
using Microsoft.CodeAnalysis;
using ScriptRuner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;

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

        // #3 从脚本文件编译并加载, 仅扫描顶层目录
        options.WithCompileKind(CompileKind.CompileAndLoadAssembly);
        options.WithRecursiveScanning(false);
        options.WithScanDirectory("../../../../Scripts");

        // 定义自定义的编译宏符号
        options.WithCompileSymbols("USE_FILE", "USE_PRINT");

        // 是否支持异步
        options.WithAllowAsync(false);

        // 添加程序集引用
        options.AddReferences<GameScript>();
        options.AddReferences("System.Collections");

        var timerProvider = new TimerProvider();
        // 增加一个分析器
        options.AddAnalyzer(timerProvider);

        // 是否支持不安全代码
        options.WithAllowUnsafe(true);


        //// 将SW001诊断提升至Error，没有标记[ScriptAttribute]的脚本对象会导致编译失败
        //options.AddDiagnosticSuppress("SW001", Microsoft.CodeAnalysis.ReportDiagnostic.Error);
        //// 将SW002诊断提升至Error，没有继承AbstractScript的脚本对象会导致编译失败
        //options.AddDiagnosticSuppress("SW002", Microsoft.CodeAnalysis.ReportDiagnostic.Error);
        //// 将SW003诊断提升至Error，没有标记[GlobalAttribute]的静态字段会导致编译失败
        //options.AddDiagnosticSuppress("SW003", Microsoft.CodeAnalysis.ReportDiagnostic.Error);
        //// 将SW004诊断提升至Error，没有标记[GlobalAttribute]的静态属性会导致编译失败
        //options.AddDiagnosticSuppress("SW004", Microsoft.CodeAnalysis.ReportDiagnostic.Error);




        // 替换类型
        // options.AddReplaceType(typeof(Task), typeof(MyTask));.

        //禁用类型
        //options.DisableType(typeof(Type));

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
        options.AddReplaceType("ScriptA.ABCD", typeof(HashSet<String>));

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


    class ScriptX : AbstractScript { }





    public static void Main()
    {
        GLOBAL.S[1] = "This is Global String Variable.";
        MagnetScript scriptManager = new MagnetScript(Options("My.Raffler"));
        scriptManager.Unloading += ScriptManager_Unloading;
        scriptManager.Unloaded += ScriptManager_Unloaded;
        ICompileResult result = null;

        using (new WatchTimer("Script Compile"))
        {
            result = scriptManager.Compile();
        }

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
            stateOptions.RegisterProvider(new TimerService());
            var stateTest = scriptManager.CreateState(stateOptions);
            //var t = typeof(AbstractScript);

            //using (new WatchTimer("FirstAs 10000000"))
            //{
            //    for (int i = 0; i < 10000000; i++)
            //    {
            //       var r1= stateTest.FirstAs<AbstractScript>(t);
            //    }
            //}


            //using (new WatchTimer("FirstAs 10000000"))
            //{
            //    for (int i = 0; i < 10000000; i++)
            //    {
            //        var r1 = stateTest.FirstAs<AbstractScript>(t);
            //    }
            //}


            //using (new WatchTimer("FirstAs 10000000"))
            //{
            //    for (int i = 0; i < 10000000; i++)
            //    {
            //        var r1 = stateTest.FirstAs<AbstractScript>();
            //    }
            //}


            //using (new WatchTimer("FirstAs 10000000"))
            //{
            //    for (int i = 0; i < 10000000; i++)
            //    {
            //        var r1 = stateTest.FirstAs<AbstractScript>();
            //    }
            //}


            for (int y = 0; y < 10; y++)
            {
                using (new WatchTimer("CreateState 100000"))
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        var stateOption1s = StateOptions.Default;
                        stateOption1s.RegisterProvider(new TimerService());
                        scriptManager.CreateState(stateOption1s);
                    }
                }
            }

            //var weakIsTypeEqual = stateTest.CreateDelegate<Func<Type, Boolean>>("ScriptExample", "IsTypeEqual");
            //if (weakIsTypeEqual.TryGetTarget(out var isTypeEqual))
            //{
            //    var bol = isTypeEqual(typeof(AbstractScript));
            //    //为什么传个type进去会导致卸载不掉？？？？
            //    Console.WriteLine(bol);
            //    isTypeEqual = null;
            //}


            var weakMain = stateTest.CreateDelegate<Action>("ScriptA", "Main");
            if (weakMain != null && weakMain.TryGetTarget(out var main))
            {
                using (new WatchTimer("With Call Main() x10"))
                    for (int i = 0; i < 10; i++)
                    {
                        main();
                    }

                main = null;
            }
            var weakPlayerLife = stateTest.FirstAs<IPlayLifeEvent>();
            if (weakPlayerLife != null && weakPlayerLife.TryGetTarget(out var lifeEvent))
            {
                using (new WatchTimer("With Call OnOnline()")) lifeEvent.OnOnline(null);
                using (new WatchTimer("With Call OnOnline()")) lifeEvent.OnOnline(null);
                lifeEvent = null;
            }
            stateTest = null;
            scriptManager.Unload(true);

            using (new WatchTimer("time()"))
            {
                Console.WriteLine();
            }


        }

        while (scriptManager.Status == ScrriptStatus.Unloading && scriptManager.IsAlive)
        {
            // Trigger GC
            var obj = new byte[1024 * 1024];
            Thread.Sleep(10);
        }
        GC.Collect();
        Console.WriteLine("=====================================================================================");

        while (true)
        {
            // Trigger GC, Recycling more memory
            var obj = new byte[1024 * 1024];
            Thread.Sleep(10);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Console.ReadKey();
    }

    private static void ScriptManager_Unloaded(MagnetScript obj)
    {
        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 脚本{obj}卸载完毕.");
    }

    private static void ScriptManager_Unloading(MagnetScript obj)
    {
        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 脚本{obj}卸载请求.");
    }


    private static void CallLogin(IMagnetState state)
    {
        var login = state.CreateDelegate<Action>("ScriptA", "Main");
        if (login.TryGetTarget(out var target))
        {
            target();
            target = null;
        }
        state = null;
    }




}


