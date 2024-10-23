using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Magnet
{

    public readonly struct ScriptExportMethod
    {
        internal ScriptExportMethod(MethodInfo methodInfo, String alias)
        {
            this.MethodInfo = methodInfo;
            this.Alias = alias;
        }
        public readonly String Alias;
        public readonly MethodInfo MethodInfo;
    }

    public readonly struct AutowriredField
    {

        internal AutowriredField(FieldInfo fieldInfo, Type requiredType, String slotName)
        {
            this.FieldInfo = fieldInfo;
            this.SlotName = slotName;
            this.RequiredType = requiredType;
        }
        public readonly String SlotName;
        public readonly Type RequiredType;
        public readonly FieldInfo FieldInfo;
    }

    public readonly struct ScriptMetadata
    {
        internal ScriptMetadata(Type scriptType, String scriptAlias)
        {
            this.ScriptType = scriptType;
            this.ScriptAlias = scriptAlias;
        }

        public readonly Type ScriptType;
        public readonly String ScriptAlias;
        public readonly IReadOnlyList<AutowriredField> AutowriredFields  = new List<AutowriredField>();
        public readonly IReadOnlyDictionary<String, ScriptExportMethod> ExportMethods = new Dictionary<string, ScriptExportMethod>();

        internal void AddExportMethod(String key, ScriptExportMethod method)
        {
            (ExportMethods as Dictionary<string, ScriptExportMethod>).Add(key, method);
        }

        internal void AddAutowriredField(AutowriredField field)
        {
            (AutowriredFields as List<AutowriredField>).Add(field);
        }
    }
}
