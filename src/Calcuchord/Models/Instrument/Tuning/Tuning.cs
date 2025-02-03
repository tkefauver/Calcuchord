using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class Tuning : PrimaryModelBase {

        #region Properties

        #region Members

        public string Name { get; set; }


        public int CapoFretNum { get; set; }


        public bool IsReadOnly { get; set; }


        public List<InstrumentNote> OpenNotes { get; set; } = [];


        public List<NoteGroupCollection> Chords { get; set; } = [];


        public List<NoteGroupCollection> Scales { get; set; } = [];


        public List<NoteGroupCollection> Modes { get; set; } = [];

        #endregion

        #region Ignored

        [JsonIgnore]
        public int FretCount =>
            Parent.FretCount - CapoFretNum;

        [JsonIgnore]
        Dictionary<MusicPatternType,List<NoteGroupCollection>> _collections;

        [JsonIgnore]
        public Dictionary<MusicPatternType,List<NoteGroupCollection>> Collections {
            get {
                if(_collections == null) {
                    _collections = new()
                    {
                        { MusicPatternType.Chords,Chords },
                        { MusicPatternType.Scales,Scales },
                        { MusicPatternType.Modes,Modes }
                    };
                }

                return _collections;
            }
        }

        [JsonIgnore]
        public Instrument Parent { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public Tuning() {
        }


        public Tuning(string name,bool isReadOnly,int capoNum = 0) : this() {
            Name = name;
            IsReadOnly = isReadOnly;
            CapoFretNum = capoNum;
        }

        #endregion

        #region Public Methods

        public void SetParent(Instrument parent) {
            Parent = parent;

            foreach(var col_kvp in Collections) {
                foreach(NoteGroupCollection col in col_kvp.Value) {
                    col.SetParent(this);
                }
            }
        }


        public void ClearCollections() {
            Collections.ForEach(x => x.Value.Clear());
        }

        public Tuning Clone() {
            // NOTE is shallow clone only (no patterns or parent)
            Tuning clone = new Tuning(Name,IsReadOnly,CapoFretNum);
            clone.OpenNotes.Clear();
            clone.OpenNotes.AddRange(OpenNotes.Select(x => x.Clone()));
            clone.CreateId(null);
            return clone;
        }

        public override string ToString() {
            return $"{Parent.InstrumentType} | {Name}";
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}