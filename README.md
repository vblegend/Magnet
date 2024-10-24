
<p align="center" > <span>Magnet</span> </p>

<div align=center><img src="icon.png"></div>


# What is Magnet?
--------------
`Magnet�ǻ��ڡ�Microsoft.CodeAnalysis.CSharp.Scripting������һ�������ܵ�c#��Ϸ�ű�����`<br/>  
Magnet is based on "Microsoft.CodeAnalysis.CSharp.Scripting" to develop a high performance c # game Script engine

`��c#���Ժ�NET����£��ű����а�ȫ���ɿء���״̬�õ��ص�`<br/>  
On the basis of C# language and.NET framework, the script is safe, controllable, flexible and state


# Features|����

- [x] Load from file | ���ļ�������ؽű�
- [x] Rewrite Type | �ṩ������д�����滻�ű���ʹ�õ�����
- [x] Unload | �ű�����ж��
- [x] Disable Namespace | ���������ռ�
- [x] Script state isolation | �ű�״̬����
- [x] Script dependency injection | �ű�����ע��
- [x] Debugger and braek; | ���ԺͶϵ�
- [x] Output assembly | �������
- [x] References assembly | �������ó���
- [x] Generation method delegate | ���ɷ���ί��
- [x] Illegal API detection | �Ƿ�API���
- [x] Global variable | ȫ�ֱ���
- [x] Expandability | ����չ��
- [x] Script inter call | �ű�֮�����
- [x] Custom Analysis | �Զ����������չ



# Examples|����

``` csharp
    private static ScriptOptions Options(String name)
    {
        ScriptOptions options = ScriptOptions.Default;
        options.WithName(name);
        options.WithOutPutFile("123.dll");
        options.WithDebug(false);

        //options.WithRelease();
        options.WithAllowAsync(true);
        options.WithDirectory("../../../../Scripts");
        options.WithPreprocessorSymbols("USE_FILE");

        options.AddReferences<GameScript>();

        var timerProvider = new TimerProvider();
        options.AddAnalyzer(timerProvider);
        options.RegisterProvider(timerProvider);

        options.DisableNamespace(typeof(Thread));

        // Insecure
        //options.DisabledInsecureTypes();

        options.WithTypeRewriter(new TypeRewriter());
        options.UseDefaultSuppressDiagnostics();
        options.WithAssemblyLoadCallback(AssemblyLoad);
        options.RegisterProvider<ObjectKilledContext>(new ObjectKilledContext());
        options.RegisterProvider(GLOBAL);
        options.RegisterProvider<IObjectContext>(new HumContext(), "SELF");

        return options;
    }




    private static WeakReference<Action> TestSccriptUnload()
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
        while (scriptManager.Status == ScrriptStatus.Unloading && scriptManager.IsAlive)
        {
            var obj = new byte[1024 * 1024];
            Thread.Sleep(10);
        }
    }

    private static void ScriptManager_Unloaded(MagnetScript obj)
    {
        Console.WriteLine($"�ű�[{obj.Name}:{obj.UniqueId}]ж�����.");
    }

    private static void ScriptManager_Unloading(MagnetScript obj)
    {
        Console.WriteLine($"�ű�[{obj.Name}:{obj.UniqueId}]ж������.");
    }

```


# Script Examples|�ű�����

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