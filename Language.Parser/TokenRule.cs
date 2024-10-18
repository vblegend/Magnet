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

    public abstract class TokenRules
    {
        public abstract RuleTestResult Test(in ReadOnlySpan<Char> codeSpan, in Int32 LineNumber, in Int32 ColumnNumber);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe Boolean IsNumber(char lpChar)
        {
            return (lpChar >= '0' && lpChar <= '9');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe Boolean IsChinese(char lpChar)
        {
            return (lpChar >= 0x4e00 && lpChar <= 0x9fbb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe Boolean IsLetter(char lpChar)
        {
            return (lpChar >= 'a' && lpChar <= 'z') || (lpChar >= 'A' && lpChar <= 'Z');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe Boolean canEscape(char lpChar, out char outchar)
        {
            Char? c = null;
            if (lpChar == 'a') c = '\a';
            if (lpChar == 'b') c = '\b';
            if (lpChar == 'f') c = '\f';
            if (lpChar == 'n') c = '\n';
            if (lpChar == 'r') c = '\r';
            if (lpChar == 't') c = '\t';
            if (lpChar == 'v') c = '\v';
            if (lpChar == '0') c = '\0';
            if (lpChar == '\\') c = '\\';
            if (lpChar == '\'') c = '\'';
            if (lpChar == '"') c = '"';
            if (c.HasValue)
            {
                outchar = c.Value;
                return true;
            }
            else
            {
                outchar = '\0';
                return false;
            }
        }
    }

}
