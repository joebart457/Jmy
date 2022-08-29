using Jmy.Parser.Interfaces;
using Jmy.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Statements
{
    public class StmtExpression : BaseStatement
    {
        public BaseExpression Expression { get; set; }
        public StmtExpression(BaseExpression expression, Location loc)
            : base("StmtExpression", loc)
        {
            Expression = expression;
        }

        public override void Visit(IInterpreter interpreter)
        {
            interpreter.Accept(this);
        }
    }
}
