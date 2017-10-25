using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

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

            AnyTextFilterSettings.Current.Loaded += AnyTextFilterSettings_Current_Loaded;

            cmbFilter.ItemsSource = AnyTextFilterSettings.Current.Filters;
            cmbFilter.DisplayMemberPath = "Title";
            cmbFilter.SetBinding(ComboBox.SelectedValueProperty, new Binding("SelectedFilter")
            {
                Source = this
            });
            chkUsesEmacsKeyBindings.SetBinding(CheckBox.IsCheckedProperty, new Binding("UsesEmacsLikeKeybindings")
            {
                Source = AnyTextFilterSettings.Current
            });
            chkUsesEmacsKeyBindings.Checked += chkUsesEmacsKeyBindings_Checked;

            txtInput.TextBox.SetBinding(TextBox.TextProperty, new Binding("UserInputText") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            txtInput.TextBox.SetBinding(TextBox.MaxHeightProperty, new DoubleToHalfBinding("ActualHeight") { Source = pnlMain });
            txtInput.TextBox.AcceptsReturn = true;
            txtInput.TextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            txtInput.TextBox.VerticalAlignment = VerticalAlignment.Top;
            txtInput.TextBox.PreviewKeyDown += txtInput_PreviewKeyDown;
            txtInput.TextBox.TextChanged += txtInput_TextChanged;
            txtInput.TextBox.GotKeyboardFocus += txtInput_GotKeyboardFocus;
        }

        #endregion

        #region Properties

        public FilterRunner FilterRunner
        {
            get { return DataContext as FilterRunner; }
            set
            {
                var oldValue = FilterRunner;
                if (oldValue != null)
                {
                    oldValue.PropertyChanged -= FilterRunner_PropertyChanged;
                    oldValue.HistoryManager.PropertyChanged -= HistoryManager_PropertyChanged;
                }
                DataContext = value;
                var newValue = FilterRunner;
                if (newValue != null)
                {
                    UpdateAutoCompleteDictionary();
                    newValue.PropertyChanged += FilterRunner_PropertyChanged;
                    newValue.HistoryManager.PropertyChanged += HistoryManager_PropertyChanged;
                }
            }
        }

        public Filter SelectedFilter
        {
            get { return (Filter)GetValue(SelectedFilterProperty); }
            set { SetValue(SelectedFilterProperty, value); }
        }

        public static readonly DependencyProperty SelectedFilterProperty = DependencyProperty.Register(
            "SelectedFilter", typeof(Filter), typeof(FilterRunnerControl), new PropertyMetadata(null));

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
            Keyboard.Focus(txtInput.TextBox);
        }

        void AnyTextFilterSettings_Current_Loaded(object sender, EventArgs e)
        {
            cmbFilter.ItemsSource = AnyTextFilterSettings.Current.Filters;
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
                if (FilterRunner == null || FilterRunner.IsRunning) return;
                FilterRunner.ApplyLastResult();
                OnApplied();
            }
        }

        void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtInputHint.Visibility = string.IsNullOrEmpty(txtInput.TextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        void txtInput_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (FilterRunner == null) return;
            UpdateAutoCompleteDictionary();
        }

        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextPreviousDifference(toPrev: false);
        }

        void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextPreviousDifference(toPrev: true);
        }

        void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (FilterRunner == null) return;

            // Prepare list items
            var listItems = AnyTextFilterSettings.Current.History.Select((h, i) =>
            {
                var filter = AnyTextFilterSettings.Current.Filters.FirstOrDefault(f => f.ID == h.FilterID);
                return new FilterHistoryListWindowItem
                {
                    FilterTitle = (filter != null) ? filter.Title : "Removed",
                    UserInputText = h.UserInputText,
                    IsPinned = h.IsPinned,
                    SourceIndex = i
                };
            }).Reverse().ToArray();
            var selectedValue = FilterRunner.HistoryManager.CurrentIndex < listItems.Length ? listItems[FilterRunner.HistoryManager.CurrentIndex] : null;

            // Show dialog
            var dialog = new FilterHistoryListWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ItemsSource = new ObservableCollection<FilterHistoryListWindowItem>(listItems),
                SelectedValue = selectedValue
            };
            if ((dialog.ShowDialog() ?? false) && dialog.SelectedValue != null)
            {
                FilterRunner.HistoryManager.CurrentIndex = dialog.SelectedValue.SourceIndex;
            }

            // Save pinned states
            bool modified = false;
            foreach (var listItem in listItems)
            {
                var historyItem = AnyTextFilterSettings.Current.History[listItem.SourceIndex];
                if (historyItem.IsPinned == listItem.IsPinned) continue;
                historyItem.IsPinned = listItem.IsPinned;
                modified = true;
            }
            if (modified)
            {
                AnyTextFilterSettings.SaveCurrentHistory();
            }
        }

        void btnHistoryBack_Click(object sender, RoutedEventArgs e)
        {
            if (FilterRunner == null || !AnyTextFilterSettings.Current.History.Any()) return;
            FilterRunner.Stop();
            FilterRunner.HistoryManager.DecrementIndex();
            if (SelectedFilter == null) return;
            FilterRunner.Start(SelectedFilter);
        }

        void btnHistoryFront_Click(object sender, RoutedEventArgs e)
        {
            if (FilterRunner == null || !AnyTextFilterSettings.Current.History.Any()) return;
            FilterRunner.Stop();
            FilterRunner.HistoryManager.IncrementIndex();
            if (SelectedFilter == null) return;
            FilterRunner.Start(SelectedFilter);
        }

        void HistoryManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentItem") return;
            if (FilterRunner == null) return;

            var currHistoryItem = FilterRunner.HistoryManager.CurrentItem;
            if (currHistoryItem == null)
            {
                MessageBox.Show("historyManager.CurrentItem is null.", "Error");
                return;
            }
            BeginEdit();
            try
            {
                if (currHistoryItem.FilterID != Guid.Empty)
                {
                    SelectedFilter = AnyTextFilterSettings.Current.Filters.FirstOrDefault(f => f.ID == currHistoryItem.FilterID);
                    FilterRunner.UserInputText = currHistoryItem.UserInputText;
                }
                else
                {
                    if (!FilterRunner.HistoryManager.CurrentItemIsDummy)
                    {
                        MessageBox.Show("currHistoryItem.FilterID is empty." + Environment.NewLine + "Update of settings may repair this.", "Error");
                    }
                    FilterRunner.UserInputText = "";
                }
            }
            finally
            {
                EndEdit(quiet: true);
            }
        }

        void chkUsesEmacsKeyBindings_Checked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Remove comflict key bindings?", Properties.Resources.EmacsLikeKeyBindings, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                string errorMessage;
                if (!MyTextEdit.TryRemoveConflictKeyBindings(dte, out errorMessage))
                {
                    MessageBox.Show(errorMessage, Properties.Resources.EmacsLikeKeyBindings, MessageBoxButton.OK);
                }
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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
       {
            if (txtInput.Popup.IsOpen)
            {
                if (txtInput.TryHandleKeyEvent(e))
                {
                    e.Handled = true;
                    return;
                }
            }
            if (AnyTextFilterSettings.Current.UsesEmacsLikeKeybindings && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (textEdit == null)
                {
                    textEdit = new MyTextEdit(() => txtInput.TextBox.Text, (t) => txtInput.TextBox.Text = t,
                        () => txtInput.TextBox.CaretIndex, (i) => txtInput.TextBox.CaretIndex = i);
                }
                if (textEdit.TryHandleKeyEvent(e))
                {
                    e.Handled = true;
                    return;
                }
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (m_IsEditing)
            {
                m_PropertyChangedEventArgsList.Add(e);
                return;
            }
            if (FilterRunner == null) return;
            if (e.Property == SelectedFilterProperty)
            {
                FilterRunner.Stop();
                if (SelectedFilter == null) return;
                FilterRunner.Start(SelectedFilter);
            }
        }

        #endregion

        #region Begin/EndEdit

        bool m_IsEditing;
        List<DependencyPropertyChangedEventArgs> m_PropertyChangedEventArgsList;

        public bool BeginEdit()
        {
            if (m_IsEditing) return false;
            m_IsEditing = true;
            m_PropertyChangedEventArgsList = new List<DependencyPropertyChangedEventArgs>();
            return true;
        }

        public void EndEdit(bool quiet)
        {
            m_IsEditing = false;
            if (!quiet)
            {
                foreach (var e in m_PropertyChangedEventArgsList)
                {
                    OnPropertyChanged(e);
                }
            }
            m_PropertyChangedEventArgsList = null;
        }

        #endregion

        #region Font Setting

        public void SetFont(string fontName, int fontSizePt)
        {
            var fontSizeConverter = new FontSizeConverter();
            double fontSizePx = (double)fontSizeConverter.ConvertFromString(string.Format("{0}pt", fontSizePt));

            txtInput.TextBox.FontFamily = new FontFamily(fontName);
            txtInput.TextBox.FontSize = fontSizePx;
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
            if (currPara == null) return;
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

        #region Auto Complete Dictionary Update

        void UpdateAutoCompleteDictionary()
        {
            txtInput.Dictionary = new NGramDictionary(2);
            txtInput.Dictionary.AddDocument(Guid.NewGuid(), "", FilterRunner.ViewText);
        }

        #endregion
    }
}
