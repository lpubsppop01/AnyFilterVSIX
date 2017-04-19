using System;
using lpubsppop01.AnyTextFilterVSIX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace lpubsppop01.AnyTextFilterVSIXTests
{
    [TestClass]
    public class JSONSettingsStoreAdapterTests
    {
        [TestMethod]
        public void TestSerializeDeserialize()
        {
            var adapter = new JSONSettingsStoreAdapter();
            var filters = new Filter[] { PresetFilters.Get(PresetFilterID.CygwinBash), PresetFilters.Get(PresetFilterID.Awk) };
            adapter.SetList("Test", "Filters", filters, (item, itemPath) => item.Save(adapter, itemPath));
            string serialized = adapter.Serialize();
            Assert.IsTrue(serialized != "");
            adapter.Deserialize(serialized);
            var deserializedFilters = adapter.GetList("Test", "Filters", new Filter[0], (itemPath) => Filter.Load(adapter, itemPath));
            Assert.AreEqual(filters.Length, deserializedFilters.Count);
            adapter.SetList("Test", "Filters", deserializedFilters, (item, itemPath) => item.Save(adapter, itemPath));
            string reSerialized = adapter.Serialize();
            Assert.AreEqual(serialized, reSerialized);
        }
    }
}
