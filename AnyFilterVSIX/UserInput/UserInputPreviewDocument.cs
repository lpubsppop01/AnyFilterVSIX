using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace lpubsppop01.AnyFilterVSIX
{
    class UserInputPreviewDocument
    {
        #region Constructor

        string previewText;
        IList<DiffMatchPatch.Diff> diffs;

        public UserInputPreviewDocument(string previewText, string inputText)
        {
            this.previewText = previewText;
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

            public LineTag(int lineIndex, bool hasDifferecne)
            {
                LineIndex = lineIndex;
                HasDifference = hasDifferecne;
            }

            #endregion

            #region Properties

            public int LineIndex { get; private set; }
            public bool HasDifference { get; private set; }

            #endregion
        }

        public FlowDocument ToFlowDocument()
        {
            var previewDoc = new FlowDocument
            {
                PageWidth = 2000, // disable wrapping ref. http://stackoverflow.com/questions/1368047/c-wpf-disable-text-wrap-of-richtextbox
                LineHeight = 1
            };
            if (diffs != null)
            {
                int iLine = 0;
                bool lineHasDiff = false;
                var para = new Paragraph();
                foreach (var diff in diffs)
                {
                    bool isFirstSplitted = true;
                    foreach (var splitted in diff.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        if (isFirstSplitted)
                        {
                            isFirstSplitted = false;
                        }
                        else
                        {
                            para.Tag = new LineTag(iLine++, lineHasDiff);
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
                para.Tag = new LineTag(iLine, lineHasDiff);
                previewDoc.Blocks.Add(para);
            }
            else
            {
                int iLine = 0;
                foreach (var line in previewText.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    previewDoc.Blocks.Add(new Paragraph(new Run(line)) { Tag = new LineTag(iLine++, false) });
                }
            }
            return previewDoc;
        }

        #endregion
    }
}
