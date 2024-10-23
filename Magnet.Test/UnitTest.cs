using App.Core;
using App.Core.Events;
using App.Core.Timer;
using Magnet.Test.Timer;
using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Magnet.Test
{
    public class Tests
    {

        private static GlobalVariableStore GLOBAL = new GlobalVariableStore();

        private static ScriptOptions Options(String name)
        {
            ScriptOptions options = new ScriptOptions();
            options.WithName(name);
            options.WithOutPutFile("123.dll");
            options.WithDebug(false);

            //options.WithRelease();
            options.WithAllowAsync(false);
            options.AddReferences<GameScript>();
            options.WithDirectory("../../../../Scripts");
            options.WithPreprocessorSymbols("USE_FILE");

            var timerProvider = new TimerProvider();
            options.AddAnalyzer(timerProvider);
            options.RegisterProvider(timerProvider);

            // Insecure
            options.DisabledInsecureTypes();
            //
            options.WithAssemblyLoadCallback(AssemblyLoad);
            options.RegisterProvider<ObjectKilledContext>(new ObjectKilledContext());
            options.RegisterProvider(GLOBAL);
            options.RegisterProvider<IObjectContext>(new HumContext(), "SELF");

            return options;
        }




        static Assembly? AssemblyLoad(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            return null;
        }


        private MagnetScript? scriptManager = null;
        private MagnetState? state = null;


        [TearDown]
        public void TearDown()
        {
            state?.Dispose();
        }



        [SetUp]
        public void Compile()
        {
            Console.WriteLine("Compile");
            RemoveDir("../../../../Scripts/obj");
            RemoveDir("../../../../Scripts/bin");
            scriptManager = new MagnetScript(Options("My.Raffler"));
            scriptManager.Unloading += ScriptManager_Unloading;
            scriptManager.Unloaded += ScriptManager_Unloaded;
            var result = scriptManager.Compile();
            if (result.Success)
            {
                var stateOptions = StateOptions.Default;
                stateOptions.Identity = 666;
                stateOptions.RegisterProvider(new TimerService());
                state = scriptManager.CreateState(stateOptions);
            }
            else
            {
                foreach (var item in result.Diagnostics)
                {
                    Console.WriteLine(item.ToString());
                }
                Assert.Fail();
            }
        }




        [Test]
        public void CreateState_100000()
        {


            List<MagnetState> states = new List<MagnetState>();
            using (new WatchTimer("Create State 100000"))
            {
                for (int i = 0; i < 100000; i++)
                {
                    var stateOptions = StateOptions.Default;
                    stateOptions.RegisterProvider(new TimerService());
                    var state = scriptManager?.CreateState(stateOptions);
                    states.Add(state!);
                }
            }


            using (new WatchTimer("Dispose State 100000"))
            {
                foreach (var state in states)
                {
                    state.Dispose();
                }
            }
        }




        [Test]
        public void CreateMethodDelegate_100000()
        {

            using (new WatchTimer("Create Delegate 100000"))
            {
                var state = scriptManager?.CreateState();
                for (int i = 0; i < 100000; i++)
                {
                    state?.MethodDelegate<Action>("ScriptA", "Login");
                }
                state = null;
            }
        }


        [Test]
        public void TestSccriptUnload()
        {
            MagnetScript scriptManager = new MagnetScript(Options("Unload.Test"));
            scriptManager.Unloading += ScriptManager_Unloading;
            scriptManager.Unloaded += ScriptManager_Unloaded;

            var result = scriptManager.Compile();
            if (!result.Success)
            {
                foreach (var item in result.Diagnostics)
                {
                    Console.WriteLine(item.ToString());
                }
                Assert.Fail();
            }
            List<MagnetState> states = new List<MagnetState>();
            var state = scriptManager.CreateState();
            var weak = state.MethodDelegate<Action>("ScriptA", "Login");
            state.Dispose();
            scriptManager.Unload();
            while (scriptManager.Status == ScrriptStatus.Unloading && scriptManager.IsAlive)
            {
                // GC
                var obj = new byte[1024 * 1024];
                Thread.Sleep(10);
            }
        }





        [Test]
        public void CallScriptMethod()
        {
            var weak = state?.MethodDelegate<Action>("ScriptA", "Main");
            if (weak != null && weak.TryGetTarget(out var handler2))
            {
                handler2();
                handler2 = null;
                Assert.Pass();
            }
            Assert.Fail();
        }


        [Test]
        public void PropertySetter()
        {
            var weakSetter = state?.PropertySetterDelegate<Double>("ScriptExample", "Target");
            if (weakSetter != null && weakSetter.TryGetTarget(out var setter))
            {
                setter(123.45);
                setter = null;
                Assert.Pass();
            }
            Assert.Fail();
        }


        [Test]
        public void PropertyGetter()
        {
            var weakGetter = state?.PropertyGetterDelegate<Double>("ScriptExample", "Target");
            if (weakGetter != null && weakGetter.TryGetTarget(out var getter))
            {
                Console.WriteLine(getter());
                getter = null;
                Assert.Pass();
            }
            Assert.Fail();
        }


        [Test]
        public void ScriptTypeOf()
        {
            var weakAttackEvent = state?.ScriptAs<IPlayLifeEvent>();
            if (weakAttackEvent != null && weakAttackEvent.TryGetTarget(out var attackEvent))
            {
                attackEvent.OnOnline(null);
                attackEvent = null;
                Assert.Pass();
            }
            Assert.Fail();
        }











        [Test]
        public void DisposeState()
        {
            state?.Dispose();
            state = null;
        }


        private static void ScriptManager_Unloaded(MagnetScript obj)
        {
            Console.WriteLine($"Ω≈±æ[{obj.Name}({obj.UniqueId})]–∂‘ÿÕÍ±œ.");
        }

        private static void ScriptManager_Unloading(MagnetScript obj)
        {
            Console.WriteLine($"Ω≈±æ[{obj.Name}({obj.UniqueId})]–∂‘ÿ«Î«Û.");
        }



        private static void RemoveDir(String dirPath)
        {
            var rootDir = Path.GetFullPath(dirPath);
            if (Directory.Exists(rootDir))
            {
                Directory.Delete(rootDir, true);
            }
        }
    }
}