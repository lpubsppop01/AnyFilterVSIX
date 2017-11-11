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
            var filter = new Filter { ID = Guid.NewGuid() };
            switch (presetID)
            {
                default:
                case PresetFilterID.Empty:
                    filter.Title = "Empty";
                    break;
                case PresetFilterID.Sed:
                    filter.Title = "sed";
                    filter.Command = Path.Combine(GetProgramDirectoryPath(), @"sed-4.2.1-bin\bin\sed.exe");
                    filter.Arguments = string.Format(@"-f ""{0}""", FilterProcess.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.TempFileExtension = ".sed";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    filter.Memo = @"I use the following sed-4.2+alpha (Japanese site):
http://ac206223.ppp.asahi-net.or.jp/adiary/memo/adiary.cgi/hirosugu/GNU%20sed%20%E3%82%92Windows%E3%81%A7%E4%BD%BF%E3%81%86";
                    filter.UserInputWindow_ShowsDifference = true;
                    filter.UserInputWindow_UsesAutoComplete = true;
                    break;
                case PresetFilterID.Awk:
                    filter.Title = "AWK";
                    filter.Command = Path.Combine(GetProgramDirectoryPath(), @"gawk4\gawk.exe");
                    filter.Arguments = string.Format(@"-f ""{0}""", FilterProcess.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".awk";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    filter.Memo = @"I use the following gawk-4.1.2:
http://www.klabaster.com/freeware.htm";
                    filter.UserInputWindow_UsesAutoComplete = true;
                    break;
                case PresetFilterID.MonoCSharpScript:
                    filter.Title = "Mono C# Script";
                    filter.Command = "cmd";
                    filter.Arguments = string.Format(@"/c ""C:\Program Files (x86)\Mono\bin\csharp.bat"" {0}", FilterProcess.VariableName_InputTempFilePath);
                    filter.TempFileExtension = ".cs";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.CurrentLine;
                    filter.InsertsAfterTargetSpan = true;
                    filter.UserInputWindow_UsesAutoComplete = true;
                    break;
                case PresetFilterID.CygwinBash:
                    filter.Title = "Cygwin bash";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    filter.Arguments = string.Format(@"-lc ""$(cygpath -u '{0}')""", FilterProcess.VariableName_InputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".bash";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.TemplateFilePath = Path.Combine(GetTemplateDirectoryPath(), "CygwinBash.txt");
                    filter.UsesTemplateFile = true;
                    break;
                case PresetFilterID.CygwinSed:
                    filter.Title = "Cygwin sed";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    // unescaped: -lc "sed -f \"$(cygpath -u '$(UserInputTempFilePath)')\""
                    filter.Arguments = string.Format(@"-lc ""sed -f \""$(cygpath -u '{0}')\""""", FilterProcess.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".sed";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    filter.UserInputWindow_ShowsDifference = true;
                    filter.UserInputWindow_UsesAutoComplete = true;
                    break;
                case PresetFilterID.CygwinGawk:
                    filter.Title = "Cygwin Gawk";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    // unescaped: -lc "awk -f \"$(cygpath -u '$(UserInputTempFilePath)')\""
                    filter.Arguments = string.Format(@"-lc ""awk -f \""$(cygpath -u '{0}')\""""", FilterProcess.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TempFileExtension = ".awk";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    filter.UserInputWindow_UsesAutoComplete = true;
                    break;
                case PresetFilterID.CMigemo:
                    filter.Title = "C/Migemo";
                    filter.Command = "powershell";
                    filter.Arguments = "-f $(InputTempFilePath)";
                    filter.TempFileExtension = ".ps1";
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.TemplateFilePath = Path.Combine(GetTemplateDirectoryPath(), "CMigemo.txt");
                    filter.UsesTemplateFile = true;
                    filter.UserInputWindow_ShowsDifference = true;
                    break;
            }
            return filter;
        }

        static string GetProgramDirectoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"AnyTextFilterVSIX\Programs");
        }

        static string GetTemplateDirectoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"AnyTextFilterVSIX\Templates");
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
                            writer.WriteLine(@"$migemo = Join-Path $myDocuments ""AnyTextFilterVSIX\Programs\cmigemo-default-win64\cmigemo.exe""");
                            writer.WriteLine(@"$migemoDict = Join-Path $myDocuments ""AnyTextFilterVSIX\Programs\cmigemo-default-win64\dict\cp932\migemo-dict""");
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
