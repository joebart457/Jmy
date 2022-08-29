using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Expressions
{
    public class ExprLiteral : BaseExpression
    {
        public object? Value { get; set; }
        public ExprLiteral(Location loc)
            : base("ExprLiteral", loc)
        {

        }

        public override object? Visit(IInterpreter interpreter)
        {
            return interpreter.Accept(this);
        }
    }
}
