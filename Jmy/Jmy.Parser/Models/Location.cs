using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models
{
    public class Location
    {
        public uint X { get; set; }
        public uint Y { get; set; }

        public Location(uint x, uint y)
        {
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return $"Line {Y}:{X}";
        }
    }
}
