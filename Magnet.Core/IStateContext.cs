﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet.Core
{
    public interface IStateContext
    {
        public T InstanceOfType<T>() where T : BaseScript;
        public BaseScript InstanceOfType(Type type);
        public BaseScript InstanceOfName(String scriptName);
        public ScriptRunMode RunMode { get; }
        public Boolean UseDebuggerBreak { get; }



    }
}