using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language.Parser
{
    internal class LexerException : Exception
    {
        public string FileName { get; private set; }
        public int LineNumber { get; private set; }
        public int ColumnNumber { get; private set; }
        public Token Token { get; private set; }

        internal LexerException(string fileName, int lineNumber, int columnNumber, string message) : base(message)
        {
            ColumnNumber = columnNumber;
            FileName = fileName;
            LineNumber = lineNumber;
            Token = Token;
        }

        internal LexerException(string fileName, Token token, string message) : base(message)
        {
            ColumnNumber = token.ColumnNumber;
            FileName = fileName;
            LineNumber = token.LineNumber;
            Token = token;
        }

        public override string ToString()
        {
            return $"Line:{LineNumber} Column:{ColumnNumber} {GetType().Name.PadRight(15, ' ')} {Token.Value}";
        }
    }
}
