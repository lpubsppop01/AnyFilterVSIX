using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace lpubsppop01.AnyTextFilterVSIX
{
    class JSONSettingsStoreAdapter : ISettingsStoreAdapter
    {
        #region Constructor

        Dictionary<string, object> rootNode;
        Dictionary<string, Dictionary<string, object>> pathToNode;

        public JSONSettingsStoreAdapter()
        {
            rootNode = new Dictionary<string, object>();
            pathToNode = new Dictionary<string, Dictionary<string, object>>();
        }

        #endregion

        #region Common Methods

        Dictionary<string, object> GetNode(string path, bool createsIfNotExists = true)
        {
            Dictionary<string, object> node;
            if (!pathToNode.TryGetValue(path, out node))
            {
                var tokens = path.Split('/', '\\');
                var currNode = rootNode;
                foreach (var token in tokens)
                {
                    object childValue;
                    if (!currNode.TryGetValue(token, out childValue))
                    {
                        if (!createsIfNotExists) return null;
                        currNode[token] = childValue = new Dictionary<string, object>();
                    }
                    currNode = childValue as Dictionary<string, object>;
                }
                pathToNode[path] = node = currNode;
            }
            return node;
        }

        static T GetValue<T>(Dictionary<string, object> node, string propetyName, T defaultValue)
        {
            object value;
            if (node.TryGetValue(propetyName, out value)) return (T)value;
            return defaultValue;
        }

        #endregion

        #region ISettingsStoreAdapter Members

        public int GetInt32(string collectionPath, string propertyName, int defaultValue = 0)
        {
            return GetValue(GetNode(collectionPath), propertyName, defaultValue);
        }

        public string GetString(string collectionPath, string propertyName, string defaultValue = "")
        {
            return GetValue(GetNode(collectionPath), propertyName, defaultValue);
        }

        public T GetEnum<T>(string collectionPath, string propertyName, T defaultValue = default(T)) where T : struct
        {
            return GetValue(GetNode(collectionPath), propertyName, defaultValue);
        }

        public bool GetBoolean(string collectionPath, string propertyName, bool defaultValue = false)
        {
            return GetValue(GetNode(collectionPath), propertyName, defaultValue);
        }

        public double? GetNullableDouble(string collectionPath, string propertyName, double? defaultValue = null)
        {
            return GetValue(GetNode(collectionPath), propertyName, defaultValue);
        }

        public IList<T> GetList<T>(string collectionPath, string propertyName, IList<T> defaultValue, Func<string, T> loadItem)
        {
            string listPath = Path.Combine(collectionPath, propertyName);
            var loaded = new List<T>();
            int iItem = 0;
            while (true)
            {
                string itemPath = Path.Combine(listPath, string.Format("Item{0}", iItem++));
                if (GetNode(itemPath, createsIfNotExists: false) == null) break;
                loaded.Add(loadItem(itemPath));
            }
            return loaded;
        }

        public void SetInt32(string collectionPath, string propertyName, int value)
        {
            GetNode(collectionPath)[propertyName] = value;
        }

        public void SetString(string collectionPath, string propertyName, string value)
        {
            GetNode(collectionPath)[propertyName] = value;
        }

        public void SetEnum<T>(string collectionPath, string propertyName, T value) where T : struct
        {
            GetNode(collectionPath)[propertyName] = value;
        }

        public void SetBoolean(string collectionPath, string propertyName, bool value)
        {
            GetNode(collectionPath)[propertyName] = value;
        }

        public void SetNullableDouble(string collectionPath, string propertyName, double? value)
        {
            GetNode(collectionPath)[propertyName] = value;
        }

        public void SetList<T>(string collectionPath, string propertyName, IList<T> value, Action<T, string> saveItem)
        {
            string listPath = Path.Combine(collectionPath, propertyName);
            GetNode(listPath).Clear();
            int iItem = 0;
            foreach (var item in value)
            {
                string itemPath = Path.Combine(listPath, string.Format("Item{0}", iItem++));
                saveItem(item, itemPath);
            }
        }

        #endregion

        #region Serialization

        public string Serialize()
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(rootNode);
        }

        public void Deserialize(string input)
        {
            var serializer = new JavaScriptSerializer();
            rootNode = serializer.Deserialize<Dictionary<string, object>>(input);
            pathToNode = new Dictionary<string, Dictionary<string, object>>();
        }

        #endregion
    }
}
