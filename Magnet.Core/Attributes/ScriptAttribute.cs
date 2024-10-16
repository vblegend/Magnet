﻿using System;

namespace Magnet.Core
{


    /// <summary>
    /// Define a script object. The script object is instantiated by the state machine. The script object is independent from the state machine
    /// </summary>

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ScriptAttribute : Attribute
    {
        public ScriptAttribute()
        {
        }

        public ScriptAttribute(string alias)
        {
            this.Alias = alias;
        }

        public string Alias { get; set; }

        public string Description { get; set; }
    }
}
