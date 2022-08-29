using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Expressions
{
    public class ExprGet : BaseExpression
    {
        public BaseExpression Lhs { get; set; }
        public string Name { get; set; } = "";
        public ExprGet(BaseExpression lhs, string name, Location loc)
            : base("ExprGet", loc)
        {
            Lhs = lhs;
            Name = name;
        }

        public override object? Visit(IInterpreter interpreter)
        {
            return interpreter.Accept(this);
        }
    }
}
