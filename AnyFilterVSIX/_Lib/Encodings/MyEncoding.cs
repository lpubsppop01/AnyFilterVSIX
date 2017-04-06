using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lpubsppop01.AnyFilterVSIX
{
    static class MyEncoding
    {
        public static Encoding GetEncoding(string myEncodingName)
        {
            if (myEncodingName == MyEncodingInfo.UTF8_WithoutBOM.Name)
            {
                return MyEncodingInfo.UTF8_WithoutBOM.GetEncoding();
            }
            return Encoding.GetEncoding(myEncodingName);
        }

        public static MyEncodingInfo[] GetEncodings()
        {
            var systemEncodings = Encoding.GetEncodings().Select(e => new MyEncodingInfo(e)).ToArray();
            var myEncodings = new[] { MyEncodingInfo.UTF8_WithoutBOM };
            return systemEncodings.Concat(myEncodings).ToArray();
        }
    }

    sealed class MyEncodingInfo
    {
        #region Constructors

        EncodingInfo encodingInfo;
        string myEncodingName;

        public MyEncodingInfo(EncodingInfo encodingInfo)
        {
            this.encodingInfo = encodingInfo;
        }

        public MyEncodingInfo(string myEncodingName)
        {
            this.myEncodingName = myEncodingName;
        }

        #endregion

        #region Constants

        public static readonly MyEncodingInfo UTF8_WithoutBOM = new MyEncodingInfo(UTF8_WithoutBOM_Name);
        const string UTF8_WithoutBOM_Name = "utf-8-without-bom";
        const string UTF8_WrithoutBOM_DisplayName = "Unicode (UTF-8 without BOM)";

        #endregion

        #region Properties

        public string DisplayName
        {
            get
            {
                if (encodingInfo != null)
                {
                    return encodingInfo.DisplayName;
                }
                else if (myEncodingName == UTF8_WithoutBOM_Name)
                {
                    return UTF8_WrithoutBOM_DisplayName;
                }
                return "";
            }
        }

        public string Name
        {
            get
            {
                if (encodingInfo != null)
                {
                    return encodingInfo.Name;
                }
                return myEncodingName;
            }
        }

        public Encoding GetEncoding()
        {
            if (encodingInfo != null)
            {
                return encodingInfo.GetEncoding();
            }
            else if (myEncodingName == UTF8_WithoutBOM_Name)
            {
                return new UTF8Encoding(false);
            }
            return null;
        }

        #endregion
    }
}
