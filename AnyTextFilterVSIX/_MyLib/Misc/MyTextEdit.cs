using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace lpubsppop01.AnyTextFilterVSIX
{
    public class MyTextEdit
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
            GetTextFromClipboard = Clipboard.GetText;
            SetTextFromClipboard = (text) => Clipboard.SetText(text);
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

        public Func<string> GetTextFromClipboard;
        public Action<string> SetTextFromClipboard;

        protected static readonly string NL = Environment.NewLine;
        protected static readonly int NLLength = Environment.NewLine.Length;

        protected int ColumnIndex { get; set; }

        #endregion

        #region Actions

        int GetOffsetToNextChar()
        {
            if (CaretIndex >= Text.Length) return 0;
            if (CaretIndex <= Text.Length - NLLength && Text.Substring(CaretIndex, NLLength) == NL) return NLLength;
            return 1;
        }

        int GetOffsetToPreviousChar()
        {
            if (CaretIndex <= 0) return 0;
            if (CaretIndex >= NLLength && Text.Substring(CaretIndex - NLLength, NLLength) == NL) return NLLength;
            return 1;
        }

        int GetCurrentLineStartIndex()
        {
            int iPrevNL = Text.LastIndexOf(NL, CaretIndex);
            int iCurrLineStart = iPrevNL != -1 ? iPrevNL + NLLength : 0;
            return iCurrLineStart;
        }

        int GetCurrentLineEndIndex()
        {
            int iNextNL = Text.IndexOf(NL, CaretIndex);
            int iCurrLineEnd = iNextNL != -1 ? iNextNL : Text.Length;
            return iCurrLineEnd;
        }

        void UpdateColumnIndex()
        {
            ColumnIndex = Math.Max(CaretIndex - GetCurrentLineStartIndex(), 0);
        }

        string killedTextBuf;

        void SetKilledTextToClipboard(int iKillStart, int iKillEnd, bool combinesWithBuf)
        {
            if (SetTextFromClipboard != null)
            {
                string killedText = Text.Substring(iKillStart, iKillEnd - iKillStart);
                if (combinesWithBuf && !string.IsNullOrEmpty(killedTextBuf))
                {
                    killedText = killedTextBuf + killedText;
                }
                SetTextFromClipboard(killedText);
                killedTextBuf = killedText;
            }
        }

        void ClearKilledTextBuffer()
        {
            killedTextBuf = null;
        }

        public void ForwardChar()
        {
            ClearKilledTextBuffer();
            int offset = GetOffsetToNextChar();
            if (offset == 0) return;
            CaretIndex += offset;
            UpdateColumnIndex();
        }

        public void BackwardChar()
        {
            ClearKilledTextBuffer();
            int offset = GetOffsetToPreviousChar();
            if (offset == 0) return;
            CaretIndex -= offset;
            UpdateColumnIndex();
        }

        public void MoveBeginningOfLine()
        {
            ClearKilledTextBuffer();
            int iCurrLineStart = GetCurrentLineStartIndex();
            if (CaretIndex < iCurrLineStart) return; // pass equal to set ColumnIndex
            CaretIndex = iCurrLineStart;
            UpdateColumnIndex();
        }

        public void MoveEndOfLine()
        {
            ClearKilledTextBuffer();
            int iCurrLineEnd = GetCurrentLineEndIndex();
            if (CaretIndex > iCurrLineEnd) return; // pass equal to set ColumnIndex
            CaretIndex = iCurrLineEnd;
            UpdateColumnIndex();
        }

        public void NextLine()
        {
            ClearKilledTextBuffer();
            int iNextNL = Text.IndexOf(NL, CaretIndex);
            if (iNextNL == -1) return;
            int iNextLineStart = iNextNL + NLLength;
            int iNextNextNL = Text.IndexOf(NL, iNextLineStart);
            int nextLineLength = ((iNextNextNL != -1) ? iNextNextNL : Text.Length) - iNextLineStart;
            CaretIndex = iNextLineStart + Math.Min(ColumnIndex, nextLineLength);
        }

        public void PreviousLine()
        {
            ClearKilledTextBuffer();
            int iPrevNL = Text.Substring(0, CaretIndex).LastIndexOf(NL);
            if (iPrevNL == -1) return;
            int iPrevPrevNL = Text.Substring(0, iPrevNL).LastIndexOf(NL);
            int iPrevLineStart = iPrevPrevNL != -1 ? iPrevPrevNL + NLLength : 0;
            int prevLineLength = iPrevNL - iPrevLineStart;
            CaretIndex = iPrevLineStart + Math.Min(ColumnIndex, prevLineLength);
        }

        public void DeleteChar()
        {
            ClearKilledTextBuffer();
            if (CaretIndex >= Text.Length) return;
            int backupCaretIndex = CaretIndex;
            int count = (CaretIndex < Text.Length - 1 && Text.Substring(CaretIndex, NLLength) == NL) ? NLLength : 1;
            Text = Text.Remove(CaretIndex, count);
            CaretIndex = backupCaretIndex;
            UpdateColumnIndex();
        }

        public void DeleteBackwardChar()
        {
            ClearKilledTextBuffer();
            if (CaretIndex <= 0) return;
            int backupCaretIndex = CaretIndex;
            int count = (CaretIndex > 1 && Text.Substring(CaretIndex - NLLength, NLLength) == NL) ? NLLength : 1;
            Text = Text.Remove(CaretIndex - count, count);
            CaretIndex = backupCaretIndex - count;
            UpdateColumnIndex();
        }

        public void KillLine()
        {
            if (CaretIndex >= Text.Length) return;
            int backupCaretIndex = CaretIndex;
            int iCurrLineEnd = GetCurrentLineEndIndex();
            int iKillEnd = (iCurrLineEnd == CaretIndex) ? iCurrLineEnd + NLLength : iCurrLineEnd;
            SetKilledTextToClipboard(CaretIndex, iKillEnd, combinesWithBuf: true);
            Text = Text.Substring(0, CaretIndex) + Text.Substring(iKillEnd);
            CaretIndex = backupCaretIndex;
            UpdateColumnIndex();
        }

        public void Yank()
        {
            ClearKilledTextBuffer();
            if (GetTextFromClipboard == null) return;
            string textToInsert = GetTextFromClipboard();
            if (string.IsNullOrEmpty(textToInsert)) return;
            int backupCaretIndex = CaretIndex;
            Text = Text.Insert(CaretIndex, textToInsert);
            CaretIndex = backupCaretIndex + textToInsert.Length;
            UpdateColumnIndex();
        }

        #endregion

        #region TryHandleKeyEvent

        Dictionary<Key, Action> keyToAction;

        public bool TryHandleKeyEvent(KeyEventArgs e)
        {
            if (keyToAction == null)
            {
                keyToAction = new Dictionary<Key, Action>
                {
                    { Key.F, ForwardChar },
                    { Key.B, BackwardChar },
                    { Key.A, MoveBeginningOfLine },
                    { Key.E, MoveEndOfLine },
                    { Key.N, NextLine },
                    { Key.P, PreviousLine },
                    { Key.D, DeleteChar },
                    { Key.H, DeleteBackwardChar },
                    { Key.K, KillLine },
                    { Key.Y, Yank },
                };
            }
            Action action;
            if (keyToAction.TryGetValue(e.Key, out action))
            {
                action();
                return true;
            }
            return false;
        }

        #endregion

        #region TryRemoveConflictKeyBindings

        public static bool TryRemoveConflictKeyBindings(EnvDTE.DTE dte, out string errorMessage)
        {
            errorMessage = "";
            bool success = true;

            foreach (EnvDTE.Command command in dte.Commands)
            {
                var bindings = command.Bindings as object[];
                if (bindings == null || !bindings.Any()) continue;
                var bindingBuf = bindings.OfType<string>().ToList();
                for (int iBinding = 0; iBinding < bindingBuf.Count;)
                {
                    var currBinding = bindingBuf[iBinding];
                    int iColonX2 = currBinding.IndexOf("::");
                    if (iColonX2 == -1)
                    {
                        ++iBinding;
                        continue;
                    }
                    int iFirstKeyStart = iColonX2 + 2;
                    int iFirstCamma = currBinding.IndexOf(",", iFirstKeyStart);
                    int firstKeyLength = (iFirstCamma == -1) ? currBinding.Length - iFirstKeyStart : iFirstCamma - iFirstKeyStart;
                    if (firstKeyLength <= 0)
                    {
                        ++iBinding;
                        continue;
                    }
                    string firstKey = bindingBuf[iBinding].Substring(iFirstKeyStart, firstKeyLength);
                    if (firstKey == "Ctrl+F" && command.Name != "Edit.EmacsCharRight")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+B" && command.Name != "Edit.EmacsCharLeft")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+A" && command.Name != "Edit.EmacsLineStart")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+E" && command.Name != "Edit.EmacsLineEnd")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+N" && command.Name != "Edit.LineDown")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+P" && command.Name != "Edit.LineUp")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+D" && command.Name != "Edit.Delete")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+H" && command.Name != "Edit.DeleteBackwards")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+K" && command.Name != "Edit.EmacsDeleteToEOL")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else if (firstKey == "Ctrl+Y" && command.Name != "Edit.Paste")
                    {
                        bindingBuf.RemoveAt(iBinding);
                    }
                    else
                    {
                        ++iBinding;
                    }
                }
                if (bindings.Length != bindingBuf.Count)
                {
                    try
                    {
                        command.Bindings = bindingBuf.Cast<object>().ToArray();
                    }
                    catch
                    {
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            errorMessage += Environment.NewLine;
                        }
                        errorMessage += "Failed: \"" + command.Name + "\", " + string.Join(", ", bindings.OfType<string>().Select(b => "\"" + b + "\""));
                        success = false;
                    }
                }
            }

            return success;
        }

        #endregion
    }
}
