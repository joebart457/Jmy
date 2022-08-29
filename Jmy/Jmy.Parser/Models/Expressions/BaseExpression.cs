using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Expressions
{
    public abstract class BaseExpression
    {
        public string Label { get; set; }
        public Location Loc { get; set; }

        public BaseExpression(string label, Location loc)
        {
            Loc = loc;
            Label = label;
        }

        public abstract object? Visit(IInterpreter interpreter);
    }
}
