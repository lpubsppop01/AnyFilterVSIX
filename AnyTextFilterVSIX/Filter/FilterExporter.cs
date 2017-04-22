using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    static class FilterExporter
    {
        #region Template File Item

        public class TemplateFileItem
        {
            #region Constructor

            public TemplateFileItem(string path, string content)
            {
                Path = path;
                Content = content;
            }

            #endregion

            #region Properties

            public string Path { get; private set; }
            public string Content { get; private set; }

            #endregion

            #region Serialization

            public void Save(ISettingsStoreAdapter settingsStore, string collectionPath)
            {
                settingsStore.SetString(collectionPath, "Path", Path);
                settingsStore.SetString(collectionPath, "Content", Content);
            }

            public static TemplateFileItem Load(ISettingsStoreAdapter settingsStore, string collectionPath)
            {
                return new TemplateFileItem(
                    settingsStore.GetString(collectionPath, "Path", ""),
                    settingsStore.GetString(collectionPath, "Content", ""));
            }

            #endregion
        }

        static TemplateFileItem CreateTemplateFileItem(Filter filter)
        {
            if (!filter.UsesTemplateFile) return null;
            if (!File.Exists(filter.TemplateFilePath)) return null;
            using (var reader = new StreamReader(filter.TemplateFilePath, MyEncoding.GetEncoding(filter.InputEncodingName)))
            {
                string rawContent = reader.ReadToEnd();
                var newLineKind = rawContent.DetectNewLineKind() ?? Environment.NewLine.ToNewLineKind();
                string envNLContent = rawContent.ConvertNewLineToEnvironment(newLineKind);
                return new TemplateFileItem(filter.TemplateFilePath, envNLContent);
            }
        }

        public static bool CreateTemplateFile(TemplateFileItem item, Filter filter)
        {
            try
            {
                string inputNLContent = item.Content.ConvertNewLineFromEnvironment(filter.InputNewLineKind);
                using (var writer = new StreamWriter(filter.TemplateFilePath, /* append: */false, MyEncoding.GetEncoding(filter.InputEncodingName)))
                {
                    writer.Write(inputNLContent);
                }
                return true;
            }
            catch { }
            return false;
        }

        #endregion

        #region Path

        public static string GetExportedDirectoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"AnyTextFilterVSIX\Exported");
        }

        #endregion

        #region Actions

        const string VariableName_MyDocuments = "$(MyDocuments)";

        public static void Export(IList<Filter> filters, string filePath)
        {
            // Replace personal folder path to variable name
            var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var variableFilters = filters.Select(f =>
            {
                var v = f.Clone();
                if (v.TemplateFilePath.StartsWith(myDocPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    v.TemplateFilePath = VariableName_MyDocuments + v.TemplateFilePath.Substring(myDocPath.Length);
                }
                return v;
            }).ToArray();

            // Write to JSON file
            var settingsStore = new JSONSettingsStoreAdapter();
            settingsStore.SetList("Exported", "Filters", variableFilters, (item, itemPath) => item.Save(settingsStore, itemPath));
            var templateFilePaths = variableFilters.Select(f => CreateTemplateFileItem(f)).Where(i => i != null).ToArray();
            settingsStore.SetList("Exported", "TemplateFiles", templateFilePaths, (item, itemPath) => item.Save(settingsStore, itemPath));
            string serialized = settingsStore.Serialize();
            using (var writer = new StreamWriter(filePath))
            {
                writer.Write(serialized);
            }
        }

        public static IList<Filter> Import(string filePath, out IList<TemplateFileItem> templateFileItems)
        {
            // Read from JSON file
            string serialized;
            using (var reader = new StreamReader(filePath))
            {
                serialized = reader.ReadToEnd();
            }
            var settingsStore = new JSONSettingsStoreAdapter();
            settingsStore.Deserialize(serialized);
            var variableFilters = settingsStore.GetList("Exported", "Filters", new Filter[0], (itemPath) => Filter.Load(settingsStore, itemPath));
            templateFileItems = settingsStore.GetList("Exported", "TemplateFiles", new TemplateFileItem[0], (itemPath) => TemplateFileItem.Load(settingsStore, itemPath));

            // Replace variable name to personal folder path
            var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
            var filters = variableFilters.Select(v =>
            {
                var f = v.Clone();
                if (f.TemplateFilePath.StartsWith(VariableName_MyDocuments))
                {
                    f.TemplateFilePath = myDocPath + f.TemplateFilePath.Substring(VariableName_MyDocuments.Length);
                }
                return f;
            }).ToArray();
            return filters;
        }

        #endregion
    }
}
