using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace lpubsppop01.AnyTextFilterVSIX
{
    sealed class FilterListEdit
    {
        #region Constructor

        Func<IList<Filter>> getItemsSource;
        Func<int> getSelectedIndex;
        Action<int> setSelectedIndex;
        Window ownerWindow;

        public FilterListEdit(Func<IList<Filter>> getItemsSource, Func<int> getSelectedIndex, Action<int> setSelectedIndex, Window ownerWindow)
        {
            this.getItemsSource = getItemsSource;
            this.getSelectedIndex = getSelectedIndex;
            this.setSelectedIndex = setSelectedIndex;
            this.ownerWindow = ownerWindow;
        }

        #endregion

        #region Properties

        IList<Filter> ItemsSource
        {
            get { return getItemsSource(); }
        }

        int SelectedIndex
        {
            get { return getSelectedIndex(); }
            set { setSelectedIndex(value); }
        }

        #endregion

        #region Template File Creation

        static bool ConfirmToCreateTemplateFile(Filter filter)
        {
            var messageBuf = new StringBuilder();
            if (File.Exists(filter.TemplateFilePath))
            {
                messageBuf.AppendLine("The following file will be overwritten:");
            }
            else
            {
                messageBuf.AppendLine("A new template file will be created at the following location:");
            }
            messageBuf.AppendLine(filter.TemplateFilePath);
            messageBuf.Append("Continue?");
            return MessageBox.Show(messageBuf.ToString(), "Add Filter", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        static bool TryCreateTemplateFile(PresetFilterID presetID, Filter filter)
        {
            if (!PresetFilters.TryCreateTemplateFile(presetID, filter.TemplateFilePath, filter.InputEncodingName, filter.InputNewLineKind))
            {
                string message = "File creation failed:" + Environment.NewLine + filter.TemplateFilePath;
                MessageBox.Show(string.Format(message), "Failed");
                return false;
            }
            return true;
        }

        static bool TryCreateTemplateFile(FilterExporter.TemplateFileItem item, Filter filter)
        {
            if (!FilterExporter.CreateTemplateFile(item, filter))
            {
                string message = "File creation failed:" + Environment.NewLine + filter.TemplateFilePath;
                MessageBox.Show(string.Format(message), "Failed");
                return false;
            }
            return true;
        }

        #endregion

        #region Actions

        public bool Add()
        {
            if (ItemsSource.Count == PkgCmdIDList.FilterCountMax)
            {
                string message = string.Format("The maximum number of filter entry is {0}.", PkgCmdIDList.FilterCountMax);
                MessageBox.Show(message, "Failed");
                return false;
            }
            var dialog = new FilterAdditionWindow
            {
                Owner = ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            if (!(dialog.ShowDialog() ?? false)) return false;
            var filter = PresetFilters.Get(dialog.SelectedID);
            if (filter.UsesTemplateFile)
            {
                if (!ConfirmToCreateTemplateFile(filter)) return false;
                if (!TryCreateTemplateFile(dialog.SelectedID, filter)) return false;
            }
            ItemsSource.Add(filter);
            filter.Number = ItemsSource.Count;
            SelectedIndex = ItemsSource.Count - 1;
            return true;
        }

        public bool Remove()
        {
            if (SelectedIndex == -1) return false;
            int iRemove = SelectedIndex;
            ItemsSource.RemoveAt(iRemove);
            foreach (var filter in ItemsSource.Skip(iRemove))
            {
                filter.Number -= 1;
            }
            SelectedIndex = Math.Max(Math.Min(iRemove, ItemsSource.Count - 1), 0);
            return true;
        }

        public bool Export()
        {
            if (!ItemsSource.Any()) return false;

            // Select target filters
            var selectionDialog = new FilterSelectionWindow
            {
                Owner = ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = Properties.Resources.SelectFiltersToExport,
                Filters = ItemsSource
            };
            if (!(selectionDialog.ShowDialog() ?? false)) return false;
            var targetFilters = selectionDialog.CheckedFilters;
            if (!targetFilters.Any()) return false;

            // Get file path
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.DefaultExt = "*.json";
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = Properties.Resources.JSONFileFilter;
            if (!(saveFileDialog.ShowDialog() ?? false)) return false;
            string filePath = saveFileDialog.FileName;

            // Export
            FilterExporter.Export(targetFilters, filePath);
            return true;
        }

        public bool Import()
        {
            // Get file path
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.DefaultExt = "*.json";
            openFileDialog.AddExtension = true;
            openFileDialog.Filter = Properties.Resources.JSONFileFilter;
            if (!(openFileDialog.ShowDialog() ?? false)) return false;
            string filePath = openFileDialog.FileName;

            // Import
            IList<FilterExporter.TemplateFileItem> templateFileItems;
            var filters = FilterExporter.Import(filePath, out templateFileItems);

            // Select target filters
            var selectionDialog = new FilterSelectionWindow
            {
                Owner = ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = Properties.Resources.SelectFiltersToImport,
                Filters = filters
            };
            if (!(selectionDialog.ShowDialog() ?? false)) return false;
            var targetFilters = selectionDialog.CheckedFilters;
            if (!targetFilters.Any()) return false;

            // Check count
            if (ItemsSource.Count + targetFilters.Count > PkgCmdIDList.FilterCountMax)
            {
                string message = string.Format("The maximum number of filter entry is {0}.", PkgCmdIDList.FilterCountMax);
                MessageBox.Show(message, "Failed");
                return false;
            }

            // Apply
            foreach (var filter in targetFilters)
            {
                if (filter.UsesTemplateFile)
                {
                    var templateFileItem = templateFileItems.FirstOrDefault(i => i.Path == filter.TemplateFilePath);
                    if (templateFileItem != null)
                    {
                        if (!ConfirmToCreateTemplateFile(filter)) return false;
                        if (!TryCreateTemplateFile(templateFileItem, filter)) return false;
                    }
                }
                ItemsSource.Add(filter);
                filter.Number = ItemsSource.Count;
            }
            SelectedIndex = ItemsSource.Count - 1;
            return true;
        }

        #endregion
    }
}
