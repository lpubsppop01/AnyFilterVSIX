using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class NameToResourceBinding : Binding
    {
        #region Static Members

        static NameToResourceBindingSource<Properties.Resources> _source = new NameToResourceBindingSource<Properties.Resources>(new Properties.Resources());

        public static MyCultureInfo Culture
        {
            set
            {
                Properties.Resources.Culture = value.Culture;
                _source.OnCultureChanged();
            }
        }

        #endregion

        #region Constructor

        public NameToResourceBinding(string path)
            : base("Resources." + path)
        {
            Source = _source;
        }

        #endregion
    }
}
