using System;
using Game.Toolkit.Analysis;

namespace Game.Toolkit.Analysis.Rules
{
    public class BlockCommentRule : ILexicalRules
    {
        public RuleTestResult Test(in ReadOnlySpan<Char> codeSpan, in Int32 LineNumber, in Int32 ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            if (codeSpan.Length >= 2 && codeSpan[0] == '/' && codeSpan[1] == '*')
            {
                for (int i = 0; i < codeSpan.Length - 1; i++)
                {
                    if (codeSpan[i] == '\n')
                    {
                        result.ColumnNumber = 0;
                        result.LineCount += 1;
                    }
                    else
                    {
                        result.ColumnNumber++;
                        if (codeSpan[i] == '*' && codeSpan[i + 1] == '/')
                        {
                            result.ColumnNumber++;
                            result.Length = i + 2;
                            result.Value = codeSpan.Slice(0, i + 2).ToString();
                            result.Type = TokenTyped.Comment;
                            result.Success = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
