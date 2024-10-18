
using Language.Parser;
using System;


namespace App.Core.Probability.Loot.DSL
{
    public class PunctuatorRule : TokenRules
    {
        public override RuleTestResult Test(in ReadOnlySpan<char> codeSpan, in int LineNumber, in int ColumnNumber)
        {
            var result = new RuleTestResult();
            result.ColumnNumber = ColumnNumber;
            if (codeSpan[0] == '#' || codeSpan[0] == '{' || codeSpan[0] == '}' || codeSpan[0] == '/')
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
