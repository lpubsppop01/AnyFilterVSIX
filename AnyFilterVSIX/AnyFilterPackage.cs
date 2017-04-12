﻿using System;
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

            NameToResourceBinding.Culture = AnyFilterSettings.Current.Culture;
            AnyFilterSettings.Current.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != "Culture") return;
                NameToResourceBinding.Culture = AnyFilterSettings.Current.Culture;
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
            var menuCommandID = new CommandID(GuidList.guidAnyFilterCmdSet, (int)PkgCmdIDList.cmdidSettings);
            var menuItem = new MenuCommand((sender, e) =>
            {
                var backup = AnyFilterSettings.Current.Clone();
                var dialog = new AnyFilterSettingsWindow
                {
                    DataContext = AnyFilterSettings.Current,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                if (dialog.ShowDialog() ?? false)
                {
                    AnyFilterSettings.SaveCurrent();
                    UpdateRunFilterMenuItems();
                }
                else
                {
                    AnyFilterSettings.Current.Copy(backup);
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
                foreach (var span in wpfTextView.Selection.SelectedSpans.Where(s => s.Length > 0))
                {
                    targetSpans.Add(span);
                }
                if (!targetSpans.Any())
                {
                    if (filter.TargetForNoSelection == TargetForNoSelection.CaretPosition)
                    {
                        targetSpans.Add(new SnapshotSpan(wpfTextView.VisualSnapshot, new Span(wpfTextView.Caret.Position.BufferPosition, 0)));
                    }
                    else if (filter.TargetForNoSelection == TargetForNoSelection.CurrentLine)
                    {
                        var currLine = wpfTextView.TextViewLines.GetTextViewLineContainingBufferPosition(wpfTextView.Caret.Position.BufferPosition);
                        targetSpans.Add(currLine.Extent);
                    }
                    else if (filter.TargetForNoSelection == TargetForNoSelection.WholeDocument)
                    {
                        targetSpans.Add(new SnapshotSpan(wpfTextView.VisualSnapshot, new Span(0, wpfTextView.VisualSnapshot.Length)));
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
                                foreach (var span in targetSpans)
                                {
                                    string envNLSpanText = span.GetText().ConvertNewLine(srcNewLineKind, envNewLineKind);
                                    previewTextBuf.Append(await FilterRunner.RunAsync(filter, envNLSpanText, buffer.UserInputText));
                                }
                                buffer.PreviewDocument = new UserInputPreviewDocument(previewTextBuf.ToString(), tabSize, buffer.ShowsDifference ? envNLInputText : null);
                            } while (support.TryContinue());
                            support.End();
                        });
                    };
                    buffer.UserInputText = "";
                    var dialog = new UserInputWindow
                    {
                        DataContext = buffer,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        UsesEmacsLikeKeybindings = AnyFilterSettings.Current.UsesEmacsLikeKeybindings,
                    };
                    if (filter.UserInputWindow_Width.HasValue) dialog.Width = filter.UserInputWindow_Width.Value;
                    if (filter.UserInputWindow_Height.HasValue) dialog.Height = filter.UserInputWindow_Height.Value;
                    dialog.MoveToNextPreviousDifferenceDone += (sender_, e_) =>
                    {
                        // ref. http://stackoverflow.com/questions/6186925/visual-studio-extensibility-move-to-line-in-a-textdocument
                        var startLine = wpfTextView.VisualSnapshot.GetLineFromPosition(targetSpans.First().Start.Position);
                        if (startLine == null) return;
                        int lineNumber = e_.LineIndex + startLine.LineNumber;
                        var targetLine = wpfTextView.VisualSnapshot.Lines.FirstOrDefault(l => l.LineNumber == lineNumber);
                        if (targetLine == null) return;
                        var span = Span.FromBounds(targetLine.Start.Position, targetLine.End.Position);
                        var snapshotSpan = new SnapshotSpan(wpfTextView.TextSnapshot, span);
                        wpfTextView.ViewScroller.EnsureSpanVisible(snapshotSpan);
                        wpfTextView.Caret.MoveTo(snapshotSpan.Start);
                    };
                    dialog.Title = "AnyFilter " + filter.Title;
                    dialog.SetFont(textEditorFontName, textEditorFontSizePt);
                    bool dialogResultIsOK = dialog.ShowDialog() ?? false;
                    filter.UserInputWindow_ShowsDifference = buffer.ShowsDifference;
                    filter.UserInputWindow_Width = dialog.ActualWidth;
                    filter.UserInputWindow_Height = dialog.ActualHeight;
                    AnyFilterSettings.Current.UsesEmacsLikeKeybindings = dialog.UsesEmacsLikeKeybindings;
                    AnyFilterSettings.SaveCurrent();
                    if (!dialogResultIsOK) return;
                    userInputText = buffer.UserInputText;
                }

                // Edit text buffer
                var textEdit = wpfTextView.TextBuffer.CreateEdit();
                foreach (var span in targetSpans)
                {
                    string envNLSpanText = span.GetText().ConvertNewLine(srcNewLineKind, envNewLineKind);
                    string envNLResultText = FilterRunner.Run(filter, envNLSpanText, userInputText);
                    string resultText = envNLResultText.ConvertNewLine(envNewLineKind, srcNewLineKind);
                    if (filter.InsertsAfterCurrentLine)
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
