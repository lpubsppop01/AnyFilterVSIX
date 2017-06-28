using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;

namespace lpubsppop01.AnyTextFilterVSIX
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")] // Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution
    [ProvideToolWindow(typeof(FilterRunnerWindowPane), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom)]
    [Guid(GuidList.guidAnyTextFilterPkgString)]
    public sealed class AnyTextFilterPackage : Package
    {
        #region Overrides

        protected override void Initialize()
        {
            base.Initialize();

            if (!TryCacheRequiredServices()) return;

            NameToResourceBinding.Culture = AnyTextFilterSettings.Current.Culture;
            AnyTextFilterSettings.Current.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != "Culture") return;
                NameToResourceBinding.Culture = AnyTextFilterSettings.Current.Culture;
            };

            AddSettingsMenuItem();
            UpdateRunFilterMenuItems();
        }

        #endregion

        #region Service Cache

        OleMenuCommandService menuCommandService;

        bool TryCacheRequiredServices()
        {
            menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (menuCommandService == null) return false;
            return true;
        }

        void ActivateToolWindow(Filter filter)
        {
            var window = FindToolWindow(typeof(FilterRunnerWindowPane), id: 0, create: true) as FilterRunnerWindowPane;
            if (window == null) return;
            window.Content.SelectedFilter = filter;
            ErrorHandler.ThrowOnFailure(window.Frame.Show());
        }

        #endregion

        #region Menu Commands

        void AddSettingsMenuItem()
        {
            var menuCommandID = new CommandID(GuidList.guidAnyTextFilterCmdSet, (int)PkgCmdIDList.cmdidSettings);
            OleMenuCommand menuItem = null;
            menuItem = new OleMenuCommand((sender, e) =>
            {
                var backup = AnyTextFilterSettings.Current.Clone();
                var dialog = new AnyTextFilterSettingsWindow
                {
                    DataContext = AnyTextFilterSettings.Current,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                if (dialog.ShowDialog() ?? false)
                {
                    AnyTextFilterSettings.SaveCurrent();
                    menuItem.Text = Properties.Resources.AnyTextFilterSettings_;
                    UpdateRunFilterMenuItems();
                }
                else
                {
                    AnyTextFilterSettings.Current.Copy(backup);
                }
            }, menuCommandID)
            {
                Text = Properties.Resources.AnyTextFilterSettings_
            };
            menuCommandService.AddCommand(menuItem);
        }

        void UpdateRunFilterMenuItems()
        {
            int filterCount = AnyTextFilterSettings.Current.Filters.Count;
            for (int iFilter = 0; iFilter < filterCount; ++iFilter)
            {
                RemoveRunFilterMenuItem(iFilter);
                AddRunFilterMenuItem(iFilter);
            }
            for (int iFilter = filterCount; iFilter < PkgCmdIDList.FilterCountMax; ++iFilter)
            {
                RemoveRunFilterMenuItem(iFilter);
            }
        }

        void RemoveRunFilterMenuItem(int iFilter)
        {
            var menuCommandID = new CommandID(GuidList.guidAnyTextFilterCmdSet, (int)PkgCmdIDList.GetCmdidRunFilter(iFilter));
            var menuItem = menuCommandService.FindCommand(menuCommandID);
            if (menuItem == null) return;
            menuCommandService.RemoveCommand(menuItem);
        }

        void AddRunFilterMenuItem(int iFilter)
        {
            var filter = AnyTextFilterSettings.Current.Filters[iFilter];
            if (filter == null) return;

            var menuCommandID = new CommandID(GuidList.guidAnyTextFilterCmdSet, (int)PkgCmdIDList.GetCmdidRunFilter(iFilter));
            var menuItem = new OleMenuCommand((sender, e) =>
            {
                ActivateToolWindow(filter);
            }, menuCommandID)
            {
                Text = filter.Title
            };
            menuCommandService.AddCommand(menuItem);
        }

        #endregion
    }
}
