using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Magnet.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class ExportMethod
    {
        internal ExportMethod(MethodInfo methodInfo, String alias)
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

        internal AutowriredField(FieldInfo fieldInfo, Action<AbstractScript, Object> setter, Type requiredType, String slotName)
        {
            this.FieldInfo = fieldInfo;
            this.SlotName = slotName;
            this.RequiredType = requiredType;
            this.IsStatic = fieldInfo.IsStatic;
            this.Setter = setter;
        }
        /// <summary>
        /// Inject field information at a point
        /// </summary>
        public readonly FieldInfo FieldInfo;


        public readonly Action<AbstractScript, Object> Setter;

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
    public class ScriptMetaTable
    {
        unsafe internal ScriptMetaTable(Type scriptType, String scriptAlias, delegate*<AbstractScript> generater, Dictionary<String, ExportMethod> exportMethods, List<AutowriredField> autowriredTables)
        {
            this.Type = scriptType;
            this.Alias = scriptAlias;
            this.Generater = generater;
            this.ExportMethods = exportMethods;
            this.AutowriredTables = autowriredTables;
        }

        /// <summary>
        /// static method pointer for the ScriptType
        /// </summary>
        public readonly unsafe delegate*<AbstractScript> Generater;

        /// <summary>
        /// Type of the script
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Alias of the script
        /// </summary>
        public readonly String Alias;

        /// <summary>
        /// The injectable point of the script
        /// </summary>
        public readonly IReadOnlyList<AutowriredField> AutowriredTables;

        /// <summary>
        /// Export method of script
        /// </summary>
        public readonly IReadOnlyDictionary<String, ExportMethod> ExportMethods;
    }
}
