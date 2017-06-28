using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace lpubsppop01.AnyTextFilterVSIX
{
    [Guid("C901AA6B-2B92-44C7-98F5-1E7089EAC5E0")]
    public class FilterRunnerWindowPane : ToolWindowPane
    {
        #region Constructor

        public FilterRunnerWindowPane()
        {
            Caption = Properties.Resources.AnyTextFilter;
            Content = new FilterRunnerControl();
            Content.Loaded += Content_Loaded;
            Content.Applied += Content_Applied;
        }

        #endregion

        #region Properties

        internal new FilterRunnerControl Content
        {
            get { return base.Content as FilterRunnerControl; }
            set { base.Content = value; }
        }

        internal new IVsWindowFrame Frame
        {
            get { return base.Frame as IVsWindowFrame; }
            set { base.Frame = value; }
        }

        #endregion

        #region Service Cache

        IVsTextManager textManager;
        IVsEditorAdaptersFactoryService editorAdapterFactory;
        string textEditorFontName;
        int textEditorFontSizePt;

        bool TryCacheRequiredServices()
        {
            bool updated = false;
            if (textManager == null)
            {
                textManager = GetService(typeof(SVsTextManager)) as IVsTextManager;
                if (textManager == null) return false;
                updated |= true;
            }
            if (editorAdapterFactory == null)
            {
                var componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
                if (componentModel == null) return false;
                editorAdapterFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
                if (editorAdapterFactory == null) return false;
                updated |= true;
            }
            if (string.IsNullOrEmpty(textEditorFontName))
            {
                var store = GetService(typeof(IVsFontAndColorStorage)) as IVsFontAndColorStorage;
                if (store == null) return false;
                GetTextEditorFont(store, out textEditorFontName, out textEditorFontSizePt);
                updated |= true;
            }
            return updated;
        }

        static void GetTextEditorFont(IVsFontAndColorStorage store, out string fontName, out int fontSize)
        {
            // ref. https://social.msdn.microsoft.com/Forums/vstudio/en-US/19a2c13d-86ac-4713-9897-88cc585201f1/vsix-how-to-get-colors-in-an-editor-extension?forum=vsx

            fontName = "Consolas";
            fontSize = 10;

            var textEditorGuid = new Guid("A27B4E24-A735-4D1D-B8E7-9716E1E3D8E0");
            store.OpenCategory(ref textEditorGuid, (uint)__FCSTORAGEFLAGS.FCSF_READONLY);
            try
            {
                var logFont = new LOGFONTW[1];
                var fontInfo = new FontInfo[1];
                if (store.GetFont(logFont, fontInfo) == VSConstants.S_OK)
                {
                    fontName = fontInfo[0].bstrFaceName;
                    fontSize = fontInfo[0].wPointSize;
                }
            }
            finally
            {
                store.CloseCategory();
            }
        }

        IWpfTextView GetWpfTextView()
        {
            IVsTextView textView;
            textManager.GetActiveView(1, null, out textView);
            if (textView == null) return null;
            return editorAdapterFactory.GetWpfTextView(textView);
        }

        #endregion

        #region Event Handlers

        void Content_Loaded(object sender, RoutedEventArgs e)
        {
            if (TryCacheRequiredServices())
            {
                Content.SetFont(textEditorFontName, textEditorFontSizePt);
                Content.FilterRunner = new FilterRunner(GetWpfTextView);
                if (Content.IsKeyboardFocusWithin)
                {
                    Content.FilterRunner.Start( Content.SelectedFilter);
                }
            }
        }

        void Content_Applied(object sender, EventArgs e)
        {
            var wpfTextView = GetWpfTextView();
            if (wpfTextView == null) return;
            wpfTextView.VisualElement.Focus();
        }

        #endregion
    }
}
