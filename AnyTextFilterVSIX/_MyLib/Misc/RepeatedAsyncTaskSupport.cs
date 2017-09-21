using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    sealed class RepeatedAsyncTaskSupport : INotifyPropertyChanged
    {
        #region Properties

        bool m_IsRunning;
        public bool IsRunning
        {
            get { return m_IsRunning; }
            set { m_IsRunning = value; OnPropertyChanged(); }
        }

        #endregion

        #region Events

        public event EventHandler CancelRequested;

        void OnCancelRequested()
        {
            if (CancelRequested != null)
            {
                CancelRequested(this, new EventArgs());
            }
        }

        #endregion

        #region Actions

        object myLock = new object();
        bool rerunsAfterCurr;

        public bool TryStart()
        {
            lock (myLock)
            {
                if (IsRunning)
                {
                    if (!rerunsAfterCurr)
                    {
                        rerunsAfterCurr = true;
                        OnCancelRequested();
                    }
                    return false;
                }
                IsRunning = true;
                rerunsAfterCurr = false;
            }
            return true;
        }

        public bool CheckRepeat()
        {
            lock (myLock)
            {
                if (rerunsAfterCurr)
                {
                    rerunsAfterCurr = false;
                    return true;
                }
            }
            return false;
        }

        public void Stop()
        {
            lock (myLock)
            {
                IsRunning = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
