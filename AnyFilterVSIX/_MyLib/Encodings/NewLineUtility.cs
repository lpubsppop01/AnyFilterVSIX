using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyFilterVSIX
{
    public enum NewLineKind
    {
        CRLF, LF
    }

    public static class NewLineUtility
    {
        public static string ToNewLineString(this NewLineKind kind)
        {
            switch (kind)
            {
                default:
                case NewLineKind.CRLF: return "\r\n";
                case NewLineKind.LF: return "\n";
            }
        }

        public static NewLineKind ToNewLineKind(this string newLineStr)
        {
            if (newLineStr == "\r\n")
            {
                return NewLineKind.CRLF;
            }
            else if (newLineStr == "\n")
            {
                return NewLineKind.LF;
            }
            throw new ArgumentException("The passed newLineStr value is not new line characters.");
        }

        public static NewLineKind? DetectNewLineKind(this string str)
        {
            int iLF = str.IndexOf('\n');
            if (iLF == -1) return NewLineKind.CRLF;
            if (iLF == 0) return NewLineKind.LF;
            return (str[iLF - 1] == '\r') ? NewLineKind.CRLF : NewLineKind.LF;
        }

        public static string ConvertNewLine(this string srcStr, NewLineKind destKind)
        {
            var srcKind = srcStr.DetectNewLineKind();
            if (srcKind == null) return srcStr;
            return srcStr.ConvertNewLine(srcKind.Value, destKind);
        }

        public static string ConvertNewLine(this string srcStr, NewLineKind srcKind, NewLineKind destKind)
        {
            if (srcKind == destKind) return srcStr;
            return srcStr.Replace(srcKind.ToNewLineString(), destKind.ToNewLineString());
        }

        public static string ConvertNewLineToEnvironment(this string srcStr, NewLineKind srcKind)
        {
            if (srcKind.ToNewLineString() != Environment.NewLine)
            {
                return srcStr.Replace(srcKind.ToNewLineString(), Environment.NewLine);
            }
            return srcStr;
        }

        public static string ConvertNewLineFromEnvironment(this string srcStr, NewLineKind destKind)
        {
            if (destKind.ToNewLineString() != Environment.NewLine)
            {
                return srcStr.Replace(Environment.NewLine, destKind.ToNewLineString());
            }
            return srcStr;
        }
    }
}
