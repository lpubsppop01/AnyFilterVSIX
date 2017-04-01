using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace lpubsppop01.AnyFilterVSIX
{
    class EnumToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Enum)) return value;
            if (!(targetType == typeof(string))) return value;

            return Enum.GetName(value.GetType(), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string)) return value;
            if (!targetType.IsEnum) return value;

            string valueString = value as string;
            return Enum.Parse(targetType, valueString);
        }

        #endregion
    }
}
