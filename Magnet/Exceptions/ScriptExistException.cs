
using System;


namespace Magnet.Exceptions
{
    public sealed class ScriptExistException : Exception
    {
        public string Name { get; private set; }

        public ScriptExistException(string name)
        {
            Name = name;
        }
    }

    public sealed class ScriptUnloadFailureException : Exception
    {

    }



}
