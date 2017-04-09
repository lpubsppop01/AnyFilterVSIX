using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;

namespace lpubsppop01.AnyFilterVSIX
{
    public class Filter : ICloneable, INotifyPropertyChanged
    {
        #region Constructors

        public Filter()
        {
            Clear();
        }

        protected Filter(Filter src)
        {
            Copy(src);
        }

        public void Clear()
        {
            Number = 0;
            Title = "";
            Command = "";
            Arguments = "";
            InputNewLineKind = MyNewLineKind.CRLF;
            InputEncodingName = Encoding.Default.WebName;
            OutputEncodingName = Encoding.Default.WebName;
            InsertsAfterCurrentLine = false;
            PassesInputTextToStandardInput = false;
            UsesTemplateFile = false;
            TemplateFilePath = "";
            UserInputWindow_ShowsDifference = false;
            UserInputWindow_Width = null;
            UserInputWindow_Height = null;
        }

        public void Copy(Filter src)
        {
            Number = src.Number;
            Title = src.Title;
            Command = src.Command;
            Arguments = src.Arguments;
            InputNewLineKind = src.InputNewLineKind;
            InputEncodingName = src.InputEncodingName;
            OutputEncodingName = src.OutputEncodingName;
            InsertsAfterCurrentLine = src.InsertsAfterCurrentLine;
            PassesInputTextToStandardInput = src.PassesInputTextToStandardInput;
            UsesTemplateFile = src.UsesTemplateFile;
            TemplateFilePath = src.TemplateFilePath;
            UserInputWindow_ShowsDifference = src.UserInputWindow_ShowsDifference;
            UserInputWindow_Width = src.UserInputWindow_Width;
            UserInputWindow_Height = src.UserInputWindow_Height;
        }

        #endregion

        #region Properties

        int number;
        public int Number
        {
            get { return number; }
            set { number = value; OnPropertyChanged(); }
        }

