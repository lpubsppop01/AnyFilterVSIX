using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lpubsppop01.AnyTextFilterVSIX
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextBox.xaml
    /// </summary>
    public partial class AutoCompleteTextBox : UserControl
    {
        #region Constructor

        public AutoCompleteTextBox()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public NGramDictionary Dictionary { get; set; }
        public bool HandlesPreviewKeyDownEvent { get; set; }

        #endregion

        #region Popup Placement

        CustomPopupPlacement[] Popup_CustomPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Vertical) };
        }

        #endregion

        #region Event Handlers

        void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Dictionary == null) return;
            string word = WordPicker.GetWord(TextBox.Text, TextBox.CaretIndex);
            int iWordStart = TextBox.Text.LastIndexOf(word, TextBox.CaretIndex);
            lstWords.ItemsSource = Dictionary.GetWords(word);
            Popup.IsOpen = (lstWords.ItemsSource as IEnumerable<string>).Any();
            if (iWordStart >= 0)
            {
                var rect = TextBox.GetRectFromCharacterIndex(iWordStart);
                Popup.HorizontalOffset = rect.X;
            }
        }

        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!HandlesPreviewKeyDownEvent) return;
            e.Handled = TryHandleKeyEvent(e);
        }

        void lstWords_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!HandlesPreviewKeyDownEvent) return;
            e.Handled = TryHandleKeyEvent(e);
        }

        #endregion

        #region TryHandleKeyEvent

        public bool TryHandleKeyEvent(KeyEventArgs e)
        {
            TextBox.TextChanged -= TextBox_TextChanged;
            try
            {
                if (TextBox.IsKeyboardFocused)
                {
                    if (e.Key == Key.Escape || (e.Key == Key.G && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
                    {
                        Popup.IsOpen = false;
                        TextBox.Focus();
                        return true;
                    }
                    else if (e.Key == Key.Down || (e.Key == Key.N && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
                    {
                        lstWords.Focus();
                        lstWords.SelectedIndex = 0;
                        lstWords.ScrollIntoView(lstWords.SelectedItem);
                        return true;
                    }
                    else if (e.Key == Key.Up || (e.Key == Key.P && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
                    {
                        lstWords.Focus();
                        var words = lstWords.ItemsSource as IEnumerable<string>;
                        lstWords.SelectedIndex = words.Count() - 1;
                        lstWords.ScrollIntoView(lstWords.SelectedItem);
                        return true;
                    }
                    else if (e.Key == Key.Tab)
                    {
                        lstWords.Focus();
                        lstWords.SelectedIndex = 0;
                        return true;
                    }
                }
                else if (lstWords.IsKeyboardFocused)
                {
                    if (e.Key == Key.Escape || (e.Key == Key.G && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
                    {
                        Popup.IsOpen = false;
                        TextBox.Focus();
                        return true;
                    }
                    else if (e.Key == Key.Enter || e.Key == Key.Tab)
                    {
                        string word = WordPicker.GetWord(TextBox.Text, TextBox.CaretIndex);
                        int iWordStart = TextBox.Text.LastIndexOf(word, TextBox.CaretIndex);
                        string selectedWord = (string)lstWords.SelectedValue;
                        TextBox.Text = TextBox.Text.Substring(0, iWordStart) + selectedWord + TextBox.Text.Substring(iWordStart + word.Length);
                        Popup.IsOpen = false;
                        TextBox.Focus();
                        TextBox.CaretIndex = iWordStart + selectedWord.Length;
                        return true;
                    }
                    else if (e.Key == Key.N && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        var words = lstWords.ItemsSource as IEnumerable<string>;
                        lstWords.SelectedIndex = (lstWords.SelectedIndex < words.Count() - 1) ? lstWords.SelectedIndex + 1 : 0;
                        lstWords.ScrollIntoView(lstWords.SelectedItem);
                        return true;
                    }
                    else if (e.Key == Key.P && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        var words = lstWords.ItemsSource as IEnumerable<string>;
                        lstWords.SelectedIndex = (lstWords.SelectedIndex > 0) ? lstWords.SelectedIndex - 1 : words.Count() - 1;
                        lstWords.ScrollIntoView(lstWords.SelectedItem);
                        return true;
                    }
                }
            }
            finally
            {
                TextBox.TextChanged += TextBox_TextChanged;
            }
            return false;
        }

        #endregion
    }
}
