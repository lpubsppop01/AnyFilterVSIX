using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    sealed class UserInputBuffer : INotifyPropertyChanged
    {
        #region Constructor

        public UserInputBuffer()
        {
            UserInputText = "";
        }

        #endregion

        #region Properties

        string userInputText;
        public string UserInputText
        {
            get { return userInputText; }
            set { userInputText = value; OnPropertyChanged(); }
        }

        UserInputPreviewDocument previewDocument;
        public UserInputPreviewDocument PreviewDocument
        {
            get { return previewDocument; }
            set { previewDocument = value; OnPropertyChanged(); }
        }

        bool showsDifference;
        public bool ShowsDifference
        {
            get { return showsDifference; }
            set { showsDifference = value; OnPropertyChanged(); }
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
}
