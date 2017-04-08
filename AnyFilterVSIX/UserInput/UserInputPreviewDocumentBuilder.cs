using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace lpubsppop01.AnyFilterVSIX
{
    static class UserInputPreviewDocumentBuilder
    {
        public static FlowDocument Build(string previewText, string inputText, bool showsDiff)
        {
            var previewDoc = new FlowDocument();
            previewDoc.Blocks.Add(new Paragraph(new Run(previewText)));
            return previewDoc;
        }
    }
}
