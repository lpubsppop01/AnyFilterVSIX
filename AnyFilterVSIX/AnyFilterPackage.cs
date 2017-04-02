using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;

namespace lpubsppop01.AnyFilterVSIX
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")] // Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution
    [Guid(GuidList.guidAnyFilterPkgString)]
    public sealed class AnyFilterPackage : Package
    {
        #region Overrides

        protected override void Initialize()
        {
            base.Initialize();

            if (!TryCacheRequiredServices()) return;

            AddSettingsMenuItem();
            UpdateRunFilterMenuItems();
        }

        #endregion

        #region Service Cache

        OleMenuCommandService menuCommandService;
        IVsTextManager textManager;
        IVsEditorAdaptersFactoryService editorAdapterFactory;
        string textEditorFontName;
        int textEditorFontSizePt;

        bool TryCacheRequiredServices()
        {
            menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (menuCommandService == null) return false;
            textManager = GetService(typeof(SVsTextManager)) as IVsTextManager;
            if (textManager == null) return false;
            var componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            if (componentModel == null) return false;
            editorAdapterFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            if (editorAdapterFactory == null) return false;
            var store = GetService(typeof(IVsFontAndColorStorage)) as IVsFontAndColorStorage;
            if (store == null) return false;
            GetTextEditorFont(store, out textEditorFontName, out textEditorFontSizePt);
            return true;
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
                store.GetFont(logFont, fontInfo);
                fontName = fontInfo[0].bstrFaceName;
                fontSize = fontInfo[0].wPointSize;
            }
            finally
            {
                store.CloseCategory();
            }
        }

        bool TryGetWpfTextView(out IWpfTextView wpfTextView)
        {
            wpfTextView = null;
            IVsTextView textView;
            textManager.GetActiveView(1, null, out textView);
            if (textView == null) return false;
            wpfTextView = editorAdapterFactory.GetWpfTextView(textView);
            return true;
        }

        #endregion

        #region Menu Commands

        void AddSettingsMenuItem()
        {
            var menuCommandID = new CommandID(GuidList.guidAnyFilterCmdSet, (int)PkgCmdIDList.cmdidSettings);
            var menuItem = new MenuCommand((sender, e) =>
            {
                var dialog = new AnyFilterSettingsWindow
                {
                    ItemsSource = new ObservableCollection<Filter>(AnyFilterSettings.Current.Filters),
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                if (dialog.ShowDialog() ?? false)
                {
                    AnyFilterSettings.Current.Filters.Clear();
                    AnyFilterSettings.Current.Filters.AddRange(dialog.ItemsSource.Select(c => c.Clone()));
                    AnyFilterSettings.SaveCurrent();

                    UpdateRunFilterMenuItems();
                }
            }, menuCommandID);
            menuCommandService.AddCommand(menuItem);
        }

        void UpdateRunFilterMenuItems()
        {
            int filterCount = AnyFilterSettings.Current.Filters.Count;
            for (int iFilter = 0; iFilter < filterCount; ++iFilter)
            {
                RemoveRunFilterMenuItem(iFilter);
                AddRunFilterMenuItem(iFilter);
            }
            for (int iFilter = filterCount; iFilter < PkgCmdIDList.FilterCountMax; ++iFilter)
            {
                RemoveRunFilterMenuItem(iFilter);
            }
        }

        void RemoveRunFilterMenuItem(int iFilter)
        {
            var menuCommandID = new CommandID(GuidList.guidAnyFilterCmdSet, (int)PkgCmdIDList.GetCmdidRunFilter(iFilter));
            var menuItem = menuCommandService.FindCommand(menuCommandID);
            if (menuItem == null) return;
            menuCommandService.RemoveCommand(menuItem);
        }

        void AddRunFilterMenuItem(int iFilter)
        {
            var filter = AnyFilterSettings.Current.Filters[iFilter];
            if (filter == null) return;

            var menuCommandID = new CommandID(GuidList.guidAnyFilterCmdSet, (int)PkgCmdIDList.GetCmdidRunFilter(iFilter));
            var menuItem = new OleMenuCommand((sender, e) =>
            {
                IWpfTextView wpfTextView;
                if (!TryGetWpfTextView(out wpfTextView)) return;

                // Get target spans
                var targetSpans = new List<SnapshotSpan>();
                bool forcesInsertsAfterCurrentLine = false;
                foreach (var span in wpfTextView.Selection.SelectedSpans.Where(s => s.Length > 0))
                {
                    targetSpans.Add(span);
                }
                if (!targetSpans.Any())
                {
                    var currLine = wpfTextView.TextViewLines.GetTextViewLineContainingBufferPosition(wpfTextView.Caret.Position.BufferPosition);
                    targetSpans.Add(currLine.Extent);
                    forcesInsertsAfterCurrentLine = true;
                }

                // Get user input
                string userInputText = "";
                if (filter.ContainsVariable(FilterRunner.VariableName_UserInput, FilterRunner.VariableName_UserInputTempFilePath))
                {
                    var support = new RepeatedAsyncTaskSupport();
                    var buffer = new UserInputBuffer();
                    buffer.PropertyChanged += (sender_, e_) =>
                    {
                        if (e_.PropertyName != "InputText") return;
                        if (!support.TryBegin()) return;
                        System.Threading.Tasks.Task.Factory.StartNew(async () =>
                        {
                            do
                            {
                                var previewTextBuf = new StringBuilder();
                                foreach (var span in targetSpans)
                                {
                                    previewTextBuf.Append(await FilterRunner.RunAsync(filter, span.GetText(), buffer.InputText));
                                }
                                buffer.PreviewText = previewTextBuf.ToString();
                            } while (support.TryContinue());
                            support.End();
                        });
                    };
                    var dialog = new UserInputWindow
                    {
                        DataContext = buffer,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        UsesEmacsLikeKeybindings = AnyFilterSettings.Current.UsesEmacsLikeKeybindings
                    };
                    dialog.Title = "AnyFilter " + filter.Title;
                    dialog.SetFont(textEditorFontName, textEditorFontSizePt);
                    bool dialogResultIsOK = dialog.ShowDialog() ?? false;
                    AnyFilterSettings.Current.UsesEmacsLikeKeybindings = dialog.UsesEmacsLikeKeybindings;
                    AnyFilterSettings.SaveCurrent();
                    if (!dialogResultIsOK) return;
                    userInputText = buffer.InputText;
                }

                // Edit text buffer
                var textEdit = wpfTextView.TextBuffer.CreateEdit();
                foreach (var span in targetSpans)
                {
                    string resultText = FilterRunner.Run(filter, span.GetText(), userInputText);
                    if (filter.InsertsAfterCurrentLine || forcesInsertsAfterCurrentLine)
                    {
                        var currLine = wpfTextView.TextViewLines.GetTextViewLineContainingBufferPosition(span.End);
                        textEdit.Insert(currLine.End, Environment.NewLine + resultText);
                    }
                    else
                    {
                        textEdit.Delete(span);
                        textEdit.Insert(span.Start, resultText);
                    }
                }
                textEdit.Apply();
            }, menuCommandID)
            {
                Text = filter.Title
            };
            menuCommandService.AddCommand(menuItem);
        }

        #endregion
    }
}
