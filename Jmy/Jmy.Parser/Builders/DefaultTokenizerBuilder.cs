using Jmy.Parser.Models;
using Jmy.Parser.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Builders
{
    public static class DefaultTokenizerBuilder
    {
        public static Tokenizer Build(List<TokenizerRule>? additionalRules = null)
        {

            TokenizerSettings settings = new TokenizerSettings
            {
                StringCatalystExcluded = ") ;",
                StringCatalystEscapable = ") \\;",
                WordIncluded = "-",
                CatchAllType = "",
                SkipWhiteSpace = true,
                IgnoreCase = true
            };

            List<TokenizerRule> rules = new List<TokenizerRule>();
            rules.Add(new TokenizerRule(TokenTypes.StringEnclosing, "\""));
            rules.Add(new TokenizerRule(TokenTypes.TTString, "'", null, true, "'"));
            rules.Add(new TokenizerRule(TokenTypes.StringCatalyst, "$"));
            rules.Add(new TokenizerRule(TokenTypes.EOLComment, "---"));
            rules.Add(new TokenizerRule(TokenTypes.EOLComment, "//"));
            rules.Add(new TokenizerRule(TokenTypes.MLCommentStart, "/*"));
            rules.Add(new TokenizerRule(TokenTypes.MLCommentEnd, "*/"));
            rules.Add(new TokenizerRule(TokenTypes.EOF, "EOF"));

            rules.Add(new TokenizerRule(TokenTypes.Set, "Set"));

            rules.Add(new TokenizerRule(TokenTypes.Load, "load"));


            rules.Add(new TokenizerRule(TokenTypes.LiteralTrue, "true"));
            rules.Add(new TokenizerRule(TokenTypes.LiteralFalse, "false"));

            rules.Add(new TokenizerRule(TokenTypes.Dot, "."));

            rules.Add(new TokenizerRule(TokenTypes.Equal, "="));
            rules.Add(new TokenizerRule(TokenTypes.LParen, "("));
            rules.Add(new TokenizerRule(TokenTypes.RParen, ")"));

            rules.Add(new TokenizerRule(TokenTypes.DoubleQuestionMark, "??"));

            rules.Add(new TokenizerRule(TokenTypes.DoubleDot, ".."));
            rules.Add(new TokenizerRule(TokenTypes.Do, "Do"));

            rules.AddRange(additionalRules ?? Enumerable.Empty<TokenizerRule>());

            return new Tokenizer(rules, settings);
        }
    }
}
