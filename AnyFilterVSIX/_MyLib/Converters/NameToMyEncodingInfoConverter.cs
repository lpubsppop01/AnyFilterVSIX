using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace lpubsppop01.AnyFilterVSIX
{
    class NameToMyEncodingInfoConverter : IValueConverter
    {
        #region Constructor

        MyEncodingInfo[] encodings;

        public NameToMyEncodingInfoConverter(MyEncodingInfo[] encodings)
        {
            this.encodings = encodings;
        }

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string)) return value;

            string valueString = value as string;
            return encodings.FirstOrDefault(e => e.Name == valueString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is MyEncodingInfo)) return value;

            var valueEncoding = value as Encoding;
            return valueEncoding.WebName;
        }

        #endregion
    }
}
