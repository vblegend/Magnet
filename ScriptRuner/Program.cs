// See https://aka.ms/new-console-template for more information


using App.Core;
using Magnet;

using ScriptRuner;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


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
        //options.AddUsings("Magnet.Proxy");
        options.WithAllowAsync(false);
        options.AddReferences("System.Threading.Thread");
        options.AddReferences("System.Console");
        options.AddReferences<MagnetScript>();
        options.AddReferences<LoginContext>();
        options.WithDirectory("../../../../Scripts");
        // thread
        options.ReplaceType(typeof(System.Threading.Thread), typeof(Magnet.Proxy.Thread));
        options.ReplaceType(typeof(System.Threading.ThreadPool), typeof(Magnet.Proxy.ThreadPool));
        options.ReplaceType(typeof(System.Threading.Tasks.Task), typeof(Magnet.Proxy.Task));

        // code safe
        options.ReplaceType(typeof(System.Activator), typeof(Magnet.Proxy.Activator));
        options.ReplaceType(typeof(System.Type), typeof(Magnet.Proxy.Type));
        options.ReplaceType(typeof(System.Reflection.Assembly), typeof(Magnet.Proxy.Assembly));
        options.ReplaceType(typeof(System.Reflection.Emit.DynamicMethod), typeof(Magnet.Proxy.DynamicMethod));
        options.ReplaceType(typeof(System.Linq.Expressions.DynamicExpression), typeof(Magnet.Proxy.DynamicMethod));
        options.ReplaceType(typeof(System.Linq.Expressions.Expression), typeof(Magnet.Proxy.DynamicMethod));
        options.ReplaceType(typeof(System.Runtime.CompilerServices.CallSite), typeof(Magnet.Proxy.DynamicMethod));
        options.ReplaceType(typeof(System.Runtime.InteropServices.DllImportAttribute), typeof(Magnet.Proxy.DllImportAttribute));
        //
        options.ReplaceType(typeof(System.Environment), typeof(Magnet.Proxy.Environment));
        options.ReplaceType(typeof(System.Diagnostics.Process), typeof(Magnet.Proxy.Process));
        options.ReplaceType(typeof(System.Runtime.InteropServices.Marshal), typeof(Magnet.Proxy.Marshal));

        // IO
        options.ReplaceType(typeof(System.IO.File), typeof(Magnet.Proxy.File));
        options.ReplaceType(typeof(System.IO.Directory), typeof(Magnet.Proxy.Directory));
        options.ReplaceType(typeof(System.IO.FileStream), typeof(Magnet.Proxy.FileStream));
        options.ReplaceType(typeof(System.IO.StreamWriter), typeof(Magnet.Proxy.StreamWriter));
        options.ReplaceType(typeof(System.IO.StreamReader), typeof(Magnet.Proxy.StreamReader));

        // NET
        options.ReplaceType(typeof(System.Net.Sockets.Socket), typeof(Magnet.Proxy.Socket));
        options.ReplaceType(typeof(System.Net.WebClient), typeof(Magnet.Proxy.WebClient));
        options.ReplaceType(typeof(System.Net.Http.HttpClient), typeof(Magnet.Proxy.HttpClient));


        //
        options.ReplaceType(typeof(System.Runtime.CompilerServices.ModuleInitializerAttribute), typeof(Magnet.Proxy.ModuleInitializerAttribute)); 
        


        


        options.AddInjectedObject<ObjectKilledContext>(new ObjectKilledContext());
        options.AddInjectedObject(GLOBAL);

        return options;
    }






    public static void Main()
    {
        RemoveDir("../../../../Scripts/obj");
        RemoveDir("../../../../Scripts/bin");


        var weakLogin = TestSccriptUnload();

        if (weakLogin.TryGetTarget(out var login))
        {
            var context = new LoginContext();
            context.UserName = "Administrator";
            login(context);
        }
        //System.Private.Xml, Version = 8.0.0.0, Culture = neutral, PublicKeyToken = cc7b13ffcd2ddd51
        // Magnet.Script, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null
        var scriptModule = AppDomain.CurrentDomain.GetAssemblies().Where(assembly=> assembly.FullName.StartsWith("Magnet.Script,")).FirstOrDefault();
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
                var state = scriptManager.CreateScriptState();
                state.SetFieldValue("ScriptB", "Value", 1.23456);
                var value = state.GetFieldValue("ScriptB", "Value");
            }

            using (new WatchTimer("Create Script State 1"))
            {
                var state = scriptManager.CreateScriptState();
            }


            using (new WatchTimer("Create Delegate 100000"))
            {
                var state = scriptManager.CreateScriptState();
                for (int i = 0; i < 100000; i++)
                {
                    state.MethodDelegate<LoginHandler>("ScriptA", "Login");
                }
            }


            List<MagnetState> states = new List<MagnetState>();
            using (new WatchTimer("Create State 1000"))
            {
                for (int i = 0; i < 1000; i++)
                {
                    var state = scriptManager.CreateScriptState();
                    states.Add(state);
                }
            }


            var stateTest = scriptManager.CreateScriptState();

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
            CallLogin(stateTest);
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
        List<MagnetState> states = new List<MagnetState>();
        var state = scriptManager.CreateScriptState();
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
        if (login.TryGetTarget(out var target)){
            target(context);
        }
   
    }




}


