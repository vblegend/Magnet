
using System;

namespace Magnet.Core
{


    /// <summary>
    /// Marking the member as a global variable prevents the compiler from generating a warning message
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class GlobalAttribute : Attribute
    {
    }
}
