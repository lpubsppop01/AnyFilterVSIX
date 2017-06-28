using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class Filter : ICloneable, INotifyPropertyChanged
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
            ID = Guid.Empty;
            DisplayNumber = 0;
            Title = "";
            Command = "";
            Arguments = "";
            InputNewLineKind = NewLineKind.CRLF;
            InputEncodingName = Encoding.Default.WebName;
            OutputEncodingName = Encoding.Default.WebName;
            TempFileExtension = ".tmp";
            TargetSpanForNoSelection = TargetSpanForNoSelection.CaretPosition;
            InsertsAfterTargetSpan = false;
            PassesInputTextToStandardInput = false;
            UsesTemplateFile = false;
            TemplateFilePath = "";
            Memo = "";
            UserInputWindow_ShowsDifference = false;
        }

        public void Copy(Filter src)
        {
            ID = src.ID;
            DisplayNumber = src.DisplayNumber;
            Title = src.Title;
            Command = src.Command;
            Arguments = src.Arguments;
            InputNewLineKind = src.InputNewLineKind;
            InputEncodingName = src.InputEncodingName;
            OutputEncodingName = src.OutputEncodingName;
            TempFileExtension = src.TempFileExtension;
            TargetSpanForNoSelection = src.TargetSpanForNoSelection;
            InsertsAfterTargetSpan = src.InsertsAfterTargetSpan;
            PassesInputTextToStandardInput = src.PassesInputTextToStandardInput;
            UsesTemplateFile = src.UsesTemplateFile;
            TemplateFilePath = src.TemplateFilePath;
            Memo = src.Memo;
            UserInputWindow_ShowsDifference = src.UserInputWindow_ShowsDifference;
        }

        #endregion

        #region Properties

        Guid m_ID;
        public Guid ID
        {
            get { return m_ID; }
            set { m_ID = value; OnPropertyChanged(); }
        }

        int m_DisplayNumber;
        public int DisplayNumber
        {
            get { return m_DisplayNumber; }
            set { m_DisplayNumber = value; OnPropertyChanged(); }
        }

        string m_Title;
        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; OnPropertyChanged(); }
        }

        string m_Command;
        public string Command
        {
            get { return m_Command; }
            set { m_Command = value; OnPropertyChanged(); }
        }

        string m_Arguments;
        public string Arguments
        {
            get { return m_Arguments; }
            set { m_Arguments = value; OnPropertyChanged(); }
        }

        NewLineKind m_InputNewLineKind;
        public NewLineKind InputNewLineKind
        {
            get { return m_InputNewLineKind; }
            set { m_InputNewLineKind = value; OnPropertyChanged(); }
        }

        string m_InputEncodingName;
        public string InputEncodingName
        {
            get { return m_InputEncodingName; }
            set { m_InputEncodingName = value; OnPropertyChanged(); }
        }

        string m_OutputEncodingName;
        public string OutputEncodingName
        {
            get { return m_OutputEncodingName; }
            set { m_OutputEncodingName = value; OnPropertyChanged(); }
        }

        string m_TempFileExtension;
        public string TempFileExtension
        {
            get { return m_TempFileExtension; }
            set { m_TempFileExtension = value; OnPropertyChanged(); }
        }

        TargetSpanForNoSelection m_TargetSpanForNoSelection;
        public TargetSpanForNoSelection TargetSpanForNoSelection
        {
            get { return m_TargetSpanForNoSelection; }
            set { m_TargetSpanForNoSelection = value; OnPropertyChanged(); }
        }

        bool m_InsertsAfterTargetSpan;
        public bool InsertsAfterTargetSpan
        {
            get { return m_InsertsAfterTargetSpan; }
            set { m_InsertsAfterTargetSpan = value; OnPropertyChanged(); }
        }

        bool m_PassesInputTextToStandardInput;
        public bool PassesInputTextToStandardInput
        {
            get { return m_PassesInputTextToStandardInput; }
            set { m_PassesInputTextToStandardInput = value; OnPropertyChanged(); }
        }

        bool m_UsesTemplateFile;
        public bool UsesTemplateFile
        {
            get { return m_UsesTemplateFile; }
            set { m_UsesTemplateFile = value; OnPropertyChanged(); }
        }

        string m_TemplateFilePath;
        public string TemplateFilePath
        {
            get { return m_TemplateFilePath; }
            set { m_TemplateFilePath = value; OnPropertyChanged(); }
        }

        string m_Memo;
        public string Memo
        {
            get { return m_Memo; }
            set { m_Memo = value; OnPropertyChanged(); }
        }

        bool m_UserInputWindow_ShowsDifference;
        public bool UserInputWindow_ShowsDifference
        {
            get { return m_UserInputWindow_ShowsDifference; }
            set { m_UserInputWindow_ShowsDifference = value; OnPropertyChanged(); }
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
                using (var reader = new StreamReader(m_TemplateFilePath, MyEncoding.GetEncoding(InputEncodingName)))
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

        public void Save(ISettingsStoreAdapter settingsStore, string collectionPath)
        {
            settingsStore.SetGuid(collectionPath, "ID", ID);
            settingsStore.SetString(collectionPath, "Title", Title);
            settingsStore.SetString(collectionPath, "Command", Command);
            settingsStore.SetString(collectionPath, "Arguments", Arguments);
            settingsStore.SetEnum(collectionPath, "InputNewLineKind", InputNewLineKind);
            settingsStore.SetString(collectionPath, "InputEncodingName", InputEncodingName);
            settingsStore.SetString(collectionPath, "OutputEncodingName", OutputEncodingName);
            settingsStore.SetString(collectionPath, "TempFileExtension", TempFileExtension);
            settingsStore.SetEnum<TargetSpanForNoSelection>(collectionPath, "TargetSpanForNoSelection", TargetSpanForNoSelection);
            settingsStore.SetBoolean(collectionPath, "InsertsAfterTargetSpan", InsertsAfterTargetSpan);
            settingsStore.SetBoolean(collectionPath, "PassesInputTextToStandardInput", PassesInputTextToStandardInput);
            settingsStore.SetBoolean(collectionPath, "UsesTemplateFile", UsesTemplateFile);
            settingsStore.SetString(collectionPath, "TemplateFilePath", TemplateFilePath);
            settingsStore.SetString(collectionPath, "Memo", Memo);
            settingsStore.SetBoolean(collectionPath, "UserInputWindow_ShowsDifference", UserInputWindow_ShowsDifference);
        }

        public static Filter Load(ISettingsStoreAdapter settingsStore, string collectionPath)
        {
            return new Filter
            {
                ID = settingsStore.GetGuid(collectionPath, "ID", Guid.NewGuid()),
                Title = settingsStore.GetString(collectionPath, "Title", ""),
                Command = settingsStore.GetString(collectionPath, "Command", ""),
                Arguments = settingsStore.GetString(collectionPath, "Arguments", ""),
                InputNewLineKind = settingsStore.GetEnum(collectionPath, "InputNewLineKind", default(NewLineKind)),
                InputEncodingName = settingsStore.GetString(collectionPath, "InputEncodingName", Encoding.Default.WebName),
                OutputEncodingName = settingsStore.GetString(collectionPath, "OutputEncodingName", Encoding.Default.WebName),
                TempFileExtension = settingsStore.GetString(collectionPath, "TempFileExtension", ".tmp"),
                TargetSpanForNoSelection = settingsStore.GetEnum<TargetSpanForNoSelection>(collectionPath, "TargetSpanForNoSelection", TargetSpanForNoSelection.CaretPosition),
                InsertsAfterTargetSpan = settingsStore.GetBoolean(collectionPath, "InsertsAfterTargetSpan", false),
                PassesInputTextToStandardInput = settingsStore.GetBoolean(collectionPath, "PassesInputTextToStandardInput", false),
                UsesTemplateFile = settingsStore.GetBoolean(collectionPath, "UsesTemplateFile", false),
                TemplateFilePath = settingsStore.GetString(collectionPath, "TemplateFilePath", ""),
                Memo = settingsStore.GetString(collectionPath, "Memo", ""),
                UserInputWindow_ShowsDifference = settingsStore.GetBoolean(collectionPath, "UserInputWindow_ShowsDifference", false),
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public enum TargetSpanForNoSelection
    {
        CaretPosition, CurrentLine, WholeDocument
    }
}
