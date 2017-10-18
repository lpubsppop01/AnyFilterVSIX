using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class FilterHistoryItem : ICloneable, INotifyPropertyChanged
    {
        #region Constructors

        public FilterHistoryItem()
        {
            Clear();
        }

        protected FilterHistoryItem(FilterHistoryItem src)
        {
            Copy(src);
        }

        public void Clear()
        {
            FilterID = Guid.Empty;
        }

        public void Copy(FilterHistoryItem src)
        {
            FilterID = src.FilterID;
            UserInputText = src.UserInputText;
        }

        #endregion

        #region Properties

        Guid m_ID;
        public Guid FilterID
        {
            get { return m_ID; }
            set { m_ID = value; OnPropertyChanged(); }
        }

        string m_UserInputText;
        public string UserInputText
        {
            get { return m_UserInputText; }
            set { m_UserInputText = value; OnPropertyChanged(); }
        }

        bool m_IsPinned;
        public bool IsPinned
        {
            get { return m_IsPinned; }
            set { m_IsPinned = value; OnPropertyChanged(); }
        }

        #endregion

        #region Serialization

        public void Save(ISettingsStoreAdapter settingsStore, string collectionPath)
        {
            settingsStore.SetGuid(collectionPath, "FilterID", FilterID);
            settingsStore.SetString(collectionPath, "UserInputText", UserInputText);
            settingsStore.SetBoolean(collectionPath, "IsPinned", IsPinned);
        }

        public static FilterHistoryItem Load(ISettingsStoreAdapter settingsStore, string collectionPath)
        {
            return new FilterHistoryItem
            {
                FilterID = settingsStore.GetGuid(collectionPath, "FilterID", Guid.Empty),
                UserInputText = settingsStore.GetString(collectionPath, "UserInputText", ""),
                IsPinned = settingsStore.GetBoolean(collectionPath, "IsPinned", false)
            };
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        public FilterHistoryItem Clone()
        {
            return new FilterHistoryItem(this);
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
