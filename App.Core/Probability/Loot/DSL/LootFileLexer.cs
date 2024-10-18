using Language.Parser;
using Language.Parser.Rules;
using System.Collections.Generic;
using System.Text;

namespace App.Core.Probability.Loot.DSL
{
    internal class LootFileLexer : AbstractLexer
    {
        public LootFileLexer(string file, Encoding encoding) : base(file, encoding)
        {

        }

        protected override void RegisterTokenRegexs(List<TokenRules> tokenRules)
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
