using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Parser.Models.Constants
{
    public static class TokenTypes
    {
        // Tokenizer required types
        public const string StringEnclosing = "StringEnclosing";
        public const string StringCatalyst = "StringCatalyst";
        public const string EOLComment = "EOLComment";
        public const string MLCommentStart = "MLCommentStart";
        public const string MLCommentEnd = "MLCommentEnd";
        public const string EOF = "EOF";
        public const string TTString = "TTString";
        public const string TTWord = "TTWord";
        public const string TTInteger = "TTInteger";
        public const string TTUnsignedInteger = "TTUnsignedInteger";
        public const string TTFloat = "TTFloat";
        public const string TTDouble = "TTDouble";
        public const string WhiteSpaceSpace = "WhiteSpaceSpace";
        public const string WhiteSpaceTab = "WhiteSpaceTab";
        public const string WhiteSpaceCR = "WhiteSpaceCR";
        public const string WhiteSpaceLF = "WhiteSpaceLF";

        // Custom Types

        public const string LiteralFalse = "LiteralFalse";
        public const string LiteralTrue = "LiteralTrue";
        public const string LiteralNull = "LiteralNull";
        public const string Minus = "Minus";
        public const string Dot = "Dot";
        public const string DoubleQuestionMark = "DoubleQuestionMark";
        public const string DoubleDot = "DoubleDot";
        public const string LParen = "LParen";
        public const string RParen = "RParen";
        public const string Equal = "Equal";
        public const string Do = "Do";
        public const string Set = "Set";
        public const string Load = "Load";
        public const string CallableFunction = "";
    }
}
