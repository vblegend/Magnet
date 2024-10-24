using System;
using System.Collections.Generic;
using System.Reflection;

namespace Magnet
{

    /// <summary>
    /// 
    /// </summary>
    public readonly struct ScriptExportMethod
    {
        internal ScriptExportMethod(MethodInfo methodInfo, String alias)
        {
            this.MethodInfo = methodInfo;
            this.Alias = alias;
        }
        /// <summary>
        /// Script method alias
        /// </summary>
        public readonly String Alias;
        /// <summary>
        /// Script method info
        /// </summary>
        public readonly MethodInfo MethodInfo;
    }


    /// <summary>
    /// 
    /// </summary>
    public readonly struct AutowriredField
    {

        internal AutowriredField(FieldInfo fieldInfo, Type requiredType, String slotName)
        {
            this.FieldInfo = fieldInfo;
            this.SlotName = slotName;
            this.RequiredType = requiredType;
        }
        /// <summary>
        /// Slot name of the injection point, any matching type if empty
        /// </summary>
        public readonly String SlotName;

        /// <summary>
        /// The type of data required for the injection point
        /// </summary>
        public readonly Type RequiredType;

        /// <summary>
        /// Inject field information at a point
        /// </summary>
        public readonly FieldInfo FieldInfo;
    }

    /// <summary>
    /// Meta information of the script
    /// </summary>
    public readonly struct ScriptMetadata
    {
        internal ScriptMetadata(Type scriptType, String scriptAlias)
        {
            this.ScriptType = scriptType;
            this.ScriptAlias = scriptAlias;
        }
        /// <summary>
        /// Type of the script object
        /// </summary>
        public readonly Type ScriptType;
        /// <summary>
        /// Alias of the script object
        /// </summary>
        public readonly String ScriptAlias;

        /// <summary>
        /// The injectable point of the script
        /// </summary>
        public readonly IReadOnlyList<AutowriredField> AutowriredFields  = new List<AutowriredField>();

        /// <summary>
        /// Export method of script
        /// </summary>
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
