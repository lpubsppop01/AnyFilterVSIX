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

        public FlowDocument ToFlowDocument()
        {
            var previewDoc = new FlowDocument();
            if (diffs != null)
            {
                var para = new Paragraph();
                foreach (var diff in diffs)
                {
                    var run = new Run(diff.Text);
                    if (diff.Operation.IsInsert)
                    {
                        run.TextDecorations.Add(TextDecorations.Underline);
                    }
                    else if (diff.Operation.IsDelete)
                    {
                        run.Foreground = Brushes.Silver;
                        run.TextDecorations.Add(TextDecorations.Strikethrough);
                    }
                    para.Inlines.Add(run);
                }
                previewDoc.Blocks.Add(para);
            }
            else
            {
                previewDoc.Blocks.Add(new Paragraph(new Run(previewText)));
            }
            return previewDoc;
        }

        #endregion
    }
}
