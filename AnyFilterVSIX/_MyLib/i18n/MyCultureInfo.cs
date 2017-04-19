using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    sealed class MyCultureInfo
    {
        #region Static Members

        public static readonly MyCultureInfo Auto = new MyCultureInfo("");
        public static readonly MyCultureInfo en_US = new MyCultureInfo("en-US");
        public static readonly MyCultureInfo ja_JP = new MyCultureInfo("ja-JP");

        public static MyCultureInfo GetCultureInfo(string name)
        {
            if (name == "en-US") return en_US;
            else if (name == "ja-JP") return ja_JP;
            return Auto;
        }

        #endregion

        #region Constructor

        MyCultureInfo(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name { get; private set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return Culture.EnglishName + " [current thread]";
                return Culture.EnglishName;
            }
        }

        CultureInfo culture;
        public CultureInfo Culture
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return CultureInfo.CurrentUICulture;
                if (culture == null)
                {
                    culture = CultureInfo.GetCultureInfo(Name);
                }
                return culture;
            }
        }

        #endregion
    }
}
