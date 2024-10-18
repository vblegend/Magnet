using System;


namespace Language.Parser.Rules
{
    internal class IdentifierRule : TokenRules
    {
        public override unsafe RuleTestResult Test(in ReadOnlySpan<Char> codeSpan, in Int32 LineNumber, in Int32 ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            if ((codeSpan[0] >= 'a' && codeSpan[0] <= 'z') ||
                (codeSpan[0] >= 'A' && codeSpan[0] <= 'Z') ||
                (codeSpan[0] >= 0x4e00 && codeSpan[0] <= 0x9fbb) ||
                  codeSpan[0] == '_' ||
                  codeSpan[0] == '$')
            {
                for (int i = 1; i < codeSpan.Length; i++)
                {
                    if ((codeSpan[i] >= 'a' && codeSpan[i] <= 'z') ||
                    (codeSpan[i] >= 'A' && codeSpan[i] <= 'Z') ||
                    (codeSpan[i] >= 0x4e00 && codeSpan[i] <= 0x9fbb) ||
                    (codeSpan[i] >= '0' && codeSpan[i] <= '9') ||
                     codeSpan[i] == '_')
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
}
