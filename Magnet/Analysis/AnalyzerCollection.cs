using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Magnet.Analysis
{
    internal class AnalyzerCollection : IDisposable
    {
        private readonly List<IAnalyzer> _analyzers = new List<IAnalyzer>();

        public AnalyzerCollection(List<IAnalyzer> analyzers)
        {
            _analyzers = new List<IAnalyzer>(analyzers);
        }

        public void Add(IAnalyzer analyzer)
        {
            _analyzers.Add(analyzer);
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

        public void DefineInstance(ScriptMetadata metadata, AbstractScript script, IStateContext context)
        {
            foreach (var analyzer in _analyzers)
            {
                if (analyzer is IInstanceAsalyzer instanceAsalyzer)
                {
                    instanceAsalyzer.DefineInstance(metadata, script, context);
                }
            }
        }

        public void Dispose()
        {
            foreach (IAnalyzer analyzer in _analyzers)
            {
                analyzer.Dispose();
            }
            _analyzers.Clear();
        }
    }


}
