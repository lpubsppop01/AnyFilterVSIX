using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class AutoCompletePopup : Popup
    {
        #region Constructor

        public AutoCompletePopup()
        {
            Height = 100;
            AllowsTransparency = true;
            Placement = PlacementMode.Custom;
            StaysOpen = true;
            CustomPopupPlacementCallback = this_CustomPopupPlacement;
            WordListBox.BorderThickness = new Thickness(1);
            WordListBox.IsSynchronizedWithCurrentItem = true;
            WordListBox.PreviewKeyDown += WordListBox_PreviewKeyDown;
        }

        #endregion

        #region Properties

        public NGramDictionary Dictionary { get; set; }
        public bool HandlesPreviewKeyDownEvent { get; set; }

        protected ListBox WordListBox
        {
            get
            {
                if (!(Child is ListBox)) Child = new ListBox();
                return Child as ListBox;
            }
        }

        TextBox m_TargetTextBox;
        public TextBox TargetTextBox
        {
            get { return m_TargetTextBox; }
            set
            {
                if (m_TargetTextBox != null)
                {
                    PlacementTarget = null;
                    BindingOperations.ClearBinding(this, MaxWidthProperty);
                    m_TargetTextBox.TextChanged -= TargetTextBox_TextChanged;
                    m_TargetTextBox.PreviewKeyDown -= TargetTextBox_PreviewKeyDown;
                }
                m_TargetTextBox = value;
                if (m_TargetTextBox != null)
                {
                    PlacementTarget = m_TargetTextBox;
                    SetBinding(MaxWidthProperty, new Binding("ActualWidth") { Source = m_TargetTextBox });
                    m_TargetTextBox.TextChanged += TargetTextBox_TextChanged;
                    m_TargetTextBox.PreviewKeyDown += TargetTextBox_PreviewKeyDown;
                }
            }
        }

        #endregion

        #region Event Handlers and Callbacks

        void TargetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Dictionary == null) return;
            string word = WordPicker.GetWord(TargetTextBox.Text, TargetTextBox.CaretIndex);
            int iWordStart = TargetTextBox.Text.LastIndexOf(word, TargetTextBox.CaretIndex);
            WordListBox.ItemsSource = Dictionary.GetWords(word);
            IsOpen = (WordListBox.ItemsSource as IEnumerable<string>).Any();
            if (iWordStart >= 0)
            {
                var rect = TargetTextBox.GetRectFromCharacterIndex(iWordStart);
                HorizontalOffset = rect.X;
            }
        }

        void TargetTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!HandlesPreviewKeyDownEvent) return;
            e.Handled = TryHandleKeyEvent(e);
        }

        void WordListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!HandlesPreviewKeyDownEvent) return;
            e.Handled = TryHandleKeyEvent(e);
        }

        CustomPopupPlacement[] this_CustomPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Vertical) };
        }

        #endregion

        #region TryHandleKeyEvent

        public bool TryHandleKeyEvent(KeyEventArgs e)
        {
            if (TargetTextBox == null) return false;
            TargetTextBox.TextChanged -= TargetTextBox_TextChanged;
            try
            {
                if (TargetTextBox.IsKeyboardFocusWithin || WordListBox.IsKeyboardFocusWithin)
                {
                    if (e.Key == Key.Down || (e.Key == Key.N && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
                    {
                        Down();
                        return true;
                    }
                    else if (e.Key == Key.Up || (e.Key == Key.P && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
                    {
                        Up();
                        return true;
                    }
                    else if (e.Key == Key.Escape || (e.Key == Key.G && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
                    {
                        Escape();
                        return true;
                    }
                    else if (e.Key == Key.Enter || e.Key == Key.Tab)
                    {
                        Enter();
                        return true;
                    }
                }
            }
            finally
            {
                TargetTextBox.TextChanged += TargetTextBox_TextChanged;
            }
            return false;
        }

        void Down()
        {
            if (!WordListBox.IsKeyboardFocused) WordListBox.Focus();
            var words = WordListBox.ItemsSource as IEnumerable<string>;
            WordListBox.SelectedIndex = (WordListBox.SelectedIndex < words.Count() - 1) ? WordListBox.SelectedIndex + 1 : 0;
            WordListBox.ScrollIntoView(WordListBox.SelectedItem);
        }

        void Up()
        {
            if (!WordListBox.IsKeyboardFocused) WordListBox.Focus();
            var words = WordListBox.ItemsSource as IEnumerable<string>;
            WordListBox.SelectedIndex = (WordListBox.SelectedIndex > 0) ? WordListBox.SelectedIndex - 1 : words.Count() - 1;
            WordListBox.ScrollIntoView(WordListBox.SelectedItem);
        }

        void Escape()
        {
            IsOpen = false;
            TargetTextBox.Focus();
        }

        void Enter()
        {
            string word = WordPicker.GetWord(TargetTextBox.Text, TargetTextBox.CaretIndex);
            int iWordStart = TargetTextBox.Text.LastIndexOf(word, TargetTextBox.CaretIndex);
            string selectedWord = (string)WordListBox.SelectedValue;
            TargetTextBox.Text = TargetTextBox.Text.Substring(0, iWordStart) + selectedWord + TargetTextBox.Text.Substring(iWordStart + word.Length);
            IsOpen = false;
            TargetTextBox.Focus();
            TargetTextBox.CaretIndex = iWordStart + selectedWord.Length;
        }

        #endregion
    }
}
