using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for AnyFilterSettingsWindow.xaml
    /// </summary>
    public partial class AnyFilterSettingsWindow : Window
    {
        #region Constructor

        public AnyFilterSettingsWindow()
        {
            InitializeComponent();

            cmbCulture.ItemsSource = new[] { MyCultureInfo.Auto, MyCultureInfo.en_US, MyCultureInfo.ja_JP };
            cmbCulture.DisplayMemberPath = "DisplayName";
            cmbCulture.SetBinding(ComboBox.SelectedValueProperty, new Binding("Culture"));
        }

        #endregion

        #region Event Handlers

        void this_LocationChanged(object sender, EventArgs e)
        {
            ctrlFilters.OnWindowLocationChanged();
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
    }
}
