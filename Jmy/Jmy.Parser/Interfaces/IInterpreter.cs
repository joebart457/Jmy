using Jmy.Parser.Models.Expressions;
using Jmy.Parser.Models.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Interfaces
{
    public interface IInterpreter
    {
        object? Accept(BaseExpression expression);

        object? Accept(ExprNullableSwitch nullableSwitch);
        object? Accept(ExprCall call);
        object? Accept(ExprGet get);
        object? Accept(ExprIdentifier identifier);
        object? Accept(ExprLiteral literal);

        void Accept(BaseStatement statement);
        void Accept(StmtLoad load);
        void Accept(StmtSet set);
        void Accept(StmtExpression expression);
    }
}
