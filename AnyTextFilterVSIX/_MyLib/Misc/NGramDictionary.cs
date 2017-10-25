using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.AnyTextFilterVSIX
{
    public class NGramDictionary
    {
        #region Constructor

        int tokenSize;

        public NGramDictionary(int tokenSize)
        {
            this.tokenSize = tokenSize;
        }

        #endregion

        #region Data Class

        public class Document
        {
            #region Constructor

            public Document(Guid id, params Part[] parts)
            {
                ID = id;
                Parts = parts;
            }

            #endregion

            #region Properties

            public Guid ID { get; private set; }
            public Part[] Parts { get; private set; }

            #endregion
        }

        public class Part
        {
            #region Constructor

            public Part(byte kind, string text)
            {
                Kind = kind;
                Text = text;
            }

            #endregion

            #region Properties

            public byte Kind { get; private set; }
            public string Text { get; private set; }

            #endregion
        }

        public static readonly byte PartKind_Title = 1;
        public static readonly byte PartKind_Content = 2;

        public class Position
        {
            #region Constructor

            public Position(Guid docID, byte partKind, int offset)
            {
                DocumentID = docID;
                PartKind = partKind;
                Offset = offset;
            }

            #endregion

            #region Properties

            public Guid DocumentID { get; private set; }
            public byte PartKind { get; private set; }
            public int Offset { get; private set; }

            #endregion
        }

        #endregion

        #region Registering Methods

        Dictionary<string, List<Position>> tokenToPosList = new Dictionary<string, List<Position>>();
        Dictionary<Guid, Document> idToDoc = new Dictionary<Guid, Document>();

        public void AddDocument(Guid id, string title, string content)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            if (title == null || content == null) throw new ArgumentNullException();
            idToDoc[id] = new Document(id, new Part(PartKind_Title, title), new Part(PartKind_Content, content));
            AddDocumentPart(id, PartKind_Title, title);
            AddDocumentPart(id, PartKind_Content, content);
        }

        public void AddDocument(Document document)
        {
            if (document == null) throw new ArgumentNullException();
            if (document.ID == Guid.Empty || document.Parts == null) throw new ArgumentException();
            foreach (var part in document.Parts)
            {
                AddDocumentPart(document.ID, part.Kind, part.Text);
            }
        }

        void AddDocumentPart(Guid docID, byte partKind, string text)
        {
            int iCharEnd = text.Length - tokenSize + 1;
            for (int iChar = 0; iChar < iCharEnd; ++iChar)
            {
                string token = text.Substring(iChar, tokenSize).ToLower();
                List<Position> positions;
                if (!tokenToPosList.TryGetValue(token, out positions))
                {
                    tokenToPosList[token] = positions = new List<Position>();
                }
                positions.Add(new Position(docID, partKind, iChar));
            }
        }

        #endregion

        #region Search Methods

        public IEnumerable<Position> GetPositions(string keyword)
        {
            var chains = new List<List<Position>>();
            int iCharEnd = keyword.Length - tokenSize + 1;
            for (int iChar = 0; iChar < iCharEnd; ++iChar)
            {
                string token = keyword.Substring(iChar, tokenSize).ToLower();
                List<Position> positions;
                if (!tokenToPosList.TryGetValue(token, out positions)) yield break;
                if (iChar == 0)
                {
                    chains.AddRange(positions.Select(p => new List<Position> { p }));
                }
                else
                {
                    var chainsToRemove = new List<List<Position>>();
                    foreach (var posChain in chains)
                    {
                        var lastPos = posChain.Last();
                        var matched = positions.FirstOrDefault(p => p.DocumentID == lastPos.DocumentID && p.PartKind == lastPos.PartKind && p.Offset == lastPos.Offset + 1);
                        if (matched == null)
                        {
                            chainsToRemove.Add(posChain);
                        }
                        else
                        {
                            posChain.Add(matched);
                        }
                    }
                    foreach (var chain in chainsToRemove)
                    {
                        chains.Remove(chain);
                    }
                }
            }
            foreach (var pos in chains.Select(c => c.First()))
            {
                yield return pos;
            }
        }

        public ISet<string> GetWords(string keyword)
        {
            var words = new SortedSet<string>();
            var positions = GetPositions(keyword);
            foreach (var pos in positions)
            {
                Document doc;
                if (!idToDoc.TryGetValue(pos.DocumentID, out doc)) continue;
                var part = doc.Parts.FirstOrDefault(p => p.Kind == pos.PartKind);
                if (part == null) continue;
                string word = WordPicker.GetWord(part.Text, pos.Offset, keyword.Length);
                if (string.IsNullOrEmpty(word)) continue;
                words.Add(word);
            }
            return words;
        }

        #endregion
    }
}
