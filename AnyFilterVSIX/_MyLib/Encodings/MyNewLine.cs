using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyFilterVSIX
{
    public enum MyNewLineKind
    {
        CRLF, LF
    }

    public static class MyNewLineConverter
    {
        public static string ToNewLineString(this MyNewLineKind kind)
        {
            switch (kind)
            {
                default:
                case MyNewLineKind.CRLF: return "\r\n";
                case MyNewLineKind.LF: return "\n";
            }
        }
    }
}
