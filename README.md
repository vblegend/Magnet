
<p align="center" > <span>Magnet</span> </p>
<div align=center><img src="icon.png"></div>

# What is Magnet?
--------------
`Magnet是基于“Microsoft.CodeAnalysis.CSharp.Scripting”开发一个高性能的c#游戏脚本引擎`<br/>  
Magnet is based on "Microsoft.CodeAnalysis.CSharp.Scripting" to develop a high performance c # game Script engine

`在c#语言和NET框架下，脚本具有安全、可控、灵活、状态好等特点`<br/>  
On the basis of C# language and.NET framework, the script is safe, controllable, flexible and state


# Features|功能

- [x] Load from file | 从文件编译加载脚本、从程序集加载脚本、仅编译脚本
- [x] Rewrite Type | 提供类型重写器以替换脚本内使用的类型
- [x] Unload | 脚本程序集卸载
- [x] Disable Namespace | 禁用命名空间
- [x] Script state isolation | 脚本状态隔离
- [x] Script dependency injection | 脚本依赖注入
- [x] Debugger and braek; | 调试和断点
- [x] Output assembly | 输出程序集
- [x] References assembly | 增加引用程序集
- [x] Generation method delegate | 生成方法委托
- [x] Illegal API detection | 非法API检测
- [x] Global variable | 全局变量
- [x] Expandability | 可扩展性
- [x] Script inter call | 脚本之间调用
- [x] Custom Analysis | 自定义分析器扩展



# Examples|例子

``` csharp
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

        // 替换类型
        options.AddReplaceType(typeof(Task), typeof(Task));
        // 禁用类型
        options.DisableType(typeof(Task));
        // 禁用命名空间
        options.DisableNamespace(typeof(Thread));
        //禁用不安全类型与命名空间
        options.DisableInsecureTypes();

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




    private static WeakReference<Action> TestScriptUnload()
    {
        MagnetScript scriptManager = new MagnetScript(Options("Unload.Test"));
        var result = scriptManager.Compile();
        if (!result.Success)
        {
            foreach (var item in result.Diagnostics)
            {
                Console.WriteLine(item.ToString());
            }
            return null;
        }
        var state = scriptManager.CreateState();
        var weak = state.MethodDelegate<Action>("ScriptExample", "Hello");
        state.Dispose();
        scriptManager.Unload();
        return weak;
    }

    public static void Main()
    {
        MagnetScript scriptManager = new MagnetScript(Options("My.Script"));
        scriptManager.Unloading += ScriptManager_Unloading;
        scriptManager.Unloaded += ScriptManager_Unloaded;

        var result = scriptManager.Compile();
        foreach (var diagnostic in result.Diagnostics)
        {
            Console.WriteLine(diagnostic.ToString());
        }
        if (result.Success)
        {
            var stateOptions = StateOptions.Default;
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
        // wait gc unloaded assembly
        while (scriptManager.Status == ScriptStatus.Unloading && scriptManager.IsAlive)
        {
            var obj = new byte[1024 * 1024];
            Thread.Sleep(10);
        }
    }

    private static void ScriptManager_Unloaded(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}:{obj.UniqueId}]卸载完毕.");
    }

    private static void ScriptManager_Unloading(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}:{obj.UniqueId}]卸载请求.");
    }

```


# Script Examples|脚本例子

``` csharp
    using Magnet.Core;
    using System;



    // A usable script must meet three requirements.
    // 1. The access must be public
    // 2. The [ScriptAttribute] must be marked
    // 3. The AbstractScript class must be inherited

    [Script(nameof(ScriptExample))]
    public class ScriptExample : AbstractScript
    {
        [Autowired("SELF")]
        protected readonly IObjectContext? SELF;

        [Autowired]
        protected readonly GlobalVariableStore? GLOBAL;

        [Function("Hello")]
        public void Hello()
        {
            this.PRINT($"Hello Wrold!");

            // call script method
            Call("ScriptB", "Test", []);
            Call("ScriptB", "PrintMessage", ["Help"]);
            TryCall("ScriptB", "PrintMessage1", ["Help"]);
            Script<ScriptB>().PrintMessage("AAA");
            Script<ScriptB>((script) =>
            {
                script.PrintMessage("BBB");
            });

        }



        public Double Target
        {
            get
            {
                return 3.14;
            }
            set
            {
                this.PRINT(value);
            }
        }
    }
```