        string title;
        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(); }
        }

        string command;
        public string Command
        {
            get { return command; }
            set { command = value; OnPropertyChanged(); }
        }

        string arguments;
        public string Arguments
        {
            get { return arguments; }
            set { arguments = value; OnPropertyChanged(); }
        }

        MyNewLineKind inputNewLineKind;
        public MyNewLineKind InputNewLineKind
        {
            get { return inputNewLineKind; }
            set { inputNewLineKind = value; OnPropertyChanged(); }
        }

        string inputEncodingName;
        public string InputEncodingName
        {
            get { return inputEncodingName; }
            set { inputEncodingName = value; OnPropertyChanged(); }
        }

        string outputEncodingName;
        public string OutputEncodingName
        {
            get { return outputEncodingName; }
            set { outputEncodingName = value; OnPropertyChanged(); }
        }

        bool insertsAfterCurrentLine;
        public bool InsertsAfterCurrentLine
        {
            get { return insertsAfterCurrentLine; }
            set { insertsAfterCurrentLine = value; OnPropertyChanged(); }
        }

        bool passesInputTextToStandardInput;
        public bool PassesInputTextToStandardInput
        {
            get { return passesInputTextToStandardInput; }
            set { passesInputTextToStandardInput = value; OnPropertyChanged(); }
        }

        bool usesTemplateFile;
        public bool UsesTemplateFile
        {
            get { return usesTemplateFile; }
            set { usesTemplateFile = value; OnPropertyChanged(); }
        }

        string templateFilePath;
        public string TemplateFilePath
        {
            get { return templateFilePath; }
            set { templateFilePath = value; OnPropertyChanged(); }
        }

        bool userInputWindow_ShowsDifference;
        public bool UserInputWindow_ShowsDifference
        {
            get { return userInputWindow_ShowsDifference; }
            set { userInputWindow_ShowsDifference = value; OnPropertyChanged(); }
        }

        double? userInputWindow_Width;
        public double? UserInputWindow_Width
        {
            get { return userInputWindow_Width; }
            set { userInputWindow_Width = value; OnPropertyChanged(); }
        }

        double? userInputWindow_Height;
        public double? UserInputWindow_Height
        {
            get { return userInputWindow_Height; }
            set { userInputWindow_Height = value; OnPropertyChanged(); }
        }

        #endregion

        #region Variable Check

        public bool ContainsVariable(params string[] varNames)
        {
            return varNames.Any(n => Arguments.Contains(n) || TemplateContains(n));
        }

        bool TemplateContains(string str)
        {
            if (!UsesTemplateFile) return false;
            try
            {
                using (var reader = new StreamReader(templateFilePath, MyEncoding.GetEncoding(InputEncodingName)))
                {
                    return reader.ReadToEnd().Contains(str);
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Serialization

        public void Save(WritableSettingsStore settingsStore, string collectionPath)
        {
            settingsStore.SetInt32(collectionPath, "Number", Number);
            settingsStore.SetString(collectionPath, "Title", Title);
            settingsStore.SetString(collectionPath, "Command", Command);
            settingsStore.SetString(collectionPath, "Arguments", Arguments);
            settingsStore.SetEnum(collectionPath, "InputNewLineKind", InputNewLineKind);
            settingsStore.SetString(collectionPath, "InputEncodingName", InputEncodingName);
            settingsStore.SetString(collectionPath, "OutputEncodingName", OutputEncodingName);
            settingsStore.SetBoolean(collectionPath, "InsertsAfterCurrentLine", InsertsAfterCurrentLine);
            settingsStore.SetBoolean(collectionPath, "PassesInputTextToStandardInput", PassesInputTextToStandardInput);
            settingsStore.SetBoolean(collectionPath, "UsesTemplateFile", UsesTemplateFile);
            settingsStore.SetString(collectionPath, "TemplateFilePath", TemplateFilePath);
            settingsStore.SetBoolean(collectionPath, "UserInputWindow_ShowsDifference", UserInputWindow_ShowsDifference);
            settingsStore.SetNullableDouble(collectionPath, "UserInputWindow_Width", UserInputWindow_Width);
            settingsStore.SetNullableDouble(collectionPath, "UserInputWindow_Height", UserInputWindow_Height);
        }

        public static Filter Load(WritableSettingsStore settingsStore, string collectionPath)
        {
            return new Filter
            {
                Number = settingsStore.GetInt32(collectionPath, "Number", 0),
                Title = settingsStore.GetString(collectionPath, "Title", ""),
                Command = settingsStore.GetString(collectionPath, "Command", ""),
                Arguments = settingsStore.GetString(collectionPath, "Arguments", ""),
                InputNewLineKind = settingsStore.GetEnum(collectionPath, "InputNewLineKind", default(MyNewLineKind)),
                InputEncodingName = settingsStore.GetString(collectionPath, "InputEncodingName", Encoding.Default.WebName),
                OutputEncodingName = settingsStore.GetString(collectionPath, "OutputEncodingName", Encoding.Default.WebName),
                InsertsAfterCurrentLine = settingsStore.GetBoolean(collectionPath, "InsertsAfterCurrentLine", false),
                PassesInputTextToStandardInput = settingsStore.GetBoolean(collectionPath, "PassesInputTextToStandardInput", false),
                UsesTemplateFile = settingsStore.GetBoolean(collectionPath, "UsesTemplateFile", false),
                TemplateFilePath = settingsStore.GetString(collectionPath, "TemplateFilePath", ""),
                UserInputWindow_ShowsDifference = settingsStore.GetBoolean(collectionPath, "UserInputWindow_ShowsDifference", false),
                UserInputWindow_Width = settingsStore.GetNullableDouble(collectionPath, "UserInputWindow_Width", null),
                UserInputWindow_Height = settingsStore.GetNullableDouble(collectionPath, "UserInputWindow_Height", null),
            };
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Filter Clone()
        {
            return new Filter(this);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
