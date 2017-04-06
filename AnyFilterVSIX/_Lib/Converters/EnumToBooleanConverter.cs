using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace lpubsppop01.AnyFilterVSIX
{
    class EnumToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Enum)) return value;
            if (!(parameter is string)) return value;

            string parameterString = parameter as string;
            string valueString = Enum.GetName(value.GetType(), value);
            return parameterString == valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return value;
            if (!(parameter is string)) return value;
            if (!targetType.IsEnum) return value;

            string parameterString = parameter as string;
            bool valueBool = (bool)value;
            return valueBool ? Enum.Parse(targetType, parameterString) : Activator.CreateInstance(targetType);
        }

        #endregion
    }

    class EnumToBooleanBinding : Binding
    {
        #region Constructor

        public EnumToBooleanBinding(string path)
            : base(path)
        {
            Converter = new EnumToBooleanConverter();
        }

        #endregion
    }
}
