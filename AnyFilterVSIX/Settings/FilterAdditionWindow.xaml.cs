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
    /// Interaction logic for FilterAdditionWindow.xaml
    /// </summary>
    public partial class FilterAdditionWindow : Window
    {
        #region Constructor

        public FilterAdditionWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public PresetFilterID SelectedID
        {
            get { return (PresetFilterID)GetValue(SelectedIDProperty); }
            set { SetValue(SelectedIDProperty, value); }
        }

        public static readonly DependencyProperty SelectedIDProperty = DependencyProperty.Register(
            "SelectedID", typeof(PresetFilterID), typeof(FilterAdditionWindow), new PropertyMetadata(PresetFilterID.Empty));

        #endregion

        #region Event Handlers

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
    }
}
