using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    sealed class RepeatedAsyncTaskSupport
    {
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
        bool isRunning;
        bool rerunsAfterCurr;

        public bool TryBegin()
        {
            lock (myLock)
            {
                if (isRunning)
                {
                    if (!rerunsAfterCurr)
                    {
                        rerunsAfterCurr = true;
                        OnCancelRequested();
                    }
                    return false;
                }
                isRunning = true;
                rerunsAfterCurr = false;
            }
            return true;
        }

        public bool TryContinue()
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

        public void End()
        {
            lock (myLock)
            {
                isRunning = false;
            }
        }

        #endregion
    }
}
