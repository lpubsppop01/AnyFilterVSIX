using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class FilterHistoryManager : INotifyPropertyChanged
    {
        #region Constructor

        public FilterHistoryManager()
        {
            LoadHistory();
        }

        #endregion

        #region Properties

        int m_CurrentIndex;
        public int CurrentIndex
        {
            get { return m_CurrentIndex; }
            set { m_CurrentIndex = value; OnPropertyChanged(); OnPropertyChanged("CurrentItem"); }
        }

        public FilterHistoryItem CurrentItem
        {
            get
            {
                if (m_historyCopy == null || CurrentIndex < 0 || CurrentIndex >= m_historyCopy.Length) return null;
                return m_historyCopy[CurrentIndex];
            }
            set
            {
                if (m_historyCopy == null) return;
                int index = Array.IndexOf(m_historyCopy, value);
                if (index == -1) return;
                CurrentIndex = index;
            }
        }

        public bool CurrentItemIsDummy
        {
            get { return CurrentIndex >= AnyTextFilterSettings.Current.History.Count; }
        }

        #endregion

        #region Methods

        FilterHistoryItem[] m_historyCopy;

        void LoadHistory()
        {
            AnyTextFilterSettings.LoadCurrentHistory();
            m_historyCopy = AnyTextFilterSettings.Current.History.Select(h => h.Clone())
                .Concat(new[] { new FilterHistoryItem { UserInputText = "" } }).ToArray();
            CurrentIndex = m_historyCopy.Length - 1;
        }

        public void AddHistoryItem(FilterHistoryItem newItem)
        {
            if (!AnyTextFilterSettings.Current.BeginEdit()) return;
            try
            {
                AnyTextFilterSettings.LoadCurrentHistory();
                if (AddHistoryItemCore(newItem))
                {
                    AnyTextFilterSettings.SaveCurrentHistory();
                }
            }
            finally
            {
                AnyTextFilterSettings.Current.EndEdit();
            }
            LoadHistory();
        }

        bool AddHistoryItemCore(FilterHistoryItem newItem)
        {
            // Do nothing if matdhed to last history item 
            if (AnyTextFilterSettings.Current.History.Any() && AnyTextFilterSettings.Current.History.Last().Equals(newItem)) return false;

            // Move to last if matched to pinned history item
            var pinnedCopyItem = newItem.Clone();
            pinnedCopyItem.IsPinned = true;
            var pinnedMatchedItem = AnyTextFilterSettings.Current.History.FirstOrDefault(h => h.Equals(pinnedCopyItem));
            if (pinnedMatchedItem != null)
            {
                AnyTextFilterSettings.Current.History.Remove(pinnedMatchedItem);
                AnyTextFilterSettings.Current.History.Add(pinnedMatchedItem);
                return true;
            }

            // Add to last
            AnyTextFilterSettings.Current.History.Add(newItem);
            while (AnyTextFilterSettings.Current.History.Count(h => !h.IsPinned) > AnyTextFilterSettings.HistoryUnpinnedCountMax)
            {
                var iFirstUnpinned = AnyTextFilterSettings.Current.History.Select((v, i) => new { v, i }).First(i => !i.v.IsPinned).i;
                AnyTextFilterSettings.Current.History.RemoveAt(iFirstUnpinned);
            }
            return true;
        }

        public void IncrementIndex()
        {
            if (m_historyCopy.Length < 2) return;
            if (CurrentIndex == m_historyCopy.Length - 2)
            {
                LoadHistory();
            }
            else if (CurrentIndex == m_historyCopy.Length - 1)
            {
                CurrentIndex = 0;
            }
            else
            {
                ++CurrentIndex;
            }
        }

        public void DecrementIndex()
        {
            if (m_historyCopy.Length < 2) return;
            if (CurrentIndex == 0)
            {
                LoadHistory();
            }
            else
            {
                --CurrentIndex;
            }
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
