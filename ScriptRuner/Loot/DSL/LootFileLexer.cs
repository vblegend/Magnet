using Language.Parser;
using Language.Parser.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptRuner.Loot.DSL
{

    internal class PunctuatorRule : ILexicalRules
    {
        public RuleTestResult Test(in ReadOnlySpan<char> codeSpan, in int LineNumber, in int ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            if (codeSpan[0] == '#' || codeSpan[0] == '{' || codeSpan[0] == '}' || codeSpan[0] == '/' || codeSpan[0] == '!')
            {
                result.ColumnNumber += 1;
                result.Length = 1;
                result.Value = codeSpan[0].ToString();
                result.Type = TokenTyped.Punctuator;
                result.Success = true;
            }
            return result;
        }
    }


    internal class IdentifierRule : ILexicalRules
    {
        public unsafe RuleTestResult Test(in ReadOnlySpan<char> codeSpan, in int LineNumber, in int ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            if (codeSpan[0] >= 'a' && codeSpan[0] <= 'z' ||
                codeSpan[0] >= 'A' && codeSpan[0] <= 'Z' ||
                codeSpan[0] >= 0x4e00 && codeSpan[0] <= 0x9fbb)
            {
                for (int i = 1; i < codeSpan.Length; i++)
                {
                    if (codeSpan[i] >= 'a' && codeSpan[i] <= 'z' ||
                    codeSpan[i] >= 'A' && codeSpan[i] <= 'Z' ||
                    codeSpan[i] >= 0x4e00 && codeSpan[i] <= 0x9fbb ||
                    codeSpan[i] >= '0' && codeSpan[i] <= '9' ||
                     codeSpan[i] == '_' || codeSpan[i] == '-' || codeSpan[i] == '.')
                    {
                    }
                    else
                    {
                        result.ColumnNumber += i;
                        result.Length = i;
                        result.Value = codeSpan.Slice(0, i).ToString();
                        result.Success = true;
                        result.Type = TokenTyped.Identifier;
                        break;
                    }
                }
            }
            return result;
        }
    }




    internal class Symbols
    {
        public static readonly Symbol LEFTBRACE = new Symbol("{", SymbolTypes.Punctuator);
        public static readonly Symbol RIGHTBRACE = new Symbol("}", SymbolTypes.Punctuator);
        public static readonly Symbol SHARP = new Symbol("#", SymbolTypes.Operator);
        public static readonly Symbol DIVIDE = new Symbol("/", SymbolTypes.Operator);
        public static readonly Symbol NEWLINE = new Symbol("\n", SymbolTypes.Operator);
        public static readonly Symbol INCLUDE = new Symbol("include", SymbolTypes.Identifier);
        public static readonly Symbol FORCE = new Symbol("!", SymbolTypes.Identifier);
    }

    internal class LootFileLexer : AbstractLexer
    {
        public LootFileLexer(string file, Encoding encoding) : base(file, encoding)
        {
        }

        protected override void RegisterSymbols(SymbolProvider symbolProvider)
        {
            symbolProvider.Register(Symbols.LEFTBRACE);
            symbolProvider.Register(Symbols.RIGHTBRACE);
            symbolProvider.Register(Symbols.SHARP);
            symbolProvider.Register(Symbols.DIVIDE);
            symbolProvider.Register(Symbols.NEWLINE);
            symbolProvider.Register(Symbols.INCLUDE);
            symbolProvider.Register(Symbols.FORCE);
        }

        protected override void RegisterTokenRegexs(List<ILexicalRules> tokenRules)
        {
            tokenRules.Add(new RowCommentRule());
            tokenRules.Add(new BlockCommentRule());
            tokenRules.Add(new NewLineRule());
            tokenRules.Add(new NumberRule());
            tokenRules.Add(new IdentifierRule());
            tokenRules.Add(new PunctuatorRule());
            tokenRules.Add(new WhiteSpaceRule());
        }
    }
}
