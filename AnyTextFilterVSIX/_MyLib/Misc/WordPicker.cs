using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    public static class WordPicker
    {
        public static string GetWord(string text, int offset, int keywordLength = 0)
        {
            var buf = new StringBuilder(text.Substring(offset, keywordLength));
            for (int iChar = offset - 1; iChar >= 0; --iChar)
            {
                if (!char.IsLetterOrDigit(text[iChar])) break;
                buf.Insert(0, text[iChar]);
            }
            for (int iChar = offset + keywordLength; iChar < text.Length; ++iChar)
            {
                if (!char.IsLetterOrDigit(text[iChar])) break;
                buf.Append(text[iChar]);
            }
            return buf.ToString();
        }
    }
}
