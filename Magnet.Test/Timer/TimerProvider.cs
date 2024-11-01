using Magnet.Core;
using Magnet.Analysis;
using System.Reflection;
using App.Core.Timer;



namespace Magnet.Test.Timer
{
    public class TimerProvider : ITimerManager, ITypeAnalyzer
    {
        private struct TimerInfo
        {
            public MethodInfo MethodInfo;
            public TimerAttribute Options;
        }

        private ITimerService? timerService;

        private Dictionary<int, TimerInfo> timersDefine = new Dictionary<int, TimerInfo>();

        public void Dispose()
        {
            timersDefine.Clear();
        }

        public void SetTimerService(ITimerService? timerService)
        {
            this.timerService = timerService;
        }


        public void EnableTimer(IScriptInstance context, int timerIndex)
        {
            if (timerService == null) return;
            var state = context.GetStateContext();
            if (state != null)
            {
                if (timersDefine.TryGetValue(timerIndex, out var timerInfo))
                {
                    var script = state.FirstAs<AbstractScript>(timerInfo.MethodInfo.DeclaringType);
                    if (script != null)
                    {
                        var callback = (Action)timerInfo.MethodInfo.CreateDelegate(typeof(Action), script);
                        var interval = (uint)timerInfo.Options.Unit * timerInfo.Options.Interval;
                        timerService.Enable(timerIndex, callback, interval);
                    }
                }
                else
                {
                    state.Output.Write(MessageType.Warning, $"无效的Timer {timerIndex}");
                }
            }
        }

        public void DisableTimer(IScriptInstance context, int timerIndex)
        {
            if (timerService == null) return;
            var state = context.GetStateContext();
            if (state != null)
            {
                if (timersDefine.TryGetValue(timerIndex, out var timerInfo))
                {
                    timerService.Disable(timerIndex);
                }
                else
                {
                    state.Output.Write(MessageType.Warning, $"无效的Timer {timerIndex}");
                }
            }
        }

        public void ClearTimer(IScriptInstance context)
        {
            if (timerService == null) return;
            var state = context.GetStateContext();
            if (state != null)
            {
                timerService.Clear();
            }
        }


        #region Analyzers

        void ITypeAnalyzer.DefineType(Type type)
        {
            var fieldList = new List<MethodInfo>();
            while (type != null)
            {
                var _methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var methodInfo in _methods)
                {
                    var timerAttr = methodInfo.GetCustomAttribute<TimerAttribute>();
                    if (timerAttr != null)
                    {
                        var attributeType = timerAttr.GetType();
                        if (timersDefine.TryGetValue(timerAttr.TimerIndex, out var timeInfo))
                        {
                            Console.WriteLine($"Repetitive Timer Index: {timerAttr.TimerIndex}  existing: {timeInfo.MethodInfo.DeclaringType!.Name}.{timeInfo.MethodInfo.Name}, ignored: {methodInfo.DeclaringType!.Name}.{methodInfo.Name}.");
                            continue;
                        }
                        timersDefine.Add(timerAttr.TimerIndex, new TimerInfo() { MethodInfo = methodInfo, Options = timerAttr });
                    }
                }
                type = type.BaseType!;
            }
        }

        void IAnalyzer.Connect(MagnetScript magnet)
        {
 
        }

        void IAnalyzer.Disconnect(MagnetScript magnet)
        {
   
        }



        #endregion

    }
}
