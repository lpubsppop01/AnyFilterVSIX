using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class WritableSettingsStoreAdapter : ISettingsStoreAdapter
    {
        #region Constructor

        WritableSettingsStore settingsStore;

        public WritableSettingsStoreAdapter(WritableSettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;
        }

        #endregion

        #region ISerializerAdapter Members

        public int GetInt32(string collectionPath, string propertyName, int defaultValue = 0)
        {
            return settingsStore.GetInt32(collectionPath, propertyName, defaultValue);
        }

        public string GetString(string collectionPath, string propertyName, string defaultValue = "")
        {
            return settingsStore.GetString(collectionPath, propertyName, defaultValue);
        }

        public T GetEnum<T>(string collectionPath, string propertyName, T defaultValue = default(T)) where T : struct
        {
            return settingsStore.GetEnum(collectionPath, propertyName, defaultValue);
        }

        public bool GetBoolean(string collectionPath, string propertyName, bool defaultValue = false)
        {
            return settingsStore.GetBoolean(collectionPath, propertyName, defaultValue);
        }

        public double? GetNullableDouble(string collectionPath, string propertyName, double? defaultValue = null)
        {
            return settingsStore.GetNullableDouble(collectionPath, propertyName, defaultValue);
        }

        public IList<T> GetList<T>(string collectionPath, string propertyName, IList<T> defaultValue, Func<string, T> loadItem)
        {
            return WritableSettingsStoreExtension.GetList<T>(settingsStore, collectionPath, propertyName, (itemPath) => loadItem(itemPath)).ToList();
        }

        public void SetInt32(string collectionPath, string propertyName, int value)
        {
            settingsStore.SetInt32(collectionPath, propertyName, value);
        }

        public void SetString(string collectionPath, string propertyName, string value)
        {
            settingsStore.SetString(collectionPath, propertyName, value);
        }

        public void SetEnum<T>(string collectionPath, string propertyName, T value) where T : struct
        {
            settingsStore.SetEnum(collectionPath, propertyName, value);
        }

        public void SetBoolean(string collectionPath, string propertyName, bool value)
        {
            settingsStore.SetBoolean(collectionPath, propertyName, value);
        }

        public void SetNullableDouble(string collectionPath, string propertyName, double? value)
        {
            settingsStore.SetNullableDouble(collectionPath, propertyName, value);
        }

        public void SetList<T>(string collectionPath, string propertyName, IList<T> value, Action<T, string> saveItem)
        {
            settingsStore.SetList(collectionPath, propertyName, value, saveItem);
        }

        #endregion
    }
}
