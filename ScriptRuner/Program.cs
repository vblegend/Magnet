// See https://aka.ms/new-console-template for more information


using App.Core;
using Magnet;

using ScriptRuner;


public static class Program
{
    public delegate void LoginHandler(LoginContext context);
    public static void Main()
    {

        RemoveDir("../../../../Scripts/obj");
        RemoveDir("../../../../Scripts/bin");

        ScriptOptions options = new ScriptOptions();
        options.WithDebug(false);

        //options.WithRelease();
        options.AddUsings("Magnet.Proxy");
        options.AddReferences("System.Threading.Thread");
        options.AddReferences<MagnetEngine>();
        options.AddReferences<LoginContext>();
        options.WithDirectory("../../../../Scripts");



        var v = new ObjectKilledContext();

        options.AddInjectedObject<IKilledContext>(v);

        MagnetEngine scriptManager = new MagnetEngine(options);
        var result = scriptManager.Compile();
        if (result.Success)
        {
            Console.WriteLine(  );
            using (new WitchTimer("CreateScriptState"))
            {
                var state = scriptManager.CreateScriptState();
                state.SetVariable("ScriptB", "Value",1.23456);
                Console.WriteLine($"Set ScriptB.Value = {1.23456};");
                CallLogin(state);
            }

            using (new WitchTimer("CreateScriptState"))
            {
                var state = scriptManager.CreateScriptState();
                var value = state.GetVariable("ScriptB", "Value");
                Console.WriteLine($"ScriptB.Value = {value};");
                CallLogin(state);
            }

            var stateTest = scriptManager.CreateScriptState();
            scriptManager.Unload();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            CallLogin(stateTest);

            //var weakDelegate = new WeakReference<Action>((Action)methodInfo.CreateDelegate(typeof(Action)));

            //// 使用 Delegate
            //if (weakDelegate.TryGetTarget(out var action))
            //{
            //    action();  // 只有在 Delegate 存在时调用
            //}


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
        var login = state.GetDelegate<LoginHandler>("ScriptA", "Login");
        var context = new LoginContext();
        context.UserName = "Administrator";
        login(context);
    }




}


