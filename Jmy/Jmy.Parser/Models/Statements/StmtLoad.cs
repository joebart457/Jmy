using Jmy.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Statements
{
    public class StmtLoad : BaseStatement
    {
        public string Path { get; set; } = "";
        public StmtLoad(Location loc)
            : base("StmtLoad", loc)
        {

        }

        public override void Visit(IInterpreter interpreter)
        {
            interpreter.Accept(this);
        }
    }
}
