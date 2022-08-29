using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models
{
    public class Token
    {
        public Location Loc { get; set; }
        public string Lexeme { get; set; }
        public string Type { get; set; }
        public string? EnclosingFront { get; set; }
        public string? EnclosingBack { get; set; }
        public Token(TokenizerRule rule, uint nRow, uint nColumn)
        {
            Lexeme = rule.Value;
            Type = rule.Type;
            Loc = new Location(nColumn, nRow);
        }

        public Token(string type, string lexeme, uint nRow, uint nColumn, string enclosingFront = null, string enclosingBack = null)
        {
            Lexeme = lexeme;
            Type = type;
            Loc = new Location(nColumn, nRow);
            EnclosingFront = enclosingFront;
            EnclosingBack = enclosingBack;
        }

        public override string ToString()
        {
            return $"Token({Type}|{Lexeme}) at {Loc.ToString()}";
        }
    }
}
