using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Expressions
{
    public class ExprIdentifier : BaseExpression
    {
        public string Symbol { get; set; } = "";
        public ExprIdentifier(Location loc)
            : base("ExprIdentifier", loc)
        {

        }

        public override object? Visit(IInterpreter interpreter)
        {
            return interpreter.Accept(this);
        }
    }
}
