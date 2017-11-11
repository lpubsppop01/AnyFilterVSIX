using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

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
        #region Properties

        OleMenuCommandService m_menuCommandService;
        OleMenuCommandService MenuCommandService
        {
            get
            {
                if (m_menuCommandService == null)
                {
                    m_menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                }
                return m_menuCommandService;
            }
        }

        #endregion

        #region Overrides

        protected override void Initialize()
        {
            base.Initialize();

            if (MenuCommandService == null) return;

            SetResourceCulture();
            AddSettingsMenuItem();
            UpdateRunFilterMenuItems();
        }

        #endregion

        #region Settings

        void SetResourceCulture()
        {
            NameToResourceBinding.Culture = AnyTextFilterSettings.Current.Culture;
            AnyTextFilterSettings.Current.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != "Culture") return;
                NameToResourceBinding.Culture = AnyTextFilterSettings.Current.Culture;
            };
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
                    if (AnyTextFilterSettings.RepairCurrent()) MessageBox.Show("Invalid settings were repaired.");
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
            MenuCommandService.AddCommand(menuItem);
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
            var menuItem = MenuCommandService.FindCommand(menuCommandID);
            if (menuItem == null) return;
            MenuCommandService.RemoveCommand(menuItem);
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
            MenuCommandService.AddCommand(menuItem);
        }

        void ActivateToolWindow(Filter filter)
        {
            var window = FindToolWindow(typeof(FilterRunnerWindowPane), id: 0, create: true) as FilterRunnerWindowPane;
            if (window == null) return;
            window.Content.SelectedFilter = filter;
            ErrorHandler.ThrowOnFailure(window.Frame.Show());
        }

        #endregion
    }
}
