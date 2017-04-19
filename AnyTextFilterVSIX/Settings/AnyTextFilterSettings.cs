using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace lpubsppop01.AnyTextFilterVSIX
{
    // ref. https://github.com/omsharp/BetterComments/blob/master/BetterComments/Options/FontSettingsManager.cs

    sealed class AnyTextFilterSettings : ICloneable, INotifyPropertyChanged
    {
        #region Constructor

        public AnyTextFilterSettings()
        {
            Filters = new ObservableCollection<Filter>();
            Culture = MyCultureInfo.Auto;
        }

        AnyTextFilterSettings(AnyTextFilterSettings src)
        {
            Copy(src);
        }

        #endregion

        #region Properties

        ObservableCollection<Filter> filters;
        public ObservableCollection<Filter> Filters
        {
            get { return filters; }
            private set { filters = value; OnPropertyChanged(); }
        }

        bool usesEmacsLikeKeybindings;
        public bool UsesEmacsLikeKeybindings
        {
            get { return usesEmacsLikeKeybindings; }
            set { usesEmacsLikeKeybindings = value; OnPropertyChanged(); }
        }

        MyCultureInfo culture;
        public MyCultureInfo Culture
        {
            get { return culture; }
            set { culture = value; OnPropertyChanged(); }
        }

        #endregion

        #region Copy

        public void Copy(AnyTextFilterSettings src)
        {
            Filters = new ObservableCollection<Filter>(src.Filters.Select(f => f.Clone()));
            UsesEmacsLikeKeybindings = src.UsesEmacsLikeKeybindings;
            Culture = src.Culture;
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        public AnyTextFilterSettings Clone()
        {
            return new AnyTextFilterSettings(this);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Static Members

        const string CollectionPath = "AnyTextFilter";

        static readonly WritableSettingsStore settingsStore;

        public static AnyTextFilterSettings Current { get; private set; }

        static AnyTextFilterSettings()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            Current = new AnyTextFilterSettings();
            LoadCurrent();
        }

        public static void SaveCurrent()
        {
            if (!settingsStore.CollectionExists(CollectionPath))
            {
                settingsStore.CreateCollection(CollectionPath);
            }

            var adapter = new WritableSettingsStoreAdapter(settingsStore);
            adapter.SetList(CollectionPath, "Filters", Current.Filters, (e, p) => e.Save(adapter, p));
            adapter.SetBoolean(CollectionPath, "UsesEmacsLikeKeybindings", Current.UsesEmacsLikeKeybindings);
            adapter.SetString(CollectionPath, "Culture", Current.Culture.Name);
        }

        public static void LoadCurrent()
        {
            if (!settingsStore.CollectionExists(CollectionPath)) return;

            var adapter = new WritableSettingsStoreAdapter(settingsStore);
            Current.Filters = new ObservableCollection<Filter>(adapter.GetList(CollectionPath, "Filters", new Filter[0], (itemPath) => Filter.Load(adapter, itemPath)));
            Current.UsesEmacsLikeKeybindings = adapter.GetBoolean(CollectionPath, "UsesEmacsLikeKeybindings", false);
            Current.Culture = MyCultureInfo.GetCultureInfo(adapter.GetString(CollectionPath, "Culture", ""));
        }

        #endregion
    }
}
