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
                string actualInputText, userInputTempFilePath;
                Prepare(filter, inputText, userInputText, out actualInputText, out userInputTempFilePath);
                var outputBuf = new StringBuilder();
                var proc = CreateProcess(filter, actualInputText, userInputText, userInputTempFilePath, outputBuf);
                StartProcess(proc, filter, actualInputText);
                proc.WaitForExit();
                proc.Close();
                if (userInputTempFilePath != null) File.Delete(userInputTempFilePath);
                return PostProcess(outputBuf.ToString(), inputText);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static Task<string> RunAsync(Filter filter, string inputText, string userInputText)
        {
            var tcs = new TaskCompletionSource<string>();
            try
            {
                string actualInputText, userInputTempFilePath;
                Prepare(filter, inputText, userInputText, out actualInputText, out userInputTempFilePath);
                var outputBuf = new StringBuilder();
                var proc = CreateProcess(filter, actualInputText, userInputText, userInputTempFilePath, outputBuf);
                proc.EnableRaisingEvents = true;
                proc.Exited += (sender, e) =>
                {
                    proc.Close();
                    tcs.TrySetResult(PostProcess(outputBuf.ToString(), inputText));
                    if (userInputTempFilePath != null) File.Delete(userInputTempFilePath);
                };
                StartProcess(proc, filter, actualInputText);
            }
            catch (Exception e)
            {
                tcs.TrySetResult(e.ToString());
            }
            return tcs.Task;
        }

        static void Prepare(Filter filter, string inputText, string userInputText, out string actualInputText, out string userInputTempFilePath)
        {
            userInputTempFilePath = null;
            if (filter.ContainsVariable(VariableName_UserInputTempFilePath))
            {
                userInputTempFilePath = CreateUserInputTempFile(filter, userInputText);
            }

            actualInputText = filter.UsesTemplateFile
                ? BuildActualInputText(inputText, userInputText, userInputTempFilePath, filter.TemplateFilePath, MyEncoding.GetEncoding(filter.InputEncodingName))
                : inputText;
        }

        static string CreateUserInputTempFile(Filter filter, string userInputText)
        {
            string userInputTempFilePath = Path.GetTempFileName();
            var inputEncoding = MyEncoding.GetEncoding(filter.InputEncodingName);
            using (var writer = new StreamWriter(userInputTempFilePath, /* append: */ false, inputEncoding)
            {
                NewLine = filter.InputNewLineKind.ToNewLineString()
            })
            {
                writer.Write(userInputText);
            }
            return userInputTempFilePath;
        }

        static Process CreateProcess(Filter filter, string inputText, string userInputText, string userInputTempFilePath, StringBuilder outputBuf)
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
            proc.StartInfo.Arguments = BuildActualArguments(filter.Arguments, inputText, inputTempFilePath, userInputText, userInputTempFilePath);
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

        static string PostProcess(string rawOuputText, string rawInputText)
        {
            if (rawInputText.EndsWith(Environment.NewLine)) return rawOuputText;
            return TrimLastNewLine(rawOuputText);
        }

        static string TrimLastNewLine(string src)
        {
            int iLastNewLine = src.LastIndexOf(Environment.NewLine);
            if (iLastNewLine == -1) return src;
            if (src.Substring(iLastNewLine) != Environment.NewLine) return src;
            return src.Substring(0, iLastNewLine);
        }

        const string VariableName_InputText = "$(InputText)";
        public const string VariableName_InputTempFilePath = "$(InputTempFilePath)";
        public const string VariableName_UserInput = "$(UserInput)";
        public const string VariableName_UserInputTempFilePath = "$(UserInputTempFilePath)";

        static string BuildActualArguments(string srcArguments, string inputText, string inputTempFilePath, string userInputText, string userInputTempFilePath)
        {
            return srcArguments
                .Replace(VariableName_InputText, inputText)
                .Replace(VariableName_InputTempFilePath, inputTempFilePath)
                .Replace(VariableName_UserInput, userInputText)
                .Replace(VariableName_UserInputTempFilePath, userInputTempFilePath);
        }

        static string BuildActualInputText(string inputText, string userInputText, string userInputTempFilePath, string templateFilePath, Encoding encoding)
        {
            try
            {
                using (var reader = new StreamReader(templateFilePath, encoding))
                {
                    return reader.ReadToEnd()
                        .Replace(VariableName_InputText, inputText)
                        .Replace(VariableName_UserInput, userInputText)
                        .Replace(VariableName_UserInputTempFilePath, userInputTempFilePath);
                }
            }
            catch
            {
                return inputText;
            }
        }
    }
}
