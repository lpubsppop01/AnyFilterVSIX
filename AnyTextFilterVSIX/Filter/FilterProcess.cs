using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    enum FilterResultKind
    {
        Done, Cancelled, ExceptionThrown
    }

    sealed class FilterResult
    {
        #region Constructor

        public FilterResult(string outputText, FilterResultKind kind)
        {
            OutputText = outputText;
            Kind = kind;
        }

        #endregion

        #region Properties

        public string OutputText { get; private set; }
        public FilterResultKind Kind { get; private set; }

        #endregion
    }

    class FilterProcess
    {
        #region Run

        public static FilterResult Run(Filter filter, string inputText, string userInputText)
        {
            try
            {
                string actualInputText, actualUserInputText, userInputTempFilePath;
                Prepare(filter, inputText, userInputText, out actualInputText, out actualUserInputText, out userInputTempFilePath);
                var outputBuf = new StringBuilder();
                var proc = CreateProcess(filter, actualInputText, userInputText, userInputTempFilePath, outputBuf);
                StartProcess(proc, filter, actualInputText);
                proc.WaitForExit();
                proc.Close();
                if (userInputTempFilePath != null) DeleteTempFile(userInputTempFilePath);
                string outputText = PostProcess(outputBuf.ToString(), inputText);
                return new FilterResult(outputText, FilterResultKind.Done);
            }
            catch (Exception e)
            {
                return new FilterResult(e.ToString(), FilterResultKind.ExceptionThrown);
            }
        }

        public static Task<FilterResult> RunAsync(Filter filter, string inputText, string userInputText, RepeatedAsyncTaskSupport support)
        {
            var tcs = new TaskCompletionSource<FilterResult>();
            try
            {
                string actualInputText, actualUserInputText, userInputTempFilePath;
                Prepare(filter, inputText, userInputText, out actualInputText, out actualUserInputText, out userInputTempFilePath);
                var outputBuf = new StringBuilder();
                var proc = CreateProcess(filter, actualInputText, userInputText, userInputTempFilePath, outputBuf);

                EventHandler cancelReqHandler = null;
                cancelReqHandler = (sender, e) =>
                {
                    try
                    {
                        proc.Kill();
                        proc.Close();
                    }
                    catch { }
                    finally
                    {
                        tcs.TrySetResult(new FilterResult("", FilterResultKind.Cancelled));
                        if (userInputTempFilePath != null) DeleteTempFile(userInputTempFilePath);
                        support.CancelRequested -= cancelReqHandler;
                    }
                };
                support.CancelRequested += cancelReqHandler;

                proc.EnableRaisingEvents = true;
                proc.Exited += (sender, e) =>
                {
                    proc.Close();
                    string outputText = PostProcess(outputBuf.ToString(), inputText);
                    tcs.TrySetResult(new FilterResult(outputText, FilterResultKind.Done));
                    if (userInputTempFilePath != null) DeleteTempFile(userInputTempFilePath);
                    support.CancelRequested -= cancelReqHandler;
                };
                StartProcess(proc, filter, actualInputText);
            }
            catch (Exception e)
            {
                tcs.TrySetResult(new FilterResult(e.ToString(), FilterResultKind.ExceptionThrown));
            }
            return tcs.Task;
        }

        #endregion

        #region Main

        static void Prepare(Filter filter, string inputText, string userInputText, out string actualInputText, out string actualUserInputText, out string userInputTempFilePath)
        {
            actualUserInputText = userInputText.ConvertNewLineFromEnvironment(filter.InputNewLineKind);
            userInputTempFilePath = null;
            if (filter.ContainsVariable(VariableName_UserInputTempFilePath))
            {
                userInputTempFilePath = CreateUserInputTempFile(filter, actualUserInputText);
            }

            actualInputText = filter.UsesTemplateFile
                ? BuildActualInputText(inputText, userInputText, userInputTempFilePath, filter.TemplateFilePath, MyEncoding.GetEncoding(filter.InputEncodingName), filter.InputNewLineKind)
                : inputText;
        }

        static string CreateUserInputTempFile(Filter filter, string actualUserInputText)
        {
            string userInputTempFilePath = CreateTempFile(filter.TempFileExtension);
            var inputEncoding = MyEncoding.GetEncoding(filter.InputEncodingName);
            using (var writer = new StreamWriter(userInputTempFilePath, /* append: */ false, inputEncoding))
            {
                writer.Write(actualUserInputText);
            }
            return userInputTempFilePath;
        }

        static Process CreateProcess(Filter filter, string inputText, string userInputText, string userInputTempFilePath, StringBuilder outputBuf)
        {
            string inputTempFilePath = null;
            if (filter.Arguments.Contains(VariableName_InputTempFilePath))
            {
                inputTempFilePath = CreateTempFile(filter.TempFileExtension);
                var inputEncoding = MyEncoding.GetEncoding(filter.InputEncodingName);
                using (var writer = new StreamWriter(inputTempFilePath, /* append: */ false, inputEncoding))
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
                proc.Exited += (sender, e) => DeleteTempFile(inputTempFilePath);
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

        static string PostProcess(string rawOutputText, string rawInputText)
        {
            if (rawInputText.EndsWith(Environment.NewLine)) return rawOutputText;
            return TrimLastNewLine(rawOutputText);
        }

        #endregion

        #region Variable Replace

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

        static string BuildActualInputText(string inputText, string userInputText, string userInputTempFilePath, string templateFilePath, Encoding encoding, NewLineKind newLineKind)
        {
            try
            {
                using (var reader = new StreamReader(templateFilePath, encoding))
                {
                    string templateText = reader.ReadToEnd();
                    templateText = templateText.ConvertNewLineToEnvironment(newLineKind);
                    string actualInputText = templateText
                        .Replace(VariableName_InputText, inputText)
                        .Replace(VariableName_UserInput, userInputText)
                        .Replace(VariableName_UserInputTempFilePath, userInputTempFilePath);
                    actualInputText = actualInputText.ConvertNewLineFromEnvironment(newLineKind);
                    return actualInputText;
                }
            }
            catch
            {
                return inputText;
            }
        }

        #endregion

        #region New Line Convert

        static string TrimLastNewLine(string src)
        {
            int iLastNewLine = src.LastIndexOf(Environment.NewLine);
            if (iLastNewLine == -1) return src;
            if (src.Substring(iLastNewLine) != Environment.NewLine) return src;
            return src.Substring(0, iLastNewLine);
        }

        #endregion

        #region Temp File

        static DateTime lastDateTime;
        static int lastNumber;

        static string CreateTempFile(string fileNameSuffix)
        {
            string dirPath = "";
#if DEBUG
            dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"AnyTextFilterVSIX\Debug");
#else
            dirPath = Path.Combine(Path.GetTempPath(), "AnyTextFilterVSIX");
#endif
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var now = DateTime.Now;
            int number = lastNumber;
            if (now == lastDateTime)
            {
                lastNumber = ++number;
            }
            else
            {
                lastDateTime = now;
                lastNumber = 0;
            }
            string filename = now.ToString("yyyyMMdd_hhmmss_fff_") + number.ToString() + fileNameSuffix;
            return Path.Combine(dirPath, filename);
        }

        static void DeleteTempFile(string path)
        {
#if !DEBUG
            try
            {
                File.Delete(path);
            }
            catch { }
#endif
        }

        #endregion
    }
}
