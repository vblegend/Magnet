using System;
using System.Runtime.CompilerServices;

namespace Language.Parser
{
    public enum TokenTyped
    {
        String,
        Number,
        Identifier,
        Punctuator,
        Comment,
        NewLine,
        WhiteSpace
    }

    public struct RuleTestResult
    {
        public Boolean Success;

        public Int32 LineCount;

        public Int32 ColumnNumber;

        public String Value;

        public Int32 Length;

        public TokenTyped Type;
    }

    public interface ILexicalRules
    {
        abstract RuleTestResult Test(in ReadOnlySpan<Char> codeSpan, in Int32 LineNumber, in Int32 ColumnNumber);

    }

}
