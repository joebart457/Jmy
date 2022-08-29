using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Expressions
{
    public class ExprNullableSwitch : BaseExpression
    {
        public BaseExpression Lhs { get; set; }
        public BaseExpression Rhs { get; set; }

        public ExprNullableSwitch(Location loc, BaseExpression lhs, BaseExpression rhs)
            : base("ExprNullableSwitch", loc)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override object? Visit(IInterpreter interpreter)
        {
            return interpreter.Accept(this);
        }
    }
}
