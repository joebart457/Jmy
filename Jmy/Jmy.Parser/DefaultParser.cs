using Jmy.Engine;
using Jmy.Parser.Builders;
using Jmy.Parser.Helpers;
using Jmy.Parser.Models;
using Jmy.Parser.Models.Constants;
using Jmy.Parser.Models.Expressions;
using Jmy.Parser.Models.Statements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser
{
    public class DefaultParser : ParsingHelper
    {

        private NumberFormatInfo DefaultNumberFormat = new NumberFormatInfo { NegativeSign = "-" };
        public Tokenizer? DefaultTokenizer { get; set; }
        private RuntimeContext? _context;
        public DefaultParser(RuntimeContext? context = null)
        {
            _context = context;
        }
        public IEnumerable<BaseStatement> Parse(string text)
        {
            var tokenizer = DefaultTokenizer ?? DefaultTokenizerBuilder.Build(BuildContextSpecificRules());

            var tokens = tokenizer.Tokenize(text);
            init(tokens);
            IList<BaseStatement> statements = new List<BaseStatement>();
            while (!atEnd() && !match(TokenTypes.EOF))
            {
                statements.Add(parseStatement());
            }
            return statements;
        }

        public IEnumerable<BaseStatement> Parse(IEnumerable<Token> tokens)
        {
            init(tokens);
            IList<BaseStatement> statements = new List<BaseStatement>();
            while (!atEnd() && !match(TokenTypes.EOF))
            {
                statements.Add(parseStatement());
            }
            return statements;
        }

        private List<TokenizerRule>? BuildContextSpecificRules()
        {
            return _context?.GetRegisteredMacros()
                .Select(val => new TokenizerRule(TokenTypes.CallableFunction, val)).ToList();
        }

        private BaseStatement parseStatement()
        {
            if (match(TokenTypes.Set))
            {
                return ParseSet();
            }
            if (match(TokenTypes.Do))
            {
                return ParseExpressionStatement();
            }
            if (match(TokenTypes.Load))
            {
                return ParseLoad();
            }

            throw new Exception($"only statements permitted. illegal token {current()}.");
        }
        private BaseStatement ParseSet()
        {
            var loc = previous().Loc;
            var target = ParseExpression();
            consume(TokenTypes.Equal, "expect = in set statement");
            var value = ParseExpression();
            return new StmtSet(target, value, loc);
        }


        private BaseStatement ParseLoad()
        {
            StmtLoad stmt = new StmtLoad(previous().Loc);
            stmt.Path = consume(TokenTypes.TTString, "expect filepath in 'load'").Lexeme;
            return stmt;
        }

        private BaseStatement ParseExpressionStatement()
        {
            var loc = current().Loc;
            StmtExpression stmtExpression = new StmtExpression(ParseExpression(), loc);
            return stmtExpression;
        }

        private BaseExpression ParseExpression()
        {
            return parseBinary();
        }

        private BaseExpression parseBinary()
        {
            var expr = ParseGet();
            while (match(TokenTypes.DoubleQuestionMark))
            {
                expr = new ExprNullableSwitch(previous().Loc, expr, parseBinary());
            }
            return expr;
        }

        private BaseExpression ParseGet()
        {
            
            var expr = ParseCall();
            while (match(TokenTypes.Dot))
            {
                Location loc = previous().Loc;
                expr = new ExprGet(expr, consume(TokenTypes.TTWord, "expect field name").Lexeme,loc);
                
            }
            return expr;
        }

        private BaseExpression ParseCall()
        {
            if (match(TokenTypes.LParen))
            {
                var identifier = consume(TokenTypes.CallableFunction, "expect function name");
                var args = new List<BaseExpression>();
                while (!match(TokenTypes.RParen))
                {
                    args.Add(ParseExpression());
                }
                return new ExprCall(identifier.Lexeme, args, identifier.Loc);
            }
            return ParsePrimary();
        }

        private BaseExpression ParsePrimary()
        {
            ExprLiteral exprLiteral = new ExprLiteral(current().Loc);
            if (match(TokenTypes.LiteralFalse))
            {
                exprLiteral.Value = false;
                return exprLiteral;
            }
            if (match(TokenTypes.LiteralTrue))
            {
                exprLiteral.Value = true;
                return exprLiteral;
            }
            if (match(TokenTypes.TTInteger))
            {
                exprLiteral.Value = int.Parse(previous().Lexeme, DefaultNumberFormat);
                return exprLiteral;
            }
            if (match(TokenTypes.TTUnsignedInteger))
            {
                exprLiteral.Value = uint.Parse(previous().Lexeme, DefaultNumberFormat);
                return exprLiteral;
            }
            if (match(TokenTypes.TTFloat))
            {
                exprLiteral.Value = float.Parse(previous().Lexeme, DefaultNumberFormat);
                return exprLiteral;
            }
            if (match(TokenTypes.TTDouble))
            {
                exprLiteral.Value = double.Parse(previous().Lexeme, DefaultNumberFormat);
                return exprLiteral;
            }
            if (match(TokenTypes.TTString))
            {
                exprLiteral.Value = previous().Lexeme;
                return exprLiteral;
            }
            if (match(TokenTypes.LiteralNull))
            {
                exprLiteral.Value = null;
                return exprLiteral;
            }
            if (match(current(), TokenTypes.Minus) &&
                (peekMatch(1, TokenTypes.TTUnsignedInteger)
                || peekMatch(1, TokenTypes.TTInteger)
                || peekMatch(1, TokenTypes.TTFloat)
                || peekMatch(1, TokenTypes.TTDouble)))
            {
                advance();
                if (match(TokenTypes.TTInteger))
                {
                    exprLiteral.Value = int.Parse("-" + previous().Lexeme, DefaultNumberFormat);
                    return exprLiteral;
                }
                if (match(TokenTypes.TTUnsignedInteger))
                {
                    exprLiteral.Value = uint.Parse("-" + previous().Lexeme, DefaultNumberFormat);
                    return exprLiteral;
                }
                if (match(TokenTypes.TTFloat))
                {
                    exprLiteral.Value = float.Parse("-" + previous().Lexeme, DefaultNumberFormat);
                    return exprLiteral;
                }
                if (match(TokenTypes.TTDouble))
                {
                    exprLiteral.Value = double.Parse("-" + previous().Lexeme, DefaultNumberFormat);
                    return exprLiteral;
                }
                throw new Exception($"unexpected token while parsing negative {current()}");
            }
            if (match(TokenTypes.TTWord))
            {
                ExprIdentifier exprIdentifier = new ExprIdentifier(previous().Loc);
                exprIdentifier.Symbol = previous().Lexeme;
                return exprIdentifier;
            }  

            throw new Exception($"unexpected token in primary {current()}");
        }
    }
}
