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

namespace lpubsppop01.AnyFilterVSIX
{
    sealed class NameToResourceBindingSource<T> : INotifyPropertyChanged
    {
        #region Constructor

        public NameToResourceBindingSource(T resources)
        {
            Resources = resources;
        }

        #endregion

        #region Properties

        public T Resources { get; private set; }

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

        public void OnCultureChanged()
        {
            OnPropertyChanged("Resources");
        }

        #endregion
    }

    class NameToResourceBinding : Binding
    {
        #region Static Members

        static NameToResourceBindingSource<Properties.Resources> _source = new NameToResourceBindingSource<Properties.Resources>(new Properties.Resources());

        public static CultureInfo Culture
        {
            get { return Properties.Resources.Culture; }
            set { Properties.Resources.Culture = value; _source.OnCultureChanged(); }
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
