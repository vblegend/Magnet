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
        options.AddReferences<MagnetScript>();
        options.AddReferences<LoginContext>();
        options.WithDirectory("../../../../Scripts");



        options.AddInjectedObject<IKilledContext>(new ObjectKilledContext());

        MagnetScript scriptManager = new MagnetScript(options);
        var result = scriptManager.Compile();
        if (result.Success)
        {
            Console.WriteLine();
            using (new WitchTimer("CreateScriptState"))
            {
                var state = scriptManager.CreateScriptState();
                state.SetVariable("ScriptB", "Value", 1.23456);
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


            using (new WitchTimer("Create Delegate 10000"))
            {
                for (int i = 0; i < 10000; i++)
                {
                    var state = scriptManager.CreateScriptState();
                    state.GetDelegate<LoginHandler>("ScriptA", "Login");
                }
            }




            List<MagnetState> states = new List<MagnetState>();
            using (new WitchTimer("Create State 10000"))
            {
                for (int i = 0; i < 10000; i++)
                {
                    var state = scriptManager.CreateScriptState();
                    states.Add(state);
                }
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










            Console.WriteLine($"HOST GLOBAL.VAR = {GLOBAL.STR[1]}");
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


