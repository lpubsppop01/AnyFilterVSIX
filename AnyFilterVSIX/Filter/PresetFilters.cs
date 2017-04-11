using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace lpubsppop01.AnyFilterVSIX
{
    public enum PresetFilterID
    {
        Empty, MonoCSharpScript, CygwinBash, CygwinSed
    }

    public class PresetFilters
    {
        public static Filter Get(PresetFilterID presetID)
        {
            var filter = new Filter();
            switch (presetID)
            {
                default:
                case PresetFilterID.Empty:
                    filter.Title = "Empty";
                    break;
                case PresetFilterID.MonoCSharpScript:
                    filter.Title = "Mono C# Script";
                    filter.Command = "cmd";
                    filter.Arguments = string.Format(@"/c ""C:\Program Files (x86)\Mono\bin\csharp.bat"" {0}", FilterRunner.VariableName_InputTempFilePath);
                    filter.TargetForNoSelection = TargetForNoSelection.CurrentLine;
                    filter.InsertsAfterCurrentLine = true;
                    break;
                case PresetFilterID.CygwinBash:
                    filter.Title = "Cygwin bash";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    filter.Arguments = string.Format(@"-lc ""$(cygpath -u '{0}')""", FilterRunner.VariableName_InputTempFilePath);
                    filter.InputNewLineKind = MyNewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TargetForNoSelection = TargetForNoSelection.WholeDocument;
                    filter.TemplateFilePath = Path.Combine(GetTemplateDirectoryPath(), "CygwinBashTemplate.txt");
                    filter.UsesTemplateFile = true;
                    break;
                case PresetFilterID.CygwinSed:
                    filter.Title = "Cygwin sed";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    // unescaped: -lc "sed -f \"$(cygpath -u '$(UserInputTempFilePath)')\""
                    filter.Arguments = string.Format(@"-lc ""sed -f \""$(cygpath -u '{0}')\""""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = MyNewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TargetForNoSelection = TargetForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
            }
            return filter;
        }

        static string GetTemplateDirectoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AnyFilterVSIX");
        }

        public static bool TryCreateTemplateFile(PresetFilterID presetID, string filePath, string encodingName, MyNewLineKind newLineKind)
        {
            try
            {
                string dirPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                switch (presetID)
                {
                    case PresetFilterID.CygwinBash:
                        using (var writer = new StreamWriter(filePath, /* append */ false, MyEncoding.GetEncoding(encodingName))
                        {
                            NewLine = newLineKind.ToNewLineString()
                        })
                        {
                            writer.WriteLine(@"INPUT_TEXT=$(cat << 'EOS'");
                            writer.WriteLine(@"$(InputText)");
                            writer.WriteLine(@"EOS");
                            writer.WriteLine(@")");
                            writer.WriteLine(@"echo ""$INPUT_TEXT"" | $(UserInput)");
                        }
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
