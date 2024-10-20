using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Language.Parser
{
    public class SymbolProvider
    {
        private readonly Dictionary<string, Symbol> _SymbolMaps = new Dictionary<string, Symbol>();

        internal Symbol FromString(string name)
        {
            _SymbolMaps.TryGetValue(name, out var symbol);
            return symbol;
        }


        public void Register(Symbol symbol)
        {
            _SymbolMaps.Add(symbol.Name, symbol);
        }
    }




    public abstract class AbstractLexer
    {
        public Int32 LineNumber { get; private set; } = 1;
        public Int32 ColumnNumber { get; private set; } = 1;
        public String FullPath { get; private set; }
        public String FileName { get; private set; }
        public String InputData { get; private set; }
        public String Directory { get; private set; }

        private List<Token> tokens = new List<Token>();
        private List<ILexicalRules> _TokenRules { get; set; } = new List<ILexicalRules>();
        private Int32 readOffset { get; set; } = 0;
        private Int32 bufferLength { get; set; } = 0;
        public Int32 Position { get; private set; } = 0;


        private readonly SymbolProvider symbolProvider = new SymbolProvider();

        public AbstractLexer(String file, Encoding encoding) : this(File.ReadAllText(file, encoding), file)
        {
        }

        public AbstractLexer(String text, String file)
        {
            this.Directory = Path.GetDirectoryName(file)!;
            this.FullPath = file;
            this.FileName = Path.GetFileName(file);
            this.InputData = text.Replace("\r\n", "\n");
            this.bufferLength = this.InputData.Length;
            this.symbolProvider.Register(Symbol.EOF);
            this.RegisterSymbols(this.symbolProvider);
            this.RegisterTokenRegexs(this._TokenRules);
            this.ParseTokens();
            this.Position = 0;
        }

        private void AddRegex(ILexicalRules rule)
        {
            this._TokenRules.Add(rule);
        }

        protected abstract void RegisterTokenRegexs(List<ILexicalRules> tokenRules);

        protected abstract void RegisterSymbols(SymbolProvider symbolProvider);



        /// <summary>
        /// If it is the specified symbol, return, otherwise report an error
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Token NextOfKind(Symbol symbol)
        {
            var token = this.Next();
            if (token.Symbol != symbol)
            {
                throw new LexerException(this.FullPath, token.LineNumber, token.ColumnNumber, $"The keyword {token.Value} appears in the wrong place, it should be {symbol.Name}.");
            }
            return token;
        }

        /// <summary>
        /// If it is the specified token, return, otherwise report an error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T NextOfKind<T>() where T : Token
        {
            var token = this.Next();
            if (token is T) return (T)token;
            throw new InvalidOperationException("");
        }

        /// <summary>
        /// If it is the specified symbol, take it out and return true, otherwise return false
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T TestNextOfKind<T>() where T : Token
        {
            var nextToken = this.LookAtHead();
            if (nextToken is T)
            {
                this.Next();
                return (T)nextToken;
            }
            return null;
        }

        public Boolean TestAtHead<T>() where T : Token
        {
            var nextToken = this.LookAtHead();
            return nextToken is T;
        }

        /// <summary>
        /// If it is the specified symbol, take it out and return true, otherwise return false
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Boolean TestSymbol(Symbol symbol)
        {
            var nextToken = this.LookAtHead();
            return (nextToken.Symbol == symbol);
        }

        /// <summary>
        /// If it is the specified symbol, take it out and return true, otherwise return false
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Boolean TestNext(Symbol symbol)
        {
            var nextToken = this.LookAtHead();
            if (nextToken.Symbol == symbol)
            {
                this.Next();
                return true;
            }
            return false;
        }

        public Boolean LookAtHead(Symbol symbol)
        {
            var nextToken = this.tokens[this.Position];
            return nextToken.Symbol == symbol;
        }





        /// <summary>
        /// get next token without removing it.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token LookAtHead()
        {
            return this.tokens[this.Position];
        }

        /// <summary>
        /// get next token without removing it.
        /// </summary>
        /// <returns></returns>
        public Token Previous(Int32 offset = 2)
        {
            return this.tokens[this.Position - offset];
        }

        /// <summary>
        /// get next token
        /// </summary>
        /// <returns></returns>
        public Token Next()
        {
            var token = this.tokens[this.Position];
            this.Position++;
            return token;
        }

        public void RollBack()
        {
            this.Position--;
        }

        /// <summary>
        /// Parse all tokens
        /// </summary>
        private void ParseTokens()
        {
            while (true)
            {
                var token = this.ParseNext();
                this.tokens.Add(token);
                if (token == Token.EOF) return;
            }
        }

        /// <summary>
        /// Parse next token
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LexerException"></exception>
        private Token ParseNext()
        {
            if (this.bufferLength <= 0) return Token.EOF;
            ReadOnlySpan<Char> span = this.InputData.AsSpan(this.readOffset, this.bufferLength);
            foreach (var rule in this._TokenRules)
            {
                var result = rule.Test(span, this.LineNumber, this.ColumnNumber);
                if (result.Success)
                {
                    if (result.Type == TokenTyped.Comment || result.Type == TokenTyped.WhiteSpace)
                    {
                        this.readOffset += result.Length;
                        this.bufferLength -= result.Length;
                        this.LineNumber += result.LineCount;
                        this.ColumnNumber = result.ColumnNumber;
                        return this.ParseNext();
                    }
                    var token = this.CreateToken(result);
                    this.readOffset += result.Length;
                    this.bufferLength -= result.Length;
                    this.LineNumber += result.LineCount;
                    this.ColumnNumber = result.ColumnNumber;
                    return token;
                }
            }
            throw new LexerException(this.FileName, this.LineNumber, this.ColumnNumber, "Invalid keywords 。");
        }

        /// <summary>
        /// create token from rule result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="LexerException"></exception>
        private Token CreateToken(in RuleTestResult result)
        {
            Token token = null;
            var symbol = this.symbolProvider.FromString(result.Value);
            if (symbol != null)
            {
                if (symbol.Type == SymbolTypes.KeyWord) token = new KeywordToken();
                if (symbol.Type == SymbolTypes.Punctuator) token = new PunctuatorToken();
                if (symbol.Type == SymbolTypes.Operator) token = new OperatorToken();
                //if (symbol.Type == SymbolTypes.Typed) token = new TypedToken();
                if (symbol.Type == SymbolTypes.NullValue) token = new NullToken();
                if (symbol.Type == SymbolTypes.BooleanValue) token = new BooleanToken();
                if (symbol.Type == SymbolTypes.Identifier) token = new IdentifierToken();
                token.Symbol = symbol;
            }
            else
            {
                if (result.Type == TokenTyped.String) token = new StringToken();
                if (result.Type == TokenTyped.Number) token = new NumberToken();
                if (result.Type == TokenTyped.Identifier) token = new IdentifierToken();
            }
            if (token == null) throw new LexerException(this.FileName, this.LineNumber, this.ColumnNumber, $"Invalid Identifier {result.Value}");
            token.LineNumber = this.LineNumber + 1;
            token.ColumnNumber = this.ColumnNumber;
            token.Value = result.Value;
            return token;
        }
    }
}
