using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyFilterVSIX
{
    class RepeatedAsyncTaskSupport
    {
        object myLock = new object();
        bool isRunning;
        bool rerunsAfterCurr;

        public bool TryBegin()
        {
            lock (myLock)
            {
                if (isRunning)
                {
                    rerunsAfterCurr = true;
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
    }
}
