using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language.Parser
{
    public enum ValueType
    {
        String,
        Number,
        Boolean,
        Null,
    }

    /// <summary>
    /// value string boolean number null
    /// </summary>
    public class ValueToken : Token
    {
        internal ValueToken()
        {
        }

        public ValueType Type { get; protected set; }

        public virtual string ToValue()
        {
            return Value;
        }
    }
    public class BooleanToken : ValueToken
    {
        internal BooleanToken()
        {
            this.Type = ValueType.Boolean;
        }
    }



    public class EndOfFileToken : Token
    {
        internal EndOfFileToken()
        {
            this.Symbol = Symbols.KW_EOF;
        }
    }
    public class IdentifierToken : Token
    {
        internal IdentifierToken()
        {
        }
    }
    public class KeywordToken : Token
    {
        internal KeywordToken()
        {
        }
    }
    public class NullToken : ValueToken
    {
        internal NullToken()
        {
            this.Type = ValueType.Null;
        }
    }
    public class NumberToken : ValueToken
    {
        internal NumberToken()
        {
            this.Type = ValueType.Number;
        }
    }

    public class PunctuatorToken : Token
    {
        internal PunctuatorToken()
        {
        }
    }

    public class OperatorToken : PunctuatorToken
    {
        internal OperatorToken()
        {
        }
    }

    public class StringToken : ValueToken
    {
        internal StringToken()
        {
            this.Type = ValueType.String;
        }

        public override string ToValue()
        {
            return $"'{this.Value.Replace("\r", "\\r").Replace("\n", "\\n")}'";
        }
    }





}
