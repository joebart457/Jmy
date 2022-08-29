using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Statements
{
    public abstract class BaseStatement
    {
        public string Label { get; set; }
        public Location Loc { get; set; }

        public BaseStatement(string label, Location loc)
        {
            Loc = loc;
            Label = label;
        }

        public abstract void Visit(IInterpreter interpreter);

        public override string ToString()
        {
            return $"{Label}({Loc})";
        }
    }
}
