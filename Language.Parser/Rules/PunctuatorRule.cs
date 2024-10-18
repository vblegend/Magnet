using System;


namespace Language.Parser.Rules
{
    internal class PunctuatorRule : TokenRules
    {
        public override RuleTestResult Test(in ReadOnlySpan<Char> codeSpan, in Int32 LineNumber, in Int32 ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            if (codeSpan.Length >= 3)
            {
                if (codeSpan[0] == '.' && codeSpan[1] == '.' && codeSpan[2] == '.')
                {
                    result.ColumnNumber += 3;
                    result.Length = 3;
                    result.Value = codeSpan.Slice(0, 3).ToString();
                    result.Success = true;
                    return result;
                }
            }
            if (codeSpan.Length >= 2)
            {
                if ((codeSpan[0] == '+' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '-' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '*' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '/' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '=' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '!' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '>' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '<' && codeSpan[1] == '=') ||
                   (codeSpan[0] == '+' && codeSpan[1] == '+') ||
                   (codeSpan[0] == '-' && codeSpan[1] == '-') ||
                   (codeSpan[0] == '|' && codeSpan[1] == '|') ||
                   (codeSpan[0] == '&' && codeSpan[1] == '&') ||
                   (codeSpan[0] == '>' && codeSpan[1] == '>') ||
                   (codeSpan[0] == '<' && codeSpan[1] == '<') ||
                   (codeSpan[0] == '=' && codeSpan[1] == '>'))

                {
                    result.ColumnNumber += 2;
                    result.Length = 2;
                    result.Value = codeSpan.Slice(0, 2).ToString();
                    result.Success = true;
                }
                else if (
                    codeSpan[0] == '+' || codeSpan[0] == '-' || codeSpan[0] == '*' || codeSpan[0] == '/' ||
                    codeSpan[0] == '=' || codeSpan[0] == '%' || codeSpan[0] == '<' || codeSpan[0] == '>' ||
                    codeSpan[0] == '.' || codeSpan[0] == ',' || codeSpan[0] == ';' || codeSpan[0] == ':' ||
                    codeSpan[0] == '?' || codeSpan[0] == '!' || codeSpan[0] == '^' || codeSpan[0] == '{' ||
                    codeSpan[0] == '}' || codeSpan[0] == '[' || codeSpan[0] == ']' || codeSpan[0] == '(' ||
                    codeSpan[0] == ')' || codeSpan[0] == '|' || codeSpan[0] == '~' || codeSpan[0] == '&'
                    )
                {
                    result.ColumnNumber += 1;
                    result.Length = 1;
                    result.Value = codeSpan[0].ToString();
                    result.Success = true;
                }
            }
            else if (
                    codeSpan[0] == '+' || codeSpan[0] == '-' || codeSpan[0] == '*' || codeSpan[0] == '/' ||
                    codeSpan[0] == '=' || codeSpan[0] == '%' || codeSpan[0] == '<' || codeSpan[0] == '>' ||
                    codeSpan[0] == '.' || codeSpan[0] == ',' || codeSpan[0] == ';' || codeSpan[0] == ':' ||
                    codeSpan[0] == '?' || codeSpan[0] == '!' || codeSpan[0] == '^' || codeSpan[0] == '{' ||
                    codeSpan[0] == '}' || codeSpan[0] == '[' || codeSpan[0] == ']' || codeSpan[0] == '(' ||
                    codeSpan[0] == ')' || codeSpan[0] == '|' || codeSpan[0] == '~' || codeSpan[0] == '&'
                    )
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
}
