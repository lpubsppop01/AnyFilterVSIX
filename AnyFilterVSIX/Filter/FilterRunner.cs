using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyFilterVSIX
{
    class FilterRunner
    {
        public static string Run(Filter filter, string inputText, string userInputText)
        {
            try
            {
                string actualInputText = filter.UsesTemplateFile
                    ? BuildActualInputText(inputText, userInputText, filter.TemplateFilePath, MyEncoding.GetEncoding(filter.InputEncodingName))
                    : inputText;
                var outputBuf = new StringBuilder();
                var proc = CreateProcess(filter, actualInputText, userInputText, outputBuf);
                StartProcess(proc, filter, actualInputText);
                proc.WaitForExit();
                proc.Close();
                return TrimLastNewLine(outputBuf.ToString());
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static Task<string> RunAsync(Filter filter, string inputText, string userInputText)
        {
            try
            {
                string actualInputText = filter.UsesTemplateFile
                    ? BuildActualInputText(inputText, userInputText, filter.TemplateFilePath, MyEncoding.GetEncoding(filter.InputEncodingName))
                    : inputText;
                var outputBuf = new StringBuilder();
                var proc = CreateProcess(filter, actualInputText, userInputText, outputBuf);
                var tcs = new TaskCompletionSource<string>();
                proc.EnableRaisingEvents = true;
                proc.Exited += (sender, e) =>
                {
                    proc.Close();
                    tcs.TrySetResult(TrimLastNewLine(outputBuf.ToString()));
                };
                StartProcess(proc, filter, actualInputText);
                return tcs.Task;
            }
            catch (Exception e)
            {
                return new Task<string>(() => e.ToString());
            }
        }

        static Process CreateProcess(Filter filter, string inputText, string userInputText, StringBuilder outputBuf)
        {
            string inputTempFilePath = null;
            if (filter.Arguments.Contains(VariableName_InputTempFilePath))
            {
                inputTempFilePath = Path.GetTempFileName();
                var inputEncoding = MyEncoding.GetEncoding(filter.InputEncodingName);
                using (var writer = new StreamWriter(inputTempFilePath, /* append: */ false, inputEncoding) {
                    NewLine = filter.InputNewLineKind.ToNewLineString()
                })
                {
                    writer.Write(inputText);
                }
            }

            var proc = new Process();
            proc.StartInfo.FileName = filter.Command;
            proc.StartInfo.Arguments = BuildActualArguments(filter.Arguments, inputText, inputTempFilePath, userInputText);
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = filter.PassesInputTextToStandardInput;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.StandardOutputEncoding = MyEncoding.GetEncoding(filter.OutputEncodingName);
            proc.StartInfo.StandardErrorEncoding = MyEncoding.GetEncoding(filter.OutputEncodingName);
            proc.OutputDataReceived += (sender, e) => { if (e.Data != null) { outputBuf.AppendLine(e.Data); } };
            proc.ErrorDataReceived += (sender, e) => { if (e.Data != null) { outputBuf.AppendLine(e.Data); } };
            if (inputTempFilePath != null)
            {
                proc.Exited += (sender, e) => File.Delete(inputTempFilePath);
            }

            return proc;
        }

        static void StartProcess(Process proc, Filter filter, string inputText)
        {
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            if (filter.PassesInputTextToStandardInput)
            {
                // ref. http://stackoverflow.com/questions/2855675/process-standardinput-encoding-problem
                using (var stdin = new StreamWriter(proc.StandardInput.BaseStream, MyEncoding.GetEncoding(filter.InputEncodingName)))
                {
                    stdin.NewLine = filter.InputNewLineKind.ToNewLineString();
                    stdin.Write(inputText);
                    stdin.Flush();
                }
            }
        }

        static string TrimLastNewLine(string src)
        {
            int iLastNewLine = src.LastIndexOf(Environment.NewLine);
            if (iLastNewLine == -1) return src;
            if (src.Substring(iLastNewLine) != Environment.NewLine) return src;
            return src.Substring(0, iLastNewLine);
        }

        public const string VariableName_InputTempFilePath = "$(InputTempFilePath)";
        const string VariableName_InputText = "$(InputText)";
        public const string VariableName_UserInput = "$(UserInput)";

        static string BuildActualArguments(string srcArguments, string inputText, string inputTempFilePath, string userInputText)
        {
            return srcArguments
                .Replace(VariableName_InputText, inputText)
                .Replace(VariableName_InputTempFilePath, inputTempFilePath)
                .Replace(VariableName_UserInput, userInputText);
        }

        static string BuildActualInputText(string inputText, string userInputText, string templateFilePath, Encoding encoding)
        {
            try
            {
                using (var reader = new StreamReader(templateFilePath, encoding))
                {
                    return reader.ReadToEnd()
                        .Replace(VariableName_InputText, inputText)
                        .Replace(VariableName_UserInput, userInputText);
                }
            }
            catch
            {
                return inputText;
            }
        }
    }
}
