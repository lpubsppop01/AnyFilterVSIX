using System;
using lpubsppop01.AnyFilterVSIX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnyFilterVSIXTests
{
    [TestClass]
    public class MyTextEditTests
    {
        static readonly string NL = Environment.NewLine;
        static readonly string TestText = "HogeHoge" + NL + "PiyoPiyoPiyo1234" + NL + "Fuga" + NL + "Hogera";
        static readonly int iSecondLineStart = ("HogeHoge" + NL).Length;
        static readonly int iThridLineStart = ("HogeHoge" + NL + "PiyoPiyoPiyo1234" + NL).Length;
        static readonly int iForthLineStart = ("HogeHoge" + NL + "PiyoPiyoPiyo1234" + NL + "Fuga" + NL).Length;
        static readonly int FirstLineCount = "HogeHoge".Length;
        static readonly int SecondLineCount = "PiyoPiyoPiyo1234".Length;
        static readonly int ThirdLineCount = "Fuga".Length;
        static readonly int ForthLineCount = "Hogera".Length;
        const int ManyCount = 100;

        [TestMethod]
        public void TestForwardChar()
        {
            int iCaret = 0;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            for (int i = 0; i < FirstLineCount; ++i) textEdit.ForwardChar();
            Assert.AreEqual(FirstLineCount, iCaret);
            textEdit.ForwardChar();
            Assert.AreEqual(iSecondLineStart, iCaret);
            for (int i = 0; i < ManyCount; ++i) textEdit.ForwardChar();
            Assert.AreEqual(TestText.Length, iCaret);
        }

        [TestMethod]
        public void TestBackwardChar()
        {
            int iCaret = TestText.Length;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            for (int i = 0; i < ForthLineCount; ++i) textEdit.BackwardChar();
            Assert.AreEqual(iForthLineStart, iCaret);
            textEdit.BackwardChar();
            Assert.AreEqual(iThridLineStart + ThirdLineCount, iCaret);
            for (int i = 0; i < ManyCount; ++i) textEdit.BackwardChar();
            Assert.AreEqual(0, iCaret);
        }

        [TestMethod]
        public void TestMoveBeginningOfLine()
        {
            int iCaret = FirstLineCount;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            textEdit.MoveBeginningOfLine();
            Assert.AreEqual(0, iCaret);
            textEdit.MoveBeginningOfLine();
            Assert.AreEqual(0, iCaret);
            iCaret = iSecondLineStart + SecondLineCount;
            textEdit.MoveBeginningOfLine();
            Assert.AreEqual(iSecondLineStart, iCaret);
            textEdit.MoveBeginningOfLine();
            Assert.AreEqual(iSecondLineStart, iCaret);
        }

        [TestMethod]
        public void TestMoveEndOfLine()
        {
            int iCaret = iSecondLineStart;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            textEdit.MoveEndOfLine();
            Assert.AreEqual(iSecondLineStart + SecondLineCount, iCaret);
            textEdit.MoveEndOfLine();
            Assert.AreEqual(iSecondLineStart + SecondLineCount, iCaret);
            iCaret = iThridLineStart;
            textEdit.MoveEndOfLine();
            Assert.AreEqual(iThridLineStart + ThirdLineCount, iCaret);
            textEdit.MoveEndOfLine();
            Assert.AreEqual(iThridLineStart + ThirdLineCount, iCaret);
        }

        [TestMethod]
        public void TestNextLine()
        {
            int iCaret = 0;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            textEdit.MoveEndOfLine(); // set ColumnIndex
            textEdit.NextLine();
            Assert.AreEqual(iSecondLineStart + FirstLineCount, iCaret);
            textEdit.NextLine();
            Assert.AreEqual(iThridLineStart + ThirdLineCount, iCaret);
            textEdit.NextLine();
            Assert.AreEqual(iForthLineStart + ForthLineCount, iCaret);
        }

        [TestMethod]
        public void TestPreviousLine()
        {
            int iCaret = iForthLineStart + ForthLineCount;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            textEdit.MoveEndOfLine(); // set ColumnIndex
            textEdit.PreviousLine();
            Assert.AreEqual(iThridLineStart + ThirdLineCount, iCaret);
            textEdit.PreviousLine();
            Assert.AreEqual(iSecondLineStart + ForthLineCount, iCaret);
            textEdit.PreviousLine();
            Assert.AreEqual(ForthLineCount, iCaret);
        }

        [TestMethod]
        public void TestDeleteChar()
        {
            int iCaret = 0;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            for (int i = 0; i < FirstLineCount; ++i) textEdit.DeleteChar();
            Assert.IsTrue(textBuf.StartsWith(NL));
            textEdit.DeleteChar();
            Assert.IsTrue(textBuf.StartsWith("Piyo"));
            for (int i = 0; i < ManyCount; ++i) textEdit.DeleteChar();
            Assert.AreEqual(textBuf, "");
        }

        [TestMethod]
        public void TestDeleteBackwardChar()
        {
            int iCaret = TestText.Length;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            for (int i = 0; i < ForthLineCount; ++i) textEdit.DeleteBackwardChar();
            Assert.IsTrue(textBuf.EndsWith(NL));
            textEdit.DeleteBackwardChar();
            Assert.IsTrue(textBuf.EndsWith("Fuga"));
            for (int i = 0; i < ManyCount; ++i) textEdit.DeleteBackwardChar();
            Assert.AreEqual(textBuf, "");
        }

        [TestMethod]
        public void TestKillLine()
        {
            int iCaret = 0;
            var textBuf = TestText;
            var textEdit = new MyTextEdit(() => textBuf, (text) => textBuf = text, () => iCaret, (caretIndex) => iCaret = caretIndex);
            textEdit.KillLine();
            Assert.IsTrue(textBuf.StartsWith(NL));
            textEdit.KillLine();
            Assert.IsTrue(textBuf.StartsWith("Piyo"));
            for (int i = 0; i < ManyCount; ++i) textEdit.KillLine();
            Assert.AreEqual(textBuf, "");
        }
    }
}
