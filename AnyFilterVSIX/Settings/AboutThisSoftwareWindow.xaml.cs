using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace lpubsppop01.AnyFilterVSIX
{
    /// <summary>
    /// Interaction logic for AboutThisSoftwareWindow.xaml
    /// </summary>
    public partial class AboutThisSoftwareWindow : Window
    {
        #region Constructor

        public AboutThisSoftwareWindow()
        {
            InitializeComponent();

            var currAssem = Assembly.GetExecutingAssembly();
            string dirPath = Path.GetDirectoryName(currAssem.Location);
            string readmeFilePath = Path.Combine(dirPath, "README.md");
            if (File.Exists(readmeFilePath))
            {
                using (var reader = new StreamReader(readmeFilePath))
                {
                    txtMain.Text = reader.ReadToEnd();
                }
            }
            else
            {
                txtMain.Text = "README.md does not exists.";
            }
        }

        #endregion

        #region Event Handlers

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
