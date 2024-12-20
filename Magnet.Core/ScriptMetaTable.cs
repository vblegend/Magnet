﻿using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Magnet.Core
{

    /// <summary>
    /// The exported method in the script is marked with [Function]
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
    /// Injectable fields in the script marked with [Autowrired]
    /// </summary>
    public class AutowriredField
    {

        internal AutowriredField(FieldInfo fieldInfo, Action<AbstractScript, Object> setter, Type requiredType, String slotName)
        {
            this.SlotName = slotName;
            this.RequiredType = requiredType;
            this.IsStatic = fieldInfo.IsStatic;
            this.Setter = setter;
            this.FieldType = fieldInfo.FieldType;
            this.DeclaringType = fieldInfo.DeclaringType;
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly Type DeclaringType;

        /// <summary>
        /// The type of data required for the injection point
        /// </summary>
        public readonly Type RequiredType;

        /// <summary>
        /// Slot name of the injection point, any matching type if empty
        /// </summary>
        public readonly String SlotName;

        /// <summary>
        /// Autowrired field type
        /// </summary>
        public readonly Type FieldType;

        /// <summary>
        /// Field is static
        /// </summary>
        public readonly Boolean IsStatic;

        /// <summary>
        /// Whether the static field has been filled
        /// </summary>
        internal Boolean IsFilled;

        /// <summary>
        /// field setter
        /// </summary>
        public readonly Action<AbstractScript, Object> Setter;

    }

    /// <summary>
    /// Meta Infomation Table of the script
    /// </summary>
    public class ScriptMetaTable
    {
        internal ScriptMetaTable(Type scriptType, String scriptAlias, Func<AbstractScript> generater, Dictionary<String, ExportMethod> exportMethods, AutowriredField[] autowriredTables)
        {
            this.Type = scriptType;
            this.Alias = scriptAlias;
            this.Generater = generater;
            this.ExportMethods = exportMethods;
            this.AutowriredTables = autowriredTables;
        }

        /// <summary>
        /// script object constructors
        /// </summary>
        public readonly Func<AbstractScript> Generater;

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
        public readonly AutowriredField[] AutowriredTables;

        /// <summary>
        /// Export method of script
        /// </summary>
        public readonly IReadOnlyDictionary<String, ExportMethod> ExportMethods;
    }
}
