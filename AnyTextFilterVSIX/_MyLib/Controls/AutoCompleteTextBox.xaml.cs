using System;
using System.Collections.Generic;
using System.IO;
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

        #endregion

        #region Event

        public event TextChangedEventHandler TextChanged;

        protected void OnTextChanged(TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        #endregion

        #region Event Handlers

        void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string word = WordPicker.GetWord(TextBox.Text, TextBox.CaretIndex);
            int iWordStart = TextBox.Text.LastIndexOf(word, TextBox.CaretIndex);
            lstWords.ItemsSource = Dictionary.GetWords(word);
            popup.IsOpen = (lstWords.ItemsSource as IEnumerable<string>).Any();
            if (iWordStart >= 0)
            {
                var rect = TextBox.GetRectFromCharacterIndex(iWordStart);
                popup.HorizontalOffset = rect.X;
            }
            OnTextChanged(e);
        }

        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox.TextChanged -= TextBox_TextChanged;
            try
            {
                if (e.Key == Key.Escape)
                {
                    popup.IsOpen = false;
                    TextBox.Focus();
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    if (!lstWords.IsFocused)
                    {
                        lstWords.Focus();
                        lstWords.SelectedIndex = 0;
                    }
                    else
                    {
                        var words = lstWords.ItemsSource as IEnumerable<string>;
                        lstWords.SelectedIndex = (lstWords.SelectedIndex < words.Count() - 1) ? lstWords.SelectedIndex + 1 : 0;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    if (!lstWords.IsFocused)
                    {
                        lstWords.Focus();
                        var words = lstWords.ItemsSource as IEnumerable<string>;
                        lstWords.SelectedIndex = words.Count() - 1;
                    }
                    else
                    {
                        var words = lstWords.ItemsSource as IEnumerable<string>;
                        lstWords.SelectedIndex = (lstWords.SelectedIndex > 0) ? lstWords.SelectedIndex - 1 : words.Count() - 1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Tab)
                {
                    if (!lstWords.IsFocused)
                    {
                        lstWords.Focus();
                        lstWords.SelectedIndex = 0;
                        e.Handled = true;
                    }
                }
            }
            finally
            {
                TextBox.TextChanged += TextBox_TextChanged;
            }
        }

        void lstWords_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox.TextChanged -= TextBox_TextChanged;
            try
            {
                if (e.Key == Key.Escape)
                {
                    popup.IsOpen = false;
                    TextBox.Focus();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    string word = WordPicker.GetWord(TextBox.Text, TextBox.CaretIndex);
                    int iWordStart = TextBox.Text.LastIndexOf(word, TextBox.CaretIndex);
                    string selectedWord = (string)lstWords.SelectedValue;
                    TextBox.Text = TextBox.Text.Substring(0, iWordStart) + selectedWord + TextBox.Text.Substring(iWordStart + word.Length);
                    popup.IsOpen = false;
                    TextBox.Focus();
                    TextBox.CaretIndex = iWordStart + selectedWord.Length;
                    e.Handled = true;
                }
            }
            finally
            {
                TextBox.TextChanged += TextBox_TextChanged;
            }
        }

        #endregion
    }
}
