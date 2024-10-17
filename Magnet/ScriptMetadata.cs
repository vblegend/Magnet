using System;
using System.Collections.Generic;
using System.Reflection;

namespace Magnet
{

    internal class ScriptExportMethod
    {
        public string Alias { get; set; }
        public MethodInfo MethodInfo { get; set; }
    }

    internal class AutowriredField
    {
        public string Alias { get; set; }
        public Type RequiredType { get; set; }
        public FieldInfo FieldInfo { get; set; }
    }

    internal class ScriptMetadata
    {
        public Type ScriptType { get; set; }
        public String ScriptAlias { get;  set; }
        public List<AutowriredField> AutowriredFields { get; set; } = new List<AutowriredField>();
        public Dictionary<String, ScriptExportMethod> ExportMethods { get; set; } = new Dictionary<string, ScriptExportMethod>();
    }
}
