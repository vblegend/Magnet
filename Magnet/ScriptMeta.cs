using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Magnet
{



    internal class ObjectProvider : IObjectProvider
    {
        internal ObjectProvider(Type type, Object value, String slotName)
        {
            this.Type = type;
            this.Value = value;
            this.SlotName = slotName;
        }
        public readonly Type Type;
        public readonly String SlotName;
        public readonly Object Value;

        Type IObjectProvider.Type => Type;

        string IObjectProvider.SlotName => SlotName;

        object IObjectProvider.Value => Value;

        public bool TypeIs<T>(string slotName = null)
        {
            return typeof(T) == this.Type && (String.IsNullOrEmpty(slotName) || slotName == SlotName);
        }
    }

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
        /// Script method info
        /// </summary>
        public readonly MethodInfo MethodInfo;
        /// <summary>
        /// Script method alias
        /// </summary>
        public readonly String Alias;
    }


    /// <summary>
    /// 
    /// </summary>
    public class AutowriredField
    {

        internal AutowriredField(FieldInfo fieldInfo, Type requiredType, String slotName)
        {
            this.FieldInfo = fieldInfo;
            this.SlotName = slotName;
            this.RequiredType = requiredType;
            this.IsStatic = fieldInfo.IsStatic;
        }
        /// <summary>
        /// Inject field information at a point
        /// </summary>
        public readonly FieldInfo FieldInfo;

        /// <summary>
        /// Field is static
        /// </summary>
        public readonly Boolean IsStatic;

        internal Boolean IsFilled;


        /// <summary>
        /// Slot name of the injection point, any matching type if empty
        /// </summary>
        public readonly String SlotName;

        /// <summary>
        /// The type of data required for the injection point
        /// </summary>
        public readonly Type RequiredType;








    }

    /// <summary>
    /// Meta information of the script
    /// </summary>
    public readonly struct ScriptMeta
    {
        unsafe internal ScriptMeta(Type scriptType, String scriptAlias, delegate*<AbstractScript> generater)
        {
            this.ScriptType = scriptType;
            this.ScriptAlias = scriptAlias;
            this.Generater = generater;
        }


        /// <summary>
        /// static method pointer for the ScriptType
        /// </summary>
        public readonly unsafe delegate*<AbstractScript> Generater;

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
        public readonly IReadOnlyList<AutowriredField> AutowriredTable = new List<AutowriredField>();

        /// <summary>
        /// Export method of script
        /// </summary>
        public readonly IReadOnlyDictionary<String, ScriptExportMethod> ExportMethodTable = new Dictionary<string, ScriptExportMethod>();



        internal void AddExportMethod(String key, ScriptExportMethod method)
        {
            (ExportMethodTable as Dictionary<string, ScriptExportMethod>).Add(key, method);
        }

        internal void AddAutowriredField(AutowriredField field)
        {
            (AutowriredTable as List<AutowriredField>).Add(field);
        }
    }
}
