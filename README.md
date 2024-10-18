# What is Magnet?
--------------
Magnet is based on "Microsoft.CodeAnalysis.CSharp.Scripting" to develop a high performance c # game Script engine

On the basis of C# language and.NET framework, the script is safe, controllable, flexible and state

# Features

- [x] Load from file
- [x] Disable type
- [x] Unload 
- [x] Script state isolation
- [x] Script dependency injection
- [x] Debugger and braek;
- [x] Output assembly
- [x] References assembly
- [x] Generation method delegate
- [x] Illegal API detection
- [x] Global variable
- [x] Expandability
- [x] Script inter call



# Examples

``` csharp
    private static ScriptOptions Options(String name)
    {
        ScriptOptions options = new ScriptOptions();
        options.WithName(name);
        options.WithOutPutFile("123.dll");
        options.WithDebug(false);

        // options.WithRelease();
        options.WithAllowAsync(false);
        options.AddReferences<LoginContext>();
        options.WithDirectory("../../../../Scripts");


        // Insecure
        options.DisabledInsecureTypes();
        //
        options.SetAssemblyLoadCallback(AssemblyLoad);
        options.AddInjectedObject<ObjectKilledContext>(new ObjectKilledContext());
        options.AddInjectedObject(GLOBAL);
        options.AddInjectedObject<IObjectContext>(new HumContext(), "SELF");
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
        MagnetScript scriptManager = new MagnetScript(Options("My.Raffler"));
        var result = scriptManager.Compile();
        if (result.Success)
        {
            var state = scriptManager.CreateState();
            var weak = state.MethodDelegate<Action>("ScriptExample", "Hello");
            if (weak.TryGetTarget(out var target))
            {
                target();
                target = null;
            }

            var weakSetter = state.PropertySetterDelegate<Double>("ScriptExample", "Target");
            if (weakSetter != null && weakSetter.TryGetTarget(out var setter))
            {
                setter(1234.45);
                setter = null;
            }

            var weakGetter = state.PropertyGetterDelegate<Double>("ScriptExample", "Target");
            if (weakGetter != null && weakGetter.TryGetTarget(out var getter))
            {
                Console.WriteLine(getter());
                getter = null;
            }


            state.Dispose();
        }
        scriptManager.Unload();
    }


```


# Script Examples

``` csharp
    using Magnet.Core;
    using System;



    // A usable script must meet three requirements.
    // 1. The access must be public
    // 2. The Script Attribute must be marked
    // 3. The BaseScript class must be inherited

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