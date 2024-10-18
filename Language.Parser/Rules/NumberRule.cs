using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language.Parser.Rules
{
    public class NumberRule : TokenRules
    {
        public override RuleTestResult Test(in ReadOnlySpan<Char> codeSpan, in Int32 LineNumber, in Int32 ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            Int32 dot = -1;
            Char? lastChar = null;
            if ((codeSpan[0] >= '0' && codeSpan[0] <= '9') || codeSpan[0] == '-')
            {
                lastChar = codeSpan[0];
                for (int i = 1; i < codeSpan.Length; i++)
                {
                    if (codeSpan[i] >= '0' && codeSpan[i] <= '9')
                    {
                    }
                    else if (codeSpan[i] == '_')
                    {
                        if (lastChar == '-' || lastChar == '.' || lastChar == '_') return result;
                    }
                    else if (codeSpan[i] == '.')
                    {
                        if (lastChar == '-' || lastChar == '.' || lastChar == '_') return result;
                        if (lastChar == '-') return result;
                        if (dot > -1) return result;
                        dot = i;
                    }
                    else if (codeSpan[0] == '-' && i == 1)
                    {
                        return result;
                    }
                    else
                    {
                        result.ColumnNumber += i;
                        result.Length = i;
                        result.Value = codeSpan.Slice(0, i).ToString();
                        result.Success = true;
                        result.Type = TokenTyped.Number;
                        break;
                    }
                    lastChar = codeSpan[i];
                }
            }
            return result;
        }
    }
}
