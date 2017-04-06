using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;

namespace lpubsppop01.AnyFilterVSIX
{
    static class WritableSettingsStoreExtension
    {
        public static void SaveCollection<T>(this ICollection<T> elements, WritableSettingsStore settingsStore, string collectionPath, Action<T, WritableSettingsStore, string> saveElement)
        {
            int iItem = 0;
            foreach (var element in elements)
            {
                string itemPath = Path.Combine(collectionPath, string.Format("Item{0}", iItem++));
                settingsStore.CreateCollection(itemPath);
                saveElement(element, settingsStore, itemPath);
            }
            while (true)
            {
                string itemPath = Path.Combine(collectionPath, string.Format("Item{0}", iItem++));
                if (!settingsStore.CollectionExists(itemPath)) break;
                settingsStore.DeleteCollection(itemPath);
            }
        }

        public static List<T> LoadCollection<T>(WritableSettingsStore settingsStore, string collectionPath, Func<WritableSettingsStore, string, T> loadElement)
        {
            var loaded = new List<T>();
            int iItem = 0;
            while (true)
            {
                string itemPath = Path.Combine(collectionPath, string.Format("Item{0}", iItem++));
                if (!settingsStore.CollectionExists(itemPath)) break;
                loaded.Add(loadElement(settingsStore, itemPath));
            }
            return loaded;
        }

        public static void SetEnum<T>(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, T value) where T : struct
        {
            settingsStore.SetString(collectionPath, propertyName, value.ToString());
        }

        public static T GetEnum<T>(this WritableSettingsStore settingsStore, string collectionPath, string propertyName, T defaultValue) where T : struct
        {
            var strValue = settingsStore.GetString(collectionPath, propertyName, "");
            if (string.IsNullOrEmpty(strValue)) return default(T);
            T enumValue;
            if (!Enum.TryParse<T>(strValue, out enumValue)) return default(T);
            return enumValue;
        }
    }
}
