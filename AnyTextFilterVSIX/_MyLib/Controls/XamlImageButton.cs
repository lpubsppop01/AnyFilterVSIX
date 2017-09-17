using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class XamlImageButton : Button
    {
        #region Constructor

        public XamlImageButton()
        {
            var viewbox = new Viewbox();
            Content = viewbox;
            var frame = new Frame();
            viewbox.Child = frame;
            frame.SetBinding(Frame.SourceProperty, new Binding("ImageSource") { Source = this });
        }

        #endregion

        #region Properties

        public Uri ImageSource
        {
            get { return (Uri)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof(Uri), typeof(XamlImageButton), new PropertyMetadata(null));

        #endregion
    }
}
