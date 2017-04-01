using System;
using System.Collections.Generic;
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

namespace lpubsppop01.AnyFilterVSIX
{
    /// <summary>
    /// Interaction logic for UserInputWindow.xaml
    /// </summary>
    public partial class UserInputWindow : Window
    {
        #region Constructor

        public UserInputWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        void this_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(txtInput);
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Properties

        public bool UsesEmacsLikeKeybindings
        {
            get { return (bool)GetValue(UsesEmacsLikeKeybindingsProperty); }
            set { SetValue(UsesEmacsLikeKeybindingsProperty, value); }
        }

        public static readonly DependencyProperty UsesEmacsLikeKeybindingsProperty = DependencyProperty.Register(
            "UsesEmacsLikeKeybindings", typeof(bool), typeof(UserInputWindow), new PropertyMetadata(false));

        #endregion

        #region Font Setting

        public void SetFont(string fontName, int fontSizePt)
        {
            var fontSizeConverter = new FontSizeConverter();
            double fontSizePx = (double)fontSizeConverter.ConvertFromString(string.Format("{0}pt", fontSizePt));

            txtInput.FontFamily = new System.Windows.Media.FontFamily(fontName);
            txtInput.FontSize = fontSizePx;
            txtPreview.FontFamily = new System.Windows.Media.FontFamily(fontName);
            txtPreview.FontSize = fontSizePx;
        }

        #endregion

        #region Overrides

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (UsesEmacsLikeKeybindings && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.F)
                {
                    if (txtInput.CaretIndex < txtInput.Text.Length)
                    {
                        txtInput.CaretIndex = txtInput.CaretIndex + 1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.B)
                {
                    if (txtInput.CaretIndex > 0)
                    {
                        txtInput.CaretIndex = txtInput.CaretIndex - 1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.A)
                {
                    if (txtInput.CaretIndex > 0)
                    {
                        txtInput.CaretIndex = 0;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.E)
                {
                    if (txtInput.CaretIndex < txtInput.Text.Length)
                    {
                        txtInput.CaretIndex = txtInput.Text.Length;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.D)
                {
                    if (txtInput.CaretIndex < txtInput.Text.Length)
                    {
                        int backupCaretIndex = txtInput.CaretIndex;
                        txtInput.Text = txtInput.Text.Remove(txtInput.CaretIndex, 1);
                        txtInput.CaretIndex = backupCaretIndex;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.H)
                {
                    if (txtInput.CaretIndex > 0)
                    {
                        int backupCaretIndex = txtInput.CaretIndex;
                        txtInput.Text = txtInput.Text.Remove(txtInput.CaretIndex - 1, 1);
                        txtInput.CaretIndex = backupCaretIndex - 1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.K)
                {
                    if (txtInput.CaretIndex < txtInput.Text.Length)
                    {
                        int backupCaretIndex = txtInput.CaretIndex;
                        txtInput.Text = txtInput.Text.Substring(0, txtInput.CaretIndex);
                        txtInput.CaretIndex = backupCaretIndex;
                    }
                    e.Handled = true;
                }
            }
            base.OnPreviewKeyDown(e);
        }

        #endregion
    }
}
