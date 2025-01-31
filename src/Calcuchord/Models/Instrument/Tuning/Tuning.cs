using System.Collections.Generic;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class Tuning {

        #region Properties

        #region Members

        string _id;

        public string Id {
            get => _id;
            set {
                if(_id == "c2a9ea0c-1e64-46fa-8b09-92f675b19917" || value == "c2a9ea0c-1e64-46fa-8b09-92f675b19917") {

                }

                _id = value;
            }
        }


        public string Name { get; set; }


        public int CapoFretNum { get; set; }


        public bool IsReadOnly { get; set; }


        public bool IsDefault { get; set; }


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


        public Tuning(string name,bool isDefault,bool isReadOnly,int capoNum = 0) : this() {
            Name = name;
            IsDefault = isDefault;
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