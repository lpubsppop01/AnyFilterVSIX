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
            History = new ObservableCollection<FilterHistoryItem>();
        }

        AnyTextFilterSettings(AnyTextFilterSettings src)
        {
            Copy(src);
        }

        #endregion

        #region Properties

        ObservableCollection<Filter> m_Filters;
        public ObservableCollection<Filter> Filters
        {
            get { return m_Filters; }
            private set { m_Filters = value; OnPropertyChanged(); }
        }

        bool m_UsesEmacsLikeKeybindings;
        public bool UsesEmacsLikeKeybindings
        {
            get { return m_UsesEmacsLikeKeybindings; }
            set { m_UsesEmacsLikeKeybindings = value; OnPropertyChanged(); }
        }

        MyCultureInfo m_Culture;
        public MyCultureInfo Culture
        {
            get { return m_Culture; }
            set { m_Culture = value; OnPropertyChanged(); }
        }

        ObservableCollection<FilterHistoryItem> m_History;
        public ObservableCollection<FilterHistoryItem> History
        {
            get { return m_History; }
            private set { m_History = value; OnPropertyChanged(); }
        }

        #endregion

        #region Copy

        public void Copy(AnyTextFilterSettings src)
        {
            Filters = new ObservableCollection<Filter>(src.Filters.Select(f => f.Clone()));
            UsesEmacsLikeKeybindings = src.UsesEmacsLikeKeybindings;
            Culture = src.Culture;
            History = new ObservableCollection<FilterHistoryItem>(src.History.Select(h => h.Clone()));
        }

        #endregion

        #region Begin/EndEdit

        bool m_IsEditing;
        HashSet<string> m_EditedPropertyNames;

        public bool BeginEdit()
        {
            if (m_IsEditing) return false;
            m_IsEditing = true;
            m_EditedPropertyNames = new HashSet<string>();
            return true;
        }

        public void EndEdit()
        {
            m_IsEditing = false;
            foreach (var name in m_EditedPropertyNames)
            {
                OnPropertyChanged(name);
            }
            m_EditedPropertyNames = null;
        }

        #endregion

        #region Events

        public event EventHandler Loaded;

        void OnLoaded()
        {
            Loaded?.Invoke(this, new EventArgs());
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
            if (m_IsEditing)
            {
                m_EditedPropertyNames.Add(propertyName);
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Static Members

        const string CollectionPath = "AnyTextFilter";

        static readonly WritableSettingsStore settingsStore;

        public static AnyTextFilterSettings Current { get; private set; }

        public static readonly int HistoryCountMax = 10;

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
            SaveCurrentHistory(adapter);
        }

        public static void SaveCurrentHistory(ISettingsStoreAdapter adapter = null)
        {
            if (adapter == null)
            {
                adapter = new WritableSettingsStoreAdapter(settingsStore);
            }
            adapter.SetList(CollectionPath, "History", Current.History, (e, p) => e.Save(adapter, p));
        }

        public static void LoadCurrent()
        {
            if (!settingsStore.CollectionExists(CollectionPath)) return;

            var adapter = new WritableSettingsStoreAdapter(settingsStore);
            Current.Filters = new ObservableCollection<Filter>(
                adapter.GetList(CollectionPath, "Filters", new Filter[0], (itemPath) => Filter.Load(adapter, itemPath)));
            int displayNumber = 0;
            foreach (var f in Current.Filters) f.DisplayNumber = ++displayNumber;
            Current.UsesEmacsLikeKeybindings = adapter.GetBoolean(CollectionPath, "UsesEmacsLikeKeybindings", false);
            Current.Culture = MyCultureInfo.GetCultureInfo(adapter.GetString(CollectionPath, "Culture", ""));
            LoadCurrentHistory(adapter);
            Current.OnLoaded();
        }

        public static void LoadCurrentHistory(ISettingsStoreAdapter adapter = null)
        {
            if (adapter == null)
            {
                adapter = new WritableSettingsStoreAdapter(settingsStore);
            }
            Current.History = new ObservableCollection<FilterHistoryItem>(
                adapter.GetList(CollectionPath, "History", new FilterHistoryItem[0],
                    (itemPath) => FilterHistoryItem.Load(adapter, itemPath)));
        }

        #endregion
    }
}
