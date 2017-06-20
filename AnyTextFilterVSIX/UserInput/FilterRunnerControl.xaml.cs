using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lpubsppop01.AnyTextFilterVSIX
{
    /// <summary>
    /// Interaction logic for FilterRunnerControl.xaml
    /// </summary>
    partial class FilterRunnerControl : UserControl
    {
        #region Constructor

        public FilterRunnerControl()
        {
            InitializeComponent();

            cmbFilter.ItemsSource = AnyTextFilterSettings.Current.Filters;
            cmbFilter.DisplayMemberPath = "Title";
            cmbFilter.SetBinding(ComboBox.SelectedValueProperty, new Binding("SelectedFilter")
            {
                Source = this
            });
            chkUsesEmacsKeybindings.SetBinding(CheckBox.IsCheckedProperty, new Binding("UsesEmacsLikeKeybindings")
            {
                Source= AnyTextFilterSettings.Current
            });
        }

        #endregion

        #region Properties

        public FilterRunner FilterRunner
        {
            get { return DataContext as FilterRunner; }
            set
            {
                var oldValue = FilterRunner;
                if (oldValue != null) oldValue.PropertyChanged -= FilterRunner_PropertyChanged;
                DataContext = value;
                var newValue = FilterRunner;
                if (newValue != null) newValue.PropertyChanged += FilterRunner_PropertyChanged;
            }
        }

        public Filter SelectedFilter
        {
            get { return (Filter)GetValue(SelectedFilterProperty); }
            set { SetValue(SelectedFilterProperty, value); }
        }

        public static readonly DependencyProperty SelectedFilterProperty = DependencyProperty.Register(
            "SelectedFilter", typeof(Filter), typeof(FilterRunnerControl), new PropertyMetadata(null));

        public int HistoryIndex
        {
            get { return (int)GetValue(HistoryIndexProperty); }
            set { SetValue(HistoryIndexProperty, value); }
        }

        public static readonly DependencyProperty HistoryIndexProperty = DependencyProperty.Register(
            "HistoryIndex", typeof(int), typeof(FilterRunnerControl), new PropertyMetadata(-1));

        #endregion

        #region Events

        public event EventHandler Applied;

        protected void OnApplied()
        {
            Applied?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Event Handlers

        void this_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedFilter == null)
            {
                SelectedFilter = AnyTextFilterSettings.Current.Filters.FirstOrDefault();
            }
            Keyboard.Focus(txtInput);
        }

        void FilterRunner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "PreviewDocument") return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var prevDoc = FilterRunner.PreviewDocument.ToFlowDocument(txtPreview.FontSize);
                prevDoc.Loaded += (sender_, e_) => ResetCurrentLine();
                txtPreview.Document = prevDoc;
            }));
        }

        void txtInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                if (FilterRunner == null) return;
                FilterRunner.ApplyLastResult();
                OnApplied();
            }
        }

        void txtInput_TextChanged(object sender, RoutedEventArgs e)
        {
            txtInputHint.Visibility = string.IsNullOrEmpty(txtInput.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextPreviousDifference(toPrev: false);
        }

        void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextPreviousDifference(toPrev: true);
        }

        void btnHistoryBack_Click(object sender, RoutedEventArgs e)
        {
            if (FilterRunner == null || !AnyTextFilterSettings.Current.History.Any()) return;
            if (HistoryIndex == -1)
            {
                HistoryIndex = AnyTextFilterSettings.Current.History.Count - 1;
            }
            else
            {
                --HistoryIndex;
            }
            if (HistoryIndex != -1)
            {
                var history = AnyTextFilterSettings.Current.History[HistoryIndex];
                SelectedFilter = AnyTextFilterSettings.Current.Filters.FirstOrDefault(f => f.ID == history.FilterID);
                FilterRunner.UserInputText = history.UserInputText;
            } else
            {
                FilterRunner.UserInputText = "";
            }
        }

        void btnHistoryFront_Click(object sender, RoutedEventArgs e)
        {
            if (FilterRunner == null || !AnyTextFilterSettings.Current.History.Any()) return;
            if (HistoryIndex == AnyTextFilterSettings.Current.History.Count - 1)
            {
                HistoryIndex = -1;
            }
            else
            {
                ++HistoryIndex;
            }
            if (HistoryIndex != -1)
            {
                var history = AnyTextFilterSettings.Current.History[HistoryIndex];
                SelectedFilter = AnyTextFilterSettings.Current.Filters.FirstOrDefault(f => f.ID == history.FilterID);
                FilterRunner.UserInputText = history.UserInputText;
            } else
            {
                FilterRunner.UserInputText = "";
            }
        }

        void btnApply_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (FilterRunner == null) return;
            FilterRunner.ApplyLastResult();
            OnApplied();
        }

        #endregion

        #region Overrides

        MyTextEdit textEdit;
        Dictionary<Key, Action> keyToAction;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (AnyTextFilterSettings.Current.UsesEmacsLikeKeybindings && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (textEdit == null)
                {
                    textEdit = new MyTextEdit(() => txtInput.Text, (t) => txtInput.Text = t, () => txtInput.CaretIndex, (i) => txtInput.CaretIndex = i)
                    {
                        GetTextFromClipboard = Clipboard.GetText,
                        SetTextFromClipboard = (text) => Clipboard.SetText(text)
                    };
                    keyToAction = new Dictionary<Key, Action>
                    {
                        { Key.F, textEdit.ForwardChar },
                        { Key.B, textEdit.BackwardChar },
                        { Key.A, textEdit.MoveBeginningOfLine },
                        { Key.E, textEdit.MoveEndOfLine },
                        { Key.N, textEdit.NextLine },
                        { Key.P, textEdit.PreviousLine },
                        { Key.D, textEdit.DeleteChar },
                        { Key.H, textEdit.DeleteBackwardChar },
                        { Key.K, textEdit.KillLine },
                        { Key.Y, textEdit.Yank },
                   };
                }
                Action action;
                if (keyToAction.TryGetValue(e.Key, out action))
                {
                    action();
                    e.Handled = true;
                }
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            if (FilterRunner == null || SelectedFilter == null) return;
            FilterRunner.Start(SelectedFilter);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (FilterRunner == null) return;
            FilterRunner.Stop();
        }

        #endregion

        #region Font Setting

        public void SetFont(string fontName, int fontSizePt)
        {
            var fontSizeConverter = new FontSizeConverter();
            double fontSizePx = (double)fontSizeConverter.ConvertFromString(string.Format("{0}pt", fontSizePt));

            txtInput.FontFamily = new FontFamily(fontName);
            txtInput.FontSize = fontSizePx;
            txtInputHint.FontFamily = new FontFamily(fontName);
            txtInputHint.FontSize = fontSizePx;
            txtPreview.FontFamily = new FontFamily(fontName);
            txtPreview.FontSize = fontSizePx;
        }

        #endregion

        #region Current Line Update

        Paragraph currPara;

        void ResetCurrentLine()
        {
            currPara = txtPreview.Document.Blocks.OfType<Paragraph>().FirstOrDefault();
            if (currPara == null) return;
            if (FilterRunner.ShowsDifference)
            {
                MoveToNextPreviousDifference(toPrev: false);
            }
            else
            {
                currPara.BringIntoView();
            }
        }

        void MoveToNextPreviousDifference(bool toPrev)
        {
            var currTag = currPara.Tag as UserInputPreviewDocument.LineTag;
            if (currTag == null) return;

            Func<Paragraph, bool> hasDifference = (para) =>
            {
                var tag = para.Tag as UserInputPreviewDocument.LineTag;
                if (tag == null) return false;
                if (tag.IsPartOfLastNewLines) return false;
                return tag.HasDifference;
            };
            var paraQuery = toPrev
                ? txtPreview.Document.Blocks.OfType<Paragraph>().Where(hasDifference).Reverse()
                : txtPreview.Document.Blocks.OfType<Paragraph>().Where(hasDifference);
            Paragraph destPara = null;
            foreach (var para in paraQuery)
            {
                var tag = para.Tag as UserInputPreviewDocument.LineTag;
                if (toPrev)
                {
                    if (tag.LineIndex >= currTag.LineIndex) continue;
                }
                else
                {
                    if (tag.LineIndex <= currTag.LineIndex) continue;
                }
                destPara = para;
                break;
            }
            if (destPara == null)
            {
                destPara = paraQuery.FirstOrDefault(hasDifference);
            }

            if (destPara != null)
            {
                new TextRange(currPara.ElementStart, currPara.ElementEnd).ApplyPropertyValue(TextElement.BackgroundProperty, null);
                currPara = destPara;
                currPara.BringIntoView();
                new TextRange(currPara.ElementStart, currPara.ElementEnd).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
                OnMoveToNextPreviousDifferenceDone();
            }
        }

        void OnMoveToNextPreviousDifferenceDone()
        {
            if (currPara == null) return;
            var currTag = currPara.Tag as UserInputPreviewDocument.LineTag;
            if (currTag == null) return;
            if (FilterRunner == null) return;
            FilterRunner.EnsureMainViewLineVisible(currTag.LineIndex);
        }

        #endregion
    }
}
