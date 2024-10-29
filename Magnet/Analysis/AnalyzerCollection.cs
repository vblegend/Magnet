using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Magnet.Analysis
{
    internal class AnalyzerCollection
    {
        private readonly List<IAnalyzer> _analyzers = new List<IAnalyzer>();
        private readonly List<IInstanceAsalyzer> _instanceAnalyzers = new List<IInstanceAsalyzer>();



        public AnalyzerCollection(List<IAnalyzer> analyzers)
        {
            foreach (var analyzer in analyzers)
            {
                if (analyzer is IInstanceAsalyzer instanceAnalyzer) _instanceAnalyzers.Add(instanceAnalyzer);
                _analyzers.Add(analyzer);
            }
        }

        public void DefineAssembly(Assembly assembly)
        {
            foreach (var analyzer in _analyzers)
            {
                if (analyzer is IAssemblyAnalyzer assemblyAnalyzer)
                {
                    assemblyAnalyzer.DefineAssembly(assembly);
                }
            }
        }

        public void DefineType(Type scriptType)
        {
            foreach (var analyzer in _analyzers)
            {
                if (analyzer is ITypeAnalyzer typeAnalyzer)
                {
                    typeAnalyzer.DefineType(scriptType);
                }
            }
        }


        public void DefineInstance(ScriptMetaTable metadata, AbstractScript script, IStateContext context)
        {
            foreach (var analyzer in _instanceAnalyzers)
            {
                analyzer.DefineInstance(metadata, script, context);
            }
        }

        public void ConnectTo(MagnetScript magnet)
        {
            foreach (IAnalyzer analyzer in _analyzers)
            {
                analyzer.Connect(magnet);
            }
        }

        public void Disconnect(MagnetScript magnet)
        {
            foreach (IAnalyzer analyzer in _analyzers)
            {
                analyzer.Disconnect(magnet);
            }
            _analyzers.Clear();
        }
    }


}
