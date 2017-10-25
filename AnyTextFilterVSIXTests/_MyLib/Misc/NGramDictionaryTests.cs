using lpubsppop01.AnyTextFilterVSIX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.IO;

namespace DynamicAbbrevTests
{
    [TestClass]
    public class NGramDictionaryTests
    {
        [TestMethod]
        public void TestNGramDictionary()
        {
            var dict = new NGramDictionary(2);
            string title = "NGramDictionary.cs";
            string content = File.ReadAllText("../../../AnyTextFilterVSIX/_MyLib/Misc/NGramDictionary.cs");
            dict.AddDocument(Guid.NewGuid(), title, content);
            var positions = dict.GetPositions("word");
            Assert.IsTrue(positions.Any());
            var words = dict.GetWords("word");
            Assert.IsTrue(words.Any());
        }
    }
}
