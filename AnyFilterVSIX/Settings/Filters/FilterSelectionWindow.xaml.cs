using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for FilterSelectionWindow.xaml
    /// </summary>
    partial class FilterSelectionWindow : Window
    {
        #region Constructor

        public FilterSelectionWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Item Class

        sealed class Item : INotifyPropertyChanged
        {
            #region Constructor

            public Item(Filter filter)
            {
                Filter = filter;
            }

            #endregion

            #region Properties

            public Filter Filter { get; private set; }

            bool isChecked;
            public bool IsChecked
            {
                get { return isChecked; }
                set { isChecked = value; OnPropertyChanged(); }
            }

            public string DisplayTitle
            {
                get { return string.Format("{0:00}. {1}", Filter.Number, Filter.Title); }
            }

            #endregion

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion
        }

        #endregion

        #region Properties

        public IList<Filter> Filters
        {
            get
            {
                var items = lstMaster.ItemsSource as IList<Item>;
                if (items == null) return null;
                return items.Select(i => i.Filter).ToArray();
            }
            set
            {
                var items = new ObservableCollection<Item>();
                foreach (var filter in value)
                {
                    items.Add(new Item(filter) { IsChecked = true });
                }
                lstMaster.ItemsSource = items;
            }
        }

        public IList<Filter> CheckedFilters
        {
            get
            {
                var items = lstMaster.ItemsSource as IList<Item>;
                if (items == null) return null;
                return items.Where(i => i.IsChecked).Select(i => i.Filter).ToArray();
            }
        }

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
