using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Expressions
{
    public class ExprCall : BaseExpression
    {
        public string Symbol { get; set; } = "";
        public IList<BaseExpression> Arguments { get; set; } = new List<BaseExpression>();
        public ExprCall(string symbol, IList<BaseExpression> arguments, Location loc)
            : base("ExprCall", loc)
        {
            Symbol = symbol;
            Arguments = arguments;
        }

        public override object? Visit(IInterpreter interpreter)
        {
            return interpreter.Accept(this);
        }
    }
}
