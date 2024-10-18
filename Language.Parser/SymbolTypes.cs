using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language.Parser
{
    internal enum SymbolTypes
    {
        Typed,
        NullValue,
        BooleanValue,
        KeyWord,
        Operator,
        Identifier,
        Punctuator,
    }
}
