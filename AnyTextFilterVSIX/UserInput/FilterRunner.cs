using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class FilterRunner : INotifyPropertyChanged
    {
        #region Constructor

        Func<IWpfTextView> getWpfTextView;

        public FilterRunner(Func<IWpfTextView> getWpfTextView)
        {
            this.getWpfTextView = getWpfTextView;
        }

        #endregion

        #region Properties

        bool hasUserInputVariables;
        public bool HasUserInputVariables
        {
            get { return hasUserInputVariables; }
            set { hasUserInputVariables = value; OnPropertyChanged(); }
        }

        string userInputText = "";
        public string UserInputText
        {
            get { return userInputText; }
            set { userInputText = value; OnPropertyChanged(); }
        }

        UserInputPreviewDocument previewDocument;
        public UserInputPreviewDocument PreviewDocument
        {
            get { return previewDocument; }
            set { previewDocument = value; OnPropertyChanged(); }
        }

        bool showsDifference;
        public bool ShowsDifference
        {
            get { return showsDifference; }
            set { showsDifference = value; OnPropertyChanged(); }
        }

        bool m_IsRunning;
        public bool IsRunning
        {
            get { return m_IsRunning; }
            private set { m_IsRunning = value; OnPropertyChanged(); }
        }

        public FilterHistoryManager HistoryManager { get; private set; } = new FilterHistoryManager();

        public string ViewText
        {
            get
            {
                var wpfTextView = getWpfTextView();
                if (wpfTextView == null) return "";
                return wpfTextView.TextSnapshot.GetText();
            }
        }

        #endregion

        #region Start/Stop

        Filter filter;
        string envNLInputText;
        List<SnapshotSpan> targetSpans;
        NewLineKind srcNewLineKind, envNewLineKind;
        int tabSize;

        public void Start(Filter filter)
        {
            if (filter == null) return;
            var wpfTextView = getWpfTextView();
            if (wpfTextView == null) return;

            // Set filter and parameters
            this.filter = filter;
            this.tabSize = wpfTextView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
            ShowsDifference = filter.UserInputWindow_ShowsDifference;
            HasUserInputVariables = filter.ContainsVariable(FilterProcess.VariableName_UserInput, FilterProcess.VariableName_UserInputTempFilePath);

            // Set target spans
            SnapshotSpan? spanToVisible;
            UpdateTargetSpans(filter, wpfTextView, out spanToVisible);

            // Analyze input text
            string rawInputText = string.Join("", targetSpans.Select(s => s.GetText()));
            envNewLineKind = Environment.NewLine.ToNewLineKind();
            srcNewLineKind = rawInputText.DetectNewLineKind() ?? envNewLineKind;
            envNLInputText = rawInputText.ConvertNewLine(srcNewLineKind, envNewLineKind);

            // Scroll main view to make target span visible
            if (spanToVisible.HasValue)
            {
                wpfTextView.ViewScroller.EnsureSpanVisible(spanToVisible.Value, EnsureSpanVisibleOptions.AlwaysCenter);
            }

            // Start first filtering
            TryStartFilteringTask();

            // Start watching
            IsRunning = true;
            PropertyChanged += this_PropertyChanged;
            taskSupport.PropertyChanged += taskSupport_PropertyChanged;
        }

        void UpdateTargetSpans(Filter filter, IWpfTextView wpfTextView, out SnapshotSpan? spanToVisible)
        {
            spanToVisible = null;

            targetSpans = new List<SnapshotSpan>();
            foreach (var span in wpfTextView.Selection.SelectedSpans.Where(s => s.Length > 0))
            {
                targetSpans.Add(span);
            }
            if (targetSpans.Any())
            {
                spanToVisible = targetSpans.First();
            }
            else
            {
                if (filter.TargetSpanForNoSelection == TargetSpanForNoSelection.CaretPosition)
                {
                    targetSpans.Add(new SnapshotSpan(wpfTextView.TextSnapshot, new Span(wpfTextView.Caret.Position.BufferPosition, 0)));
                    var currLine = wpfTextView.TextViewLines.GetTextViewLineContainingBufferPosition(wpfTextView.Caret.Position.BufferPosition);
                    spanToVisible = currLine.Extent;
                }
                else if (filter.TargetSpanForNoSelection == TargetSpanForNoSelection.CurrentLine)
                {
                    var currLine = wpfTextView.TextViewLines.GetTextViewLineContainingBufferPosition(wpfTextView.Caret.Position.BufferPosition);
                    targetSpans.Add(currLine.Extent);
                    spanToVisible = currLine.Extent;
                }
                else if (filter.TargetSpanForNoSelection == TargetSpanForNoSelection.WholeDocument)
                {
                    targetSpans.Add(new SnapshotSpan(wpfTextView.TextSnapshot, new Span(0, wpfTextView.TextSnapshot.Length)));
                }
            }
        }

        public void Stop()
        {
            // Stop watching
            PropertyChanged -= this_PropertyChanged;
            taskSupport.PropertyChanged -= taskSupport_PropertyChanged;
            IsRunning = false;

            // Save settings
            if (filter == null) return;
            filter.UserInputWindow_ShowsDifference = ShowsDifference;
            AnyTextFilterSettings.SaveCurrent();
        }

        #endregion

        #region Event Handlers

        RepeatedAsyncTaskSupport taskSupport = new RepeatedAsyncTaskSupport();
        IList<string> lastResult;

        void this_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "UserInputText" && e.PropertyName != "ShowsDifference") return;
            TryStartFilteringTask();
        }

        void taskSupport_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsRunning") return;
            IsRunning = taskSupport.IsRunning;
        }

        void TryStartFilteringTask()
        {
            if (!taskSupport.TryStart()) return;
            Task.Factory.StartNew(async () =>
            {
                do
                {
                    var resultTextBuf = new List<string>();
                    bool cancelled = false;
                    foreach (var span in targetSpans)
                    {
                        string envNLSpanText = span.GetText().ConvertNewLine(srcNewLineKind, envNewLineKind);
                        var filterResult = await FilterProcess.RunAsync(filter, envNLSpanText, UserInputText, taskSupport);
                        if (filterResult.Kind == FilterResultKind.Cancelled)
                        {
                            cancelled = true;
                            break;
                        }
                        resultTextBuf.Add(filterResult.OutputText);
                    }
                    if (cancelled) continue;
                    lastResult = resultTextBuf.Select(s => s.ConvertNewLine(envNewLineKind, srcNewLineKind)).ToArray();
                    var previewText = string.Concat(resultTextBuf);
                    PreviewDocument = new UserInputPreviewDocument(previewText, tabSize, ShowsDifference ? envNLInputText : null);
                } while (taskSupport.CheckRepeat());
                taskSupport.Stop();
            });
        }

        #endregion

        #region Apply

        public void ApplyLastResult()
        {
            var wpfTextView = getWpfTextView();

            var textEdit = wpfTextView.TextBuffer.CreateEdit();
            int iSpan = 0;
            foreach (var span in targetSpans)
            {
                string resultText = lastResult[iSpan];
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

            HistoryManager.AddHistoryItem(new FilterHistoryItem
            {
                FilterID = filter.ID,
                UserInputText = UserInputText
            });
            UserInputText = "";
        }

        #endregion

        #region Relay to Main View

        public void EnsureMainViewLineVisible(int lineIndex)
        {
            // ref. http://stackoverflow.com/questions/6186925/visual-studio-extensibility-move-to-line-in-a-textdocument
            var wpfTextView = getWpfTextView();
            if (wpfTextView == null) return;
            var startLine = wpfTextView.TextSnapshot.GetLineFromPosition(targetSpans.First().Start.Position);
            if (startLine == null) return;
            int lineNumber = lineIndex + startLine.LineNumber;
            var targetLine = wpfTextView.TextSnapshot.Lines.FirstOrDefault(l => l.LineNumber == lineNumber);
            if (targetLine == null) return;
            var span = Span.FromBounds(targetLine.Start.Position, targetLine.End.Position);
            var snapshotSpan = new SnapshotSpan(wpfTextView.TextSnapshot, span);
            wpfTextView.ViewScroller.EnsureSpanVisible(snapshotSpan, EnsureSpanVisibleOptions.AlwaysCenter);
            wpfTextView.Caret.MoveTo(snapshotSpan.Start);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
