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

namespace lpubsppop01.AnyTextFilterVSIX
{
    /// <summary>
    /// Interaction logic for FilterHistoryListWindow.xaml
    /// </summary>
    partial class FilterHistoryListWindow : Window
    {
        #region Constructor

        public FilterHistoryListWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public ObservableCollection<FilterHistoryListWindowItem> ItemsSource
        {
            get { return lstHistory.ItemsSource as ObservableCollection<FilterHistoryListWindowItem>; }
            set { lstHistory.ItemsSource = value; }
        }

        public FilterHistoryListWindowItem SelectedValue
        {
            get { return lstHistory.SelectedValue as FilterHistoryListWindowItem; }
            set { lstHistory.SelectedValue = value; }
        }

        #endregion

        #region Event Handlers

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void lstHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion
    }

    class FilterHistoryListWindowItem : INotifyPropertyChanged
    {
        #region Properties

        string m_FilterTitle;
        public string FilterTitle
        {
            get { return m_FilterTitle; }
            set { m_FilterTitle = value; OnPropertyChanged(); }
        }

        string m_UserInputText;
        public string UserInputText
        {
            get { return m_UserInputText; }
            set { m_UserInputText = value; OnPropertyChanged(); }
        }

        int m_SourceIndex;
        public int SourceIndex
        {
            get { return m_SourceIndex; }
            set { m_SourceIndex = value; OnPropertyChanged(); }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
