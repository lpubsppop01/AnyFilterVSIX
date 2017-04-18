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

        #region Properties

        public bool UsesEmacsLikeKeybindings
        {
            get { return (bool)GetValue(UsesEmacsLikeKeybindingsProperty); }
            set { SetValue(UsesEmacsLikeKeybindingsProperty, value); }
        }

        public static readonly DependencyProperty UsesEmacsLikeKeybindingsProperty = DependencyProperty.Register(
            "UsesEmacsLikeKeybindings", typeof(bool), typeof(UserInputWindow), new PropertyMetadata(false));

        #endregion

        #region Events

        public class MoveToNextPreviousDoneDifferenceEventArgs : EventArgs
        {
            #region Constructor

            public MoveToNextPreviousDoneDifferenceEventArgs(int lineIndex)
            {
                LineIndex = lineIndex;
            }

            #endregion

            #region Properties

            public int LineIndex { get; private set; }

            #endregion
        }

        public event EventHandler<MoveToNextPreviousDoneDifferenceEventArgs> MoveToNextPreviousDifferenceDone;

        protected void OnMoveToNextPreviousDifferenceDone()
        {
            if (MoveToNextPreviousDifferenceDone != null)
            {
                if (currPara == null) return;
                var currTag = currPara.Tag as UserInputPreviewDocument.LineTag;
                if (currTag == null) return;
                MoveToNextPreviousDifferenceDone(this, new MoveToNextPreviousDoneDifferenceEventArgs(currTag.LineIndex));
            }
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
                var prevDoc = buffer.PreviewDocument.ToFlowDocument(txtPreview.FontSize);
                prevDoc.Loaded += (sender_, e_) => ResetCurrentLine();
                txtPreview.Document = prevDoc;
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

        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextPreviousDifference(toPrev: false);
        }

        void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextPreviousDifference(toPrev: true);
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

        #region Overrides

        MyTextEdit textEdit;
        Dictionary<Key, Action> keyToAction;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (UsesEmacsLikeKeybindings && Keyboard.Modifiers == ModifierKeys.Control)
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

        #endregion

        #region Position Setting

        public void SetPosition(FrameworkElement ctrlTextView)
        {
            var mainWindow = Window.GetWindow(ctrlTextView);
            Height = (mainWindow != null) ? mainWindow.ActualHeight / 4 : 300;
            Width = ctrlTextView.ActualWidth;
            var viewTopLeftOnScreen = ctrlTextView.PointToScreen(new Point());
            Left = viewTopLeftOnScreen.X;
            Top = (viewTopLeftOnScreen.Y < ctrlTextView.ActualHeight)
                ? viewTopLeftOnScreen.Y + ctrlTextView.ActualHeight - Height
                : viewTopLeftOnScreen.Y - Height;
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
            if (buffer.ShowsDifference)
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

        #endregion
    }
}
