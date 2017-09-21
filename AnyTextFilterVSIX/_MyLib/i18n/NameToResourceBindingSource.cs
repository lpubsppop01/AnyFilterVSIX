using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace lpubsppop01.AnyTextFilterVSIX
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnCultureChanged()
        {
            OnPropertyChanged("Resources");
        }

        #endregion
    }

}
