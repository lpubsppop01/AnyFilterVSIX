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

namespace lpubsppop01.AnyFilterVSIX
{
    /// <summary>
    /// Interaction logic for UserInputWindow.xaml
    /// </summary>
    partial class UserInputWindow : Window
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

        UserInputBuffer buffer;

        void this_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (buffer != null)
            {
                buffer.PropertyChanged -= buffer_PropertyChanged;
                buffer = null;
            }
            buffer = DataContext as UserInputBuffer;
            if (buffer != null)
            {
                buffer.PropertyChanged += buffer_PropertyChanged;
            }
        }

        void buffer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "PreviewDocument") return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtPreview.Document = buffer.PreviewDocument.ToFlowDocument();
            }));
        }

        void txtInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                DialogResult = true;
                Close();
            }
        }

        void txtInput_TextChanged(object sender, RoutedEventArgs e)
        {
            txtInputHint.Visibility = string.IsNullOrEmpty(txtInput.Text) ? Visibility.Visible : Visibility.Hidden;
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

            txtInput.FontFamily = new FontFamily(fontName);
            txtInput.FontSize = fontSizePx;
            txtInputHint.FontFamily = new FontFamily(fontName);
            txtInputHint.FontSize = fontSizePx;
            txtPreview.FontFamily = new FontFamily(fontName);
            txtPreview.FontSize = fontSizePx;
        }

        #endregion

        #region Overrides

        MyTextEdit textEdit;
        Dictionary<Key, Action> keyToAction;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (UsesEmacsLikeKeybindings && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (textEdit == null)
                {
                    textEdit = new MyTextEdit(() => txtInput.Text, (t) => txtInput.Text = t, () => txtInput.CaretIndex, (i) => txtInput.CaretIndex = i);
                    keyToAction = new Dictionary<Key, Action>
                    {
                        { Key.F, textEdit.ForwardChar },
                        { Key.B, textEdit.BackwardChar },
                        { Key.A, textEdit.MoveBiginningOfLine },
                        { Key.E, textEdit.MoveEndOfLine },
                        { Key.N, textEdit.NextLine },
                        { Key.P, textEdit.PreviousLine },
                        { Key.D, textEdit.DeleteChar },
                        { Key.H, textEdit.DeleteBackwardChar },
                        { Key.K, textEdit.KillLine },
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

        #endregion
    }
}
