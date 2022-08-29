using Jmy.Engine;
using Jmy.Parser.Interfaces;
using Jmy.Parser.Models.Expressions;
using Jmy.Parser.Models.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Interpreter
{
    public class DefaultInterpreter: IInterpreter
    {
        private RuntimeContext _context = new RuntimeContext();

        public DefaultInterpreter(RuntimeContext context)
        {
            _context = context;
        }

        public object? Accept(BaseExpression expression)
        {
            return expression.Visit(this);
        }
        public object? Accept(ExprNullableSwitch nullableSwitch)
        {
            var lhsVal = nullableSwitch.Lhs.Visit(this);
            if (lhsVal != null) return lhsVal;
            return nullableSwitch.Rhs.Visit(this);
        }
        public object? Accept(ExprCall call)
        {
            var args = new List<object?>();
            foreach(var arg in call.Arguments)
            {
                args.Add(arg.Visit(this));    
            }
            return _context.InvokeMacro(call.Symbol, args.ToArray());
        }
        public object? Accept(ExprGet get)
        {
            var lhs = get.Lhs.Visit(this);
            if (lhs == null) throw new Exception($"unable to access field {get.Name} on null value object");
            var klass = _context.GetClassDefinition(lhs.GetType());
            return klass.GetField(get.Name, lhs);
        }

        public object? Accept(ExprIdentifier identifier)
        {
            return _context.GetStoredValue(identifier.Symbol);
        }
        public object? Accept(ExprLiteral literal)
        {
            return literal.Value;
        }
        public void Accept(BaseStatement statement)
        {
            statement.Visit(this);
        }
        public void Accept(StmtLoad load)
        {
            _context.RegisterAssembly(load.Path);
        }
        public void Accept(StmtSet set)
        {
            var val = set.Value.Visit(this);
            if (set.Target is ExprGet get)
            {
                var lhs = get.Lhs.Visit(this);
                if (lhs == null) throw new Exception($"unable to assign value to field {get.Name} of null value");
                var klass = _context.GetClassDefinition(lhs.GetType());
                klass.SetField(get.Name, lhs, val);
            }
            else if (set.Target is ExprIdentifier identifier)
            {
                _context.StoreValue(identifier.Symbol, val);
            } else
            {
                throw new Exception($"invalid assignment target {set.Target}");
            }
        }
        public void Accept(StmtExpression expression)
        {
            expression.Expression.Visit(this);
        }
    }
}
