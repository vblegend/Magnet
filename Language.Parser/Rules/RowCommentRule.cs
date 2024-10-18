using System;


namespace Language.Parser.Rules
{
    public class RowCommentRule : TokenRules
    {
        public override RuleTestResult Test(in ReadOnlySpan<Char> codeSpan, in Int32 LineNumber, in Int32 ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            if (codeSpan.Length >= 2 && codeSpan[0] == '/' && codeSpan[1] == '/')
            {
                for (int i = 0; i < codeSpan.Length; i++)
                {
                    if (codeSpan[i] == '\n')
                    {
                        result.ColumnNumber = 0;
                        result.LineCount = 1;
                        result.Length = i + 1;
                        result.Value = codeSpan.Slice(0, i + 1).ToString();
                        result.Type = TokenTyped.Comment;
                        result.Success = true;
                        break;
                    }
                }
                if (!result.Success)
                {
                    result.ColumnNumber = 0;
                    result.LineCount = 1;
                    result.Length = codeSpan.Length;
                    result.Value = codeSpan.Slice(0, codeSpan.Length).ToString();
                    result.Type = TokenTyped.Comment;
                    result.Success = true;
                }
            }
            return result;
        }
    }
}
