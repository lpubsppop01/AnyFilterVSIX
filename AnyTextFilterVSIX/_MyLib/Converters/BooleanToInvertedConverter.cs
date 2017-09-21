using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class BooleanToInvertedConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return value;
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return value;
            return !(bool)value;
        }

        #endregion
    }

    class BooleanToInvertedBinding : Binding
    {
        #region Constructor

        public BooleanToInvertedBinding(string path)
            : base(path)
        {
            Converter = new BooleanToInvertedConverter();
        }

        #endregion
    }
}
