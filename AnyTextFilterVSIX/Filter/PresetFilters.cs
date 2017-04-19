using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace lpubsppop01.AnyTextFilterVSIX
{
    enum PresetFilterID
    {
        Empty, Sed, Awk, MonoCSharpScript, CygwinBash, CygwinSed, CygwinGawk, CMigemo
    }

    class PresetFilters
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
                case PresetFilterID.Sed:
                    filter.Title = "sed";
                    filter.Command = "sed.exe";
                    filter.Arguments = string.Format(@"-f ""{0}""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".sed";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
                case PresetFilterID.Awk:
                    filter.Title = "AWK";
                    filter.Command = "awk.exe";
                    filter.Arguments = string.Format(@"-f ""{0}""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".awk";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
                case PresetFilterID.MonoCSharpScript:
                    filter.Title = "Mono C# Script";
                    filter.Command = "cmd";
                    filter.Arguments = string.Format(@"/c ""C:\Program Files (x86)\Mono\bin\csharp.bat"" {0}", FilterRunner.VariableName_InputTempFilePath);
                    filter.TempFileExtension = ".cs";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.CurrentLine;
                    filter.InsertsAfterTargetSpan = true;
                    break;
                case PresetFilterID.CygwinBash:
                    filter.Title = "Cygwin bash";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    filter.Arguments = string.Format(@"-lc ""$(cygpath -u '{0}')""", FilterRunner.VariableName_InputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".bash";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.TemplateFilePath = Path.Combine(GetTemplateDirectoryPath(), "CygwinBashTemplate.txt");
                    filter.UsesTemplateFile = true;
                    break;
                case PresetFilterID.CygwinSed:
                    filter.Title = "Cygwin sed";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    // unescaped: -lc "sed -f \"$(cygpath -u '$(UserInputTempFilePath)')\""
                    filter.Arguments = string.Format(@"-lc ""sed -f \""$(cygpath -u '{0}')\""""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".sed";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
                case PresetFilterID.CygwinGawk:
                    filter.Title = "Cygwin Gawk";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    // unescaped: -lc "awk -f \"$(cygpath -u '$(UserInputTempFilePath)')\""
                    filter.Arguments = string.Format(@"-lc ""awk -f \""$(cygpath -u '{0}')\""""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".awk";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
                case PresetFilterID.CMigemo:
                    filter.Title = "C/Migemo";
                    filter.Command = "powershell";
                    filter.Arguments = "-f $(InputTempFilePath)";
                    filter.TempFileExtension = ".ps1";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.TemplateFilePath = Path.Combine(GetTemplateDirectoryPath(), "CMigemoTemplate.txt");
                    filter.UsesTemplateFile = true;
                    break;
            }
            return filter;
        }

        static string GetTemplateDirectoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AnyTextFilterVSIX");
        }

        public static bool TryCreateTemplateFile(PresetFilterID presetID, string filePath, string encodingName, NewLineKind newLineKind)
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
                    case PresetFilterID.CMigemo:
                        using (var writer = new StreamWriter(filePath, /* append */ false, MyEncoding.GetEncoding(encodingName))
                        {
                            NewLine = newLineKind.ToNewLineString()
                        })
                        {
                            writer.WriteLine(@"$userInputText = '$(UserInput)'");
                            writer.WriteLine(@"$inputText = @'");
                            writer.WriteLine(@"$(InputText)");
                            writer.WriteLine(@"'@");
                            writer.WriteLine(@"");
                            writer.WriteLine(@"if ($userInputText -eq """") {");
                            writer.WriteLine(@"    echo $inputText");
                            writer.WriteLine(@"    exit");
                            writer.WriteLine(@"}");
                            writer.WriteLine(@"");
                            writer.WriteLine(@"$myDocuments = [Environment]::GetFolderPath('MyDocuments')");
                            writer.WriteLine(@"$migemo = Join-Path $myDocuments ""AnyTextFilterVSIX\cmigemo-default-win64\cmigemo.exe""");
                            writer.WriteLine(@"$migemoDict = Join-Path $myDocuments ""AnyTextFilterVSIX\cmigemo-default-win64\dict\cp932\migemo-dict""");
                            writer.WriteLine(@"");
                            writer.WriteLine(@"$pattern = (echo $userInputText | &$migemo -d $migemoDict -q)");
                            writer.WriteLine(@"$outputText = $inputText -replace $pattern, ""<$&>""");
                            writer.WriteLine(@"echo $outputText");
                        }
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
