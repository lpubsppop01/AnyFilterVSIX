using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class UserInputPreviewDocument
    {
        #region Constructor

        string previewText;
        int tabSize;
        IList<DiffMatchPatch.Diff> diffs;

        public UserInputPreviewDocument(string previewText, int tabSize, string inputText)
        {
            this.previewText = previewText;
            this.tabSize = tabSize;
            if (inputText != null)
            {
                var dmp = DiffMatchPatch.DiffMatchPatchModule.Default;
                this.diffs = dmp.DiffMain(inputText, previewText);
            }
        }

        #endregion

        #region Convert

        public class LineTag
        {
            #region Constructor

            public LineTag(int lineIndex)
            {
                LineIndex = lineIndex;
            }

            #endregion

            #region Properties

            public int LineIndex { get; private set; }
            public bool HasDifference { get; set; }
            public bool IsPartOfLastNewLines { get; set; }

            #endregion
        }

        public FlowDocument ToFlowDocument(double fontSizePx)
        {
            var previewDoc = new FlowDocument { LineHeight = fontSizePx * 0.1 };
            string tabSizeSpaces = string.Concat((Enumerable.Repeat(' ', tabSize)));
            if (diffs != null)
            {
                int iLine = 0;
                bool lineHasDiff = false;
                var para = new Paragraph();
                foreach (var diff in diffs)
                {
                    string untabified = diff.Text.Replace("\t", tabSizeSpaces);
                    bool isFirstSplitted = true;
                    foreach (var splitted in untabified.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        if (isFirstSplitted)
                        {
                            isFirstSplitted = false;
                        }
                        else
                        {
                            para.Tag = new LineTag(iLine++) { HasDifference = lineHasDiff };
                            previewDoc.Blocks.Add(para);
                            lineHasDiff = false;
                            para = new Paragraph();
                        }

                        var run = new Run(splitted);
                        if (diff.Operation.IsInsert)
                        {
                            run.TextDecorations.Add(TextDecorations.Underline);
                            lineHasDiff = true;
                        }
                        else if (diff.Operation.IsDelete)
                        {
                            run.Foreground = Brushes.Silver;
                            run.TextDecorations.Add(TextDecorations.Strikethrough);
                            lineHasDiff = true;
                        }
                        para.Inlines.Add(run);
                    }
                }
                para.Tag = new LineTag(iLine) { HasDifference = lineHasDiff };
                previewDoc.Blocks.Add(para);
            }
            else
            {
                var splitLines = previewText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                for(int iLine = 0; iLine < splitLines.Length; ++iLine)
                {
                    string untabified = splitLines[iLine].Replace("\t", tabSizeSpaces);
                    previewDoc.Blocks.Add(new Paragraph(new Run(untabified)) { Tag = new LineTag(iLine) });
                }
            }
            previewDoc.PageWidth = previewDoc.Blocks.OfType<Paragraph>().Max(p => GetParagraphLengthPx(p, fontSizePx));
            foreach (var para in previewDoc.Blocks.OfType<Paragraph>().Reverse())
            {
                if (para.Inlines.OfType<Run>().Any(r => r.Text != "")) break;
                (para.Tag as LineTag).IsPartOfLastNewLines = true;
            }
            return previewDoc;
        }

        #endregion

        #region Line Length Calculation

        static double GetParagraphLengthPx(Paragraph para, double fontSizePx)
        {
            return para.Inlines.OfType<Run>().Select(r => GetStringLengthPx(r.Text, fontSizePx)).Sum();
        }

        static double GetStringLengthPx(string str, double fontSizePx)
        {
            double width = 0;
            foreach (char c in str)
            {
                width += IsMultiByteChar(c) ? (double)fontSizePx : (double)fontSizePx / 2;
            }
            return Math.Max(width * 1.1, 500);
        }

        static bool IsMultiByteChar(char c)
        {
            int byteCount = MyEncodingInfo.UTF8_WithoutBOM.GetEncoding().GetByteCount(new[] { c });
            return byteCount > 1;
        }

        #endregion
    }
}
