using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for FilterListEditControl.xaml
    /// </summary>
    partial class FilterListEditControl : UserControl
    {
        #region Constructor

        public FilterListEditControl()
        {
            InitializeComponent();

            var encodings = MyEncoding.GetEncodings();
            cmbInputEncoding.ItemsSource = encodings;
            cmbInputEncoding.DisplayMemberPath = "DisplayName";
            cmbInputEncoding.SetBinding(ComboBox.SelectedValueProperty, new Binding("SelectedValue.InputEncodingName") {
                ElementName = "lstMaster", Converter = new NameToMyEncodingInfoConverter(encodings)
            });
            cmbInputNewLineKind.ItemsSource = Enum.GetValues(typeof(NewLineKind));
            cmbInputNewLineKind.SetBinding(ComboBox.SelectedValueProperty, new Binding("SelectedValue.InputNewLineKind") {
                ElementName = "lstMaster", Converter = new EnumToStringConverter()
            });
            cmbOutputEncoding.ItemsSource = encodings;
            cmbOutputEncoding.DisplayMemberPath = "DisplayName";
            cmbOutputEncoding.SetBinding(ComboBox.SelectedValueProperty, new Binding("SelectedValue.OutputEncodingName") {
                ElementName = "lstMaster", Converter = new NameToMyEncodingInfoConverter(encodings)
            });
        }

        #endregion

        #region Properties

        public ObservableCollection<Filter> ItemsSource
        {
            get { return lstMaster.ItemsSource as ObservableCollection<Filter>; }
            set
            {
                lstMaster.ItemsSource = value;
                if (value == null) return;
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    lstMaster.SelectedValue = value.FirstOrDefault();
                    HasSelection = (lstMaster.SelectedIndex != -1);
                    HasItems = ItemsSource.Any();
                }));
            }
        }

        public bool HasSelection
        {
            get { return (bool)GetValue(HasSelectionProperty); }
            set { SetValue(HasSelectionProperty, value); }
        }

        public static readonly DependencyProperty HasSelectionProperty = DependencyProperty.Register(
            "HasSelection", typeof(bool), typeof(FilterListEditControl), new PropertyMetadata(false));

        public bool HasItems
        {
            get { return (bool)GetValue(HasItemsProperty); }
            set { SetValue(HasItemsProperty, value); }
        }

        public static readonly DependencyProperty HasItemsProperty = DependencyProperty.Register(
            "HasItems", typeof(bool), typeof(FilterListEditControl), new PropertyMetadata(false));

        #endregion

        #region Event Handlers

        FilterListEdit edit;

        void this_Loaded(object sender, RoutedEventArgs e)
        {
            edit = new FilterListEdit(() => ItemsSource, () => lstMaster.SelectedIndex, i => lstMaster.SelectedIndex = i, Window.GetWindow(this));
        }

        void this_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var settings = DataContext as AnyTextFilterSettings;
            ItemsSource = (settings != null) ? settings.Filters : null;
        }

        void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!edit.Add()) return;
            HasSelection = (lstMaster.SelectedIndex != -1);
            HasItems = ItemsSource.Any();
        }

        void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (!edit.Remove()) return;
            HasSelection = (lstMaster.SelectedIndex != -1);
            HasItems = ItemsSource.Any();
        }

        void btnExport_Click(object sender, RoutedEventArgs e)
        {
            edit.Export();
        }

        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (!edit.Import()) return;
            HasSelection = (lstMaster.SelectedIndex != -1);
            HasItems = ItemsSource.Any();
        }

        void btnCommand_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = "*.*";
            if (dialog.ShowDialog() ?? false)
            {
                txtCommand.Text = dialog.FileName;
            }
        }

        void btnTemplate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = "*.*";
            if (dialog.ShowDialog() ?? false)
            {
                txtTemplate.Text = dialog.FileName;
            }
        }

        #endregion

        #region Template File Creation

        //static bool ConfirmToCreateTemplateFile(Filter filter)
        //{
        //    var messageBuf = new StringBuilder();
        //    if (File.Exists(filter.TemplateFilePath))
        //    {
        //        messageBuf.AppendLine("The following file will be overwritten:");
        //    }
        //    else
        //    {
        //        messageBuf.AppendLine("A new template file will be created at the following location:");
        //    }
        //    messageBuf.AppendLine(filter.TemplateFilePath);
        //    messageBuf.Append("Continue?");
        //    return MessageBox.Show(messageBuf.ToString(), "Add Filter", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        //}

        //static bool TryCreateTemplateFile(PresetFilterID presetID, Filter filter)
        //{
        //    if (!PresetFilters.TryCreateTemplateFile(presetID, filter.TemplateFilePath, filter.InputEncodingName, filter.InputNewLineKind))
        //    {
        //        string message = "File creation failed:" + Environment.NewLine + filter.TemplateFilePath;
        //        MessageBox.Show(string.Format(message), "Failed");
        //        return false;
        //    }
        //    return true;
        //}

        #endregion

        #region Popup Location Update

        IEnumerable<Popup> Popups
        {
            get
            {
                yield return popupArgumentsHint;
                yield return popupTemplateFilePathHint;
            }
        }

        public void OnWindowLocationChanged()
        {
            // ref. http://stackoverflow.com/questions/1600218/how-can-i-move-a-wpf-popup-when-its-anchor-element-moves
            foreach (var popup in Popups.Where(p => p.IsOpen))
            {
                double offset = popup.HorizontalOffset;
                popup.HorizontalOffset = offset + 1;
                popup.HorizontalOffset = offset;
            }
        }

        #endregion
    }
}
