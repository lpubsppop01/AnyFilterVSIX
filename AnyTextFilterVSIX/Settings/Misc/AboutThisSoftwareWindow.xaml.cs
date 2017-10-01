using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace lpubsppop01.AnyTextFilterVSIX
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

            var htmlStr = GetReadmeHTMLString();
            ctrlMain.NavigateToString(htmlStr);
        }

        static string GetReadmeHTMLString()
        {
            string readmeText = "";
            var currAssem = Assembly.GetExecutingAssembly();
            string dirPath = Path.GetDirectoryName(currAssem.Location);
            string readmeFilePath = Path.Combine(dirPath, "README.md");
            if (File.Exists(readmeFilePath))
            {
                using (var reader = new StreamReader(readmeFilePath))
                {
                    readmeText = reader.ReadToEnd();
                }
            }
            else
            {
                readmeText = "README.md does not exists.";
            }

            var htmlBodyStr = Markdig.Markdown.ToHtml(readmeText);
            return string.Format(HtmlFormatString, htmlBodyStr.Replace("<a ", "<a target=\"_blank\" "));
        }

        const string HtmlFormatString = @"
<!DOCTYPE html>
<head>
<style type=""text/css"">
<!--
body {{ font-family: sans-serif; }}
-->
</style>
</head>
<body>
{0}
</body>
";

        #endregion

        #region Event Handlers

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
