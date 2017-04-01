using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace lpubsppop01.AnyFilterVSIX
{
    // ref. https://github.com/omsharp/BetterComments/blob/master/BetterComments/Options/FontSettingsManager.cs

    class AnyFilterSettings
    {
        #region Constructor

        public AnyFilterSettings()
        {
            Filters = new List<Filter>();
        }

        #endregion

        #region Properties

        public List<Filter> Filters { get; private set; }

        public bool UsesEmacsLikeKeybindings { get; set; }

        #endregion

        #region Static Members

        const string CollectionPath = "AnyFilter";

        static readonly WritableSettingsStore settingsStore;

        public static AnyFilterSettings Current { get; private set; }

        static AnyFilterSettings()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            Current = new AnyFilterSettings();
            LoadCurrent();
        }

        public static void SaveCurrent()
        {
            if (!settingsStore.CollectionExists(CollectionPath))
            {
                settingsStore.CreateCollection(CollectionPath);
            }

            Current.Filters.SaveCollection(settingsStore, Path.Combine(CollectionPath, "Filters"), (e, s, p) => e.Save(s, p));
            settingsStore.SetBoolean(CollectionPath, "UsesEmacsLikeKeybindings", Current.UsesEmacsLikeKeybindings);
        }

        public static void LoadCurrent()
        {
            if (!settingsStore.CollectionExists(CollectionPath)) return;

            Current.Filters = WritableSettingsStoreExtension.LoadCollection(settingsStore, Path.Combine(CollectionPath, "Filters"), Filter.Load);
            Current.UsesEmacsLikeKeybindings = settingsStore.GetBoolean(CollectionPath, "UsesEmacsLikeKeybindings", false);
        }

        #endregion
    }
}
