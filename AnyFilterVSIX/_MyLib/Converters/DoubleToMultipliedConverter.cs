using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace lpubsppop01.AnyTextFilterVSIX
{
    // ref. http://stackoverflow.com/questions/717299/wpf-setting-the-width-and-height-as-a-percentage-value

    class DoubleToMultipliedConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value) / System.Convert.ToDouble(parameter);
            }
            catch
            {
                return value;
            }
        }

        #endregion
    }

    class DoubleToHalfBinding : Binding
    {
        #region Constructor

        public DoubleToHalfBinding(string path)
            : base(path)
        {
            Converter = new DoubleToMultipliedConverter();
            ConverterParameter = 0.5;
        }

        #endregion
    }
}
