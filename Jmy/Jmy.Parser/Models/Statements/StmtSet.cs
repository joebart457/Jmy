using Jmy.Parser.Interfaces;
using Jmy.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Statements
{
    public class StmtSet : BaseStatement
    {
        public BaseExpression Target { get; set; }
        public BaseExpression Value { get; set; }
        public StmtSet(BaseExpression target, BaseExpression value, Location loc)
            : base("StmtSet", loc)
        {
            Target = target;
            Value = value;
        }

        public override void Visit(IInterpreter interpreter)
        {
            interpreter.Accept(this);
        }
    }
}
