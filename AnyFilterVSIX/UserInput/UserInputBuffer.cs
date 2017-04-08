using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyFilterVSIX
{
    public class UserInputBuffer : ICloneable, INotifyPropertyChanged
    {
        #region Constructors

        public UserInputBuffer()
        {
            UserInputText = "";
        }

        protected UserInputBuffer(UserInputBuffer src)
        {
            UserInputText = src.UserInputText;
            PreviewText = src.PreviewText;
            ShowsDifference = src.ShowsDifference;
            InputText = src.InputText;
        }

        #endregion

        #region Properties

        string userInputText;
        public string UserInputText
        {
            get { return userInputText; }
            set { userInputText = value; OnPropertyChanged(); }
        }

        string previewText;
        public string PreviewText
        {
            get { return previewText; }
            set { previewText = value; OnPropertyChanged(); }
        }

        bool showsDifference;
        public bool ShowsDifference
        {
            get { return showsDifference; }
            set { showsDifference = value; OnPropertyChanged(); }
        }

        string inputText;
        public string InputText
        {
            get { return inputText; }
            set { inputText = value; OnPropertyChanged(); }
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        public UserInputBuffer Clone()
        {
            return new UserInputBuffer(this);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
