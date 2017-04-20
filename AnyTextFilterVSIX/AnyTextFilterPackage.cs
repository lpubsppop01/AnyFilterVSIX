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
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;

namespace lpubsppop01.AnyTextFilterVSIX
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")] // Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution
    [Guid(GuidList.guidAnyTextFilterPkgString)]
    public sealed class AnyTextFilterPackage : Package
    {
        #region Overrides

        protected override void Initialize()
        {
            base.Initialize();

            if (!TryCacheRequiredServices()) return;

            NameToResourceBinding.Culture = AnyTextFilterSettings.Current.Culture;
            AnyTextFilterSettings.Current.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != "Culture") return;
                NameToResourceBinding.Culture = AnyTextFilterSettings.Current.Culture;
            };

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
            var menuCommandID = new CommandID(GuidList.guidAnyTextFilterCmdSet, (int)PkgCmdIDList.cmdidSettings);
            OleMenuCommand menuItem = null;
            menuItem = new OleMenuCommand((sender, e) =>
            {
                var backup = AnyTextFilterSettings.Current.Clone();
                var dialog = new AnyTextFilterSettingsWindow
                {
                    DataContext = AnyTextFilterSettings.Current,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                if (dialog.ShowDialog() ?? false)
                {
                    AnyTextFilterSettings.SaveCurrent();
                    menuItem.Text = Properties.Resources.AnyTextFilterSettings_;
                    UpdateRunFilterMenuItems();
                }
                else
                {
                    AnyTextFilterSettings.Current.Copy(backup);
                }
            }, menuCommandID)
            {
                Text = Properties.Resources.AnyTextFilterSettings_
            };
            menuCommandService.AddCommand(menuItem);
        }

        void UpdateRunFilterMenuItems()
        {
            int filterCount = AnyTextFilterSettings.Current.Filters.Count;
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
            var menuCommandID = new CommandID(GuidList.guidAnyTextFilterCmdSet, (int)PkgCmdIDList.GetCmdidRunFilter(iFilter));
            var menuItem = menuCommandService.FindCommand(menuCommandID);
            if (menuItem == null) return;
            menuCommandService.RemoveCommand(menuItem);
        }

        void AddRunFilterMenuItem(int iFilter)
        {
            var filter = AnyTextFilterSettings.Current.Filters[iFilter];
            if (filter == null) return;

            var menuCommandID = new CommandID(GuidList.guidAnyTextFilterCmdSet, (int)PkgCmdIDList.GetCmdidRunFilter(iFilter));
            var menuItem = new OleMenuCommand((sender, e) =>
            {
                IWpfTextView wpfTextView;
                if (!TryGetWpfTextView(out wpfTextView)) return;

                // Get target spans
                var targetSpans = new List<SnapshotSpan>();
                SnapshotSpan? snapshotSpanToVisible = null;
                foreach (var span in wpfTextView.Selection.SelectedSpans.Where(s => s.Length > 0))
                {
                    targetSpans.Add(span);
                }
                if (targetSpans.Any())
                {
                    snapshotSpanToVisible = targetSpans.First();
                }
                else
                {
                    if (filter.TargetSpanForNoSelection == TargetSpanForNoSelection.CaretPosition)
                    {
                        targetSpans.Add(new SnapshotSpan(wpfTextView.TextSnapshot, new Span(wpfTextView.Caret.Position.BufferPosition, 0)));
                        var currLine = wpfTextView.TextViewLines.GetTextViewLineContainingBufferPosition(wpfTextView.Caret.Position.BufferPosition);
                        snapshotSpanToVisible = currLine.Extent;
                    }
                    else if (filter.TargetSpanForNoSelection == TargetSpanForNoSelection.CurrentLine)
                    {
                        var currLine = wpfTextView.TextViewLines.GetTextViewLineContainingBufferPosition(wpfTextView.Caret.Position.BufferPosition);
                        targetSpans.Add(currLine.Extent);
                        snapshotSpanToVisible = currLine.Extent;
                    }
                    else if (filter.TargetSpanForNoSelection == TargetSpanForNoSelection.WholeDocument)
                    {
                        targetSpans.Add(new SnapshotSpan(wpfTextView.TextSnapshot, new Span(0, wpfTextView.TextSnapshot.Length)));
                    }
                }

                // Get connected input text
                string rawInputText = string.Join("", targetSpans.Select(s => s.GetText()));
                var envNewLineKind = Environment.NewLine.ToNewLineKind();
                var srcNewLineKind = rawInputText.DetectNewLineKind() ?? envNewLineKind;

                // Get user input
                string userInputText = "";
                if (filter.ContainsVariable(FilterRunner.VariableName_UserInput, FilterRunner.VariableName_UserInputTempFilePath))
                {
                    var support = new RepeatedAsyncTaskSupport();
                    int tabSize = wpfTextView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
                    string envNLInputText = rawInputText.ConvertNewLine(srcNewLineKind, envNewLineKind);
                    var buffer = new UserInputBuffer { ShowsDifference = filter.UserInputWindow_ShowsDifference };
                    buffer.PropertyChanged += (sender_, e_) =>
                    {
                        if (e_.PropertyName != "UserInputText" && e_.PropertyName != "ShowsDifference") return;
                        if (!support.TryBegin()) return;
                        System.Threading.Tasks.Task.Factory.StartNew(async () =>
                        {
                            do
                            {
                                var previewTextBuf = new StringBuilder();
                                bool cancelled = false;
                                foreach (var span in targetSpans)
                                {
                                    string envNLSpanText = span.GetText().ConvertNewLine(srcNewLineKind, envNewLineKind);
                                    var filterResult = await FilterRunner.RunAsync(filter, envNLSpanText, buffer.UserInputText, support);
                                    if (filterResult.Kind == FilterResultKind.Cancelled)
                                    {
                                        cancelled = true;
                                        break;
                                    }
                                    previewTextBuf.Append(filterResult.OutputText);
                                }
                                if (cancelled) continue;
                                buffer.PreviewDocument = new UserInputPreviewDocument(previewTextBuf.ToString(), tabSize, buffer.ShowsDifference ? envNLInputText : null);
                            } while (support.TryContinue());
                            support.End();
                        });
                    };
                    buffer.UserInputText = "";
                    var dialog = new UserInputWindow
                    {
                        DataContext = buffer,
                        Owner = Window.GetWindow(wpfTextView.VisualElement),
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        UsesEmacsLikeKeybindings = AnyTextFilterSettings.Current.UsesEmacsLikeKeybindings,
                    };
                    dialog.MoveToNextPreviousDifferenceDone += (sender_, e_) =>
                    {
                        // ref. http://stackoverflow.com/questions/6186925/visual-studio-extensibility-move-to-line-in-a-textdocument
                        var startLine = wpfTextView.TextSnapshot.GetLineFromPosition(targetSpans.First().Start.Position);
                        if (startLine == null) return;
                        int lineNumber = e_.LineIndex + startLine.LineNumber;
                        var targetLine = wpfTextView.TextSnapshot.Lines.FirstOrDefault(l => l.LineNumber == lineNumber);
                        if (targetLine == null) return;
                        var span = Span.FromBounds(targetLine.Start.Position, targetLine.End.Position);
                        var snapshotSpan = new SnapshotSpan(wpfTextView.TextSnapshot, span);
                        wpfTextView.ViewScroller.EnsureSpanVisible(snapshotSpan, EnsureSpanVisibleOptions.AlwaysCenter);
                        wpfTextView.Caret.MoveTo(snapshotSpan.Start);
                    };
                    dialog.Title = "AnyTextFilter " + filter.Title;
                    dialog.SetPosition(wpfTextView.VisualElement);
                    dialog.SetFont(textEditorFontName, textEditorFontSizePt);
                    if (snapshotSpanToVisible.HasValue)
                    {
                        wpfTextView.ViewScroller.EnsureSpanVisible(snapshotSpanToVisible.Value, EnsureSpanVisibleOptions.AlwaysCenter);
                    }
                    bool dialogResultIsOK = dialog.ShowDialog() ?? false;
                    filter.UserInputWindow_ShowsDifference = buffer.ShowsDifference;
                    AnyTextFilterSettings.Current.UsesEmacsLikeKeybindings = dialog.UsesEmacsLikeKeybindings;
                    AnyTextFilterSettings.SaveCurrent();
                    if (!dialogResultIsOK) return;
                    userInputText = buffer.UserInputText;
                }

                // Edit text buffer
                var textEdit = wpfTextView.TextBuffer.CreateEdit();
                foreach (var span in targetSpans)
                {
                    string envNLSpanText = span.GetText().ConvertNewLine(srcNewLineKind, envNewLineKind);
                    string envNLResultText = FilterRunner.Run(filter, envNLSpanText, userInputText).OutputText;
                    string resultText = envNLResultText.ConvertNewLine(envNewLineKind, srcNewLineKind);
                    if (filter.InsertsAfterTargetSpan)
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
