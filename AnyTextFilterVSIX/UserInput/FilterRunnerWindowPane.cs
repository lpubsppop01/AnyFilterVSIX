using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace lpubsppop01.AnyTextFilterVSIX
{
    [Guid("C901AA6B-2B92-44C7-98F5-1E7089EAC5E0")]
    public class FilterRunnerWindowPane : ToolWindowPane
    {
        #region Constructor

        public FilterRunnerWindowPane()
        {
            Caption = Properties.Resources.AnyTextFilter;
            Content = new FilterRunnerControl();
        }

        #endregion

        #region Properties

        internal new FilterRunnerControl Content
        {
            get { return base.Content as FilterRunnerControl; }
            set { base.Content = value; }
        }

        internal new IVsWindowFrame Frame
        {
            get { return base.Frame as IVsWindowFrame; }
            set { base.Frame = value; }
        }

        #endregion
    }
}
