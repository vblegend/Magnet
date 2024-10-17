using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet
{
    public sealed class ScriptExistException : Exception
    {
        public String Name { get; private set; }

        public ScriptExistException(string name)
        {
            Name = name;    
        }
    }

    public sealed class ScriptUnloadFailureException : Exception
    {

    }



}
