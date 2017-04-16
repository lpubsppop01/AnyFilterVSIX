using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyFilterVSIX
{
    interface ISettingsStoreAdapter
    {
        Int32 GetInt32(string collectionPath, string propertyName, Int32 defaultValue = 0);
        string GetString(string collectionPath, string propertyName, string defaultValue = "");
        T GetEnum<T>(string collectionPath, string propertyName, T defaultValue = default(T)) where T : struct;
        bool GetBoolean(string collectionPath, string propertyName, bool defaultValue = false);
        double? GetNullableDouble(string collectionPath, string propertyName, double? defaultValue = null);
        IList<T> GetList<T>(string collectionPath, string propertyName, IList<T> defaultValue, Func<string, T> loadItem);

        void SetInt32(string collectionPath, string propertyName, Int32 value);
        void SetString(string collectionPath, string propertyName, string value);
        void SetEnum<T>(string collectionPath, string propertyName, T value) where T : struct;
        void SetBoolean(string collectionPath, string propertyName, bool value);
        void SetNullableDouble(string collectionPath, string propertyName, double? value);
        void SetList<T>(string collectionPath, string propertyName, IList<T> value, Action<T, string> saveItem);
    }
}
