using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyFilterVSIX
{
    class MyTextEdit
    {
        #region Constructor

        Func<string> getText;
        Action<string> setText;
        Func<int> getCaretIndex;
        Action<int> setCaretIndex;

        public MyTextEdit(Func<string> getText, Action<string> setText, Func<int> getCaretIndex, Action<int> setCaretIndex)
        {
            this.getText = getText;
            this.setText = setText;
            this.getCaretIndex = getCaretIndex;
            this.setCaretIndex = setCaretIndex;
        }

        #endregion

        #region Properties

        protected string Text
        {
            get { return getText(); }
            set { setText(value); }
        }

        protected int CaretIndex
        {
            get { return getCaretIndex(); }
            set { setCaretIndex(value); }
        }

        protected static readonly string NL = Environment.NewLine;
        protected static readonly int NLLength = Environment.NewLine.Length;

        #endregion

        #region Actions

        public void ForwardChar()
        {
            if (CaretIndex < Text.Length)
            {
                CaretIndex = CaretIndex + 1;
            }
        }

        public void BackwardChar()
        {
            if (CaretIndex > 0)
            {
                CaretIndex = CaretIndex - 1;
            }
        }

        public void MoveBiginningOfLine()
        {
            int iPrevNL = Text.Substring(0, CaretIndex).LastIndexOf(NL);
            int iCurrLineStart = iPrevNL != -1 ? iPrevNL + NLLength : 0;
            if (CaretIndex > iCurrLineStart)
            {
                CaretIndex = iCurrLineStart;
            }
        }

        public void MoveEndOfLine()
        {
            int iNextNL = Text.IndexOf(NL, CaretIndex);
            int iCurrLineEnd = iNextNL != -1 ? iNextNL : Text.Length;
            if (CaretIndex < iCurrLineEnd)
            {
                CaretIndex = iCurrLineEnd;
            }
        }

        public void NextLine()
        {
            int iNextNL = Text.IndexOf(NL, CaretIndex);
            if (iNextNL != -1)
            {
                int iPrevNL = Text.Substring(0, CaretIndex).LastIndexOf(NL);
                int iCaretFromCurrLineStart = CaretIndex - (iPrevNL != -1 ? iPrevNL + NLLength : 0);
                int iNextLineStart = iNextNL + NLLength;
                int iNextNextNL = Text.IndexOf(NL, iNextLineStart);
                int nextLineLength = ((iNextNextNL != -1) ? iNextNextNL : Text.Length) - iNextLineStart;
                CaretIndex = iNextLineStart + Math.Min(iCaretFromCurrLineStart, nextLineLength);
            }
        }

        public void PreviousLine()
        {
            int iPrevNL = Text.Substring(0, CaretIndex).LastIndexOf(NL);
            if (iPrevNL != -1)
            {
                int iNextNL = Text.IndexOf(NL, CaretIndex);
                int iCaretFromCurrLineStart = CaretIndex - (iPrevNL + NLLength);
                int iPrevPrevNL = Text.Substring(0, iPrevNL).LastIndexOf(NL);
                int iPrevLineStart = iPrevPrevNL != -1 ? iPrevPrevNL + NLLength : 0;
                int prevLineLength = iPrevNL - iPrevLineStart;
                CaretIndex = iPrevLineStart + Math.Min(iCaretFromCurrLineStart, prevLineLength);
            }
        }

        public void DeleteChar()
        {
            if (CaretIndex < Text.Length)
            {
                int backupCaretIndex = CaretIndex;
                int count = (CaretIndex < Text.Length - 1 && Text.Substring(CaretIndex, NLLength) == NL) ? NLLength : 1;
                Text = Text.Remove(CaretIndex, count);
                CaretIndex = backupCaretIndex;
            }
        }

        public void DeleteBackwardChar()
        {
            if (CaretIndex > 0)
            {
                int backupCaretIndex = CaretIndex;
                int count = (CaretIndex > 1 && Text.Substring(CaretIndex - NLLength, NLLength) == NL) ? NLLength : 1;
                Text = Text.Remove(CaretIndex - count, count);
                CaretIndex = backupCaretIndex - count;
            }
        }

        public void KillLine()
        {
            if (CaretIndex < Text.Length)
            {
                int backupCaretIndex = CaretIndex;
                Text = Text.Substring(0, CaretIndex);
                CaretIndex = backupCaretIndex;
            }
        }

        #endregion
    }
}
