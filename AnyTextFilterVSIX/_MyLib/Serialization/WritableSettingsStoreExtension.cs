using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;

namespace lpubsppop01.AnyTextFilterVSIX
{
    static class WritableSettingsStoreExtension
    {
        #region Get/SetList<T>

        public static IList<T> GetList<T>(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, Func<string, T> loadItem)
        {
            string listPath = Path.Combine(collectionPath, propertyName);
            var loaded = new List<T>();
            int iItem = 0;
            while (true)
            {
                string itemPath = Path.Combine(listPath, string.Format("Item{0}", iItem++));
                if (!settingsStore.CollectionExists(itemPath)) break;
                loaded.Add(loadItem(itemPath));
            }
            return loaded;
        }

        public static void SetList<T>(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, IList<T> value, Action<T, string> saveItem)
        {
            string listPath = Path.Combine(collectionPath, propertyName);
            int iItem = 0;
            foreach (var item in value)
            {
                string itemPath = Path.Combine(listPath, string.Format("Item{0}", iItem++));
                settingsStore.CreateCollection(itemPath);
                saveItem(item, itemPath);
            }
            while (true)
            {
                string itemPath = Path.Combine(listPath, string.Format("Item{0}", iItem++));
                if (!settingsStore.CollectionExists(itemPath)) break;
                settingsStore.DeleteCollection(itemPath);
            }
        }

        #endregion

        #region Get/SetEnum<T>

        public static T GetEnum<T>(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, T defaultValue) where T : struct
        {
            var strValue = settingsStore.GetString(collectionPath, propertyName, "");
            if (string.IsNullOrEmpty(strValue)) return defaultValue;
            T enumValue;
            if (!Enum.TryParse<T>(strValue, out enumValue)) return defaultValue;
            return enumValue;
        }

        public static void SetEnum<T>(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, T value) where T : struct
        {
            settingsStore.SetString(collectionPath, propertyName, value.ToString());
        }

        #endregion

        #region Get/SetNullableDouble

        public static double? GetNullableDouble(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, double? defaultValue)
        {
            var strValue = settingsStore.GetString(collectionPath, propertyName, "");
            if (strValue == null) return defaultValue;
            if (strValue == "") return null;
            double doubleValue;
            if (!double.TryParse(strValue, out doubleValue)) return defaultValue;
            return doubleValue;
        }

        public static void SetNullableDouble(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, double? value)
        {
            settingsStore.SetString(collectionPath, propertyName, value.HasValue ? value.Value.ToString() : "");
        }

        #endregion

        #region Get/SetGuid

        public static Guid GetGuid(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, Guid defaultValue)
        {
            var strValue = settingsStore.GetString(collectionPath, propertyName, "");
            if (string.IsNullOrEmpty(strValue)) return defaultValue;
            Guid guidValue;
            if (!Guid.TryParse(strValue, out guidValue)) return defaultValue;
            return guidValue;
        }

        public static void SetGuid(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, Guid value)
        {
            settingsStore.SetString(collectionPath, propertyName, value.ToString());
        }

        #endregion
    }
}
