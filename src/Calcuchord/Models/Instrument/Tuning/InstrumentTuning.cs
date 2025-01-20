using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Calcuchord {
    [DataContract]
    public class InstrumentTuning {
        #region Properties

        #region Members

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int CapoFretNum { get; set; }

        [DataMember]
        public DateTime LastSelectedDt { get; set; } = DateTime.MinValue;

        [DataMember]
        public bool IsReadOnly { get; set; }

        [DataMember]
        public bool IsDefault { get; set; }

        [DataMember]
        public ObservableCollection<InstrumentNote> OpenNotes { get; set; } = [];

        [DataMember]
        public ObservableCollection<NoteGroupCollection> Chords { get; set; } = [];

        [DataMember]
        public ObservableCollection<NoteGroupCollection> Scales { get; set; } = [];

        [DataMember]
        public ObservableCollection<NoteGroupCollection> Modes { get; set; } = [];

        #endregion

        #region Ignored

        [IgnoreDataMember]
        Dictionary<MusicPatternType,ObservableCollection<NoteGroupCollection>> _collections;

        [IgnoreDataMember]
        public Dictionary<MusicPatternType,ObservableCollection<NoteGroupCollection>> Collections {
            get {
                if(_collections == null) {
                    _collections = new() {
                        { MusicPatternType.Chords,Chords },
                        { MusicPatternType.Scales,Scales },
                        { MusicPatternType.Modes,Modes }
                    };
                }

                return _collections;
            }
        }

        [IgnoreDataMember]
        public Instrument Parent { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public InstrumentTuning() {
            Id = Guid.NewGuid().ToString();
        }

        public InstrumentTuning(string name,bool isDefault,bool isReadOnly) {
            Name = name;
            IsDefault = isDefault;
            IsReadOnly = isReadOnly;
        }

        #endregion

        #region Public Methods

        public void SetParent(Instrument parent) {
            Parent = parent;

            foreach(var col_kvp in Collections) {
                foreach(var col in col_kvp.Value) {
                    col.SetParent(this);
                }
            }
        }

        public override string ToString() {
            return $"{Parent.InstrumentType} | {Name}";
        }

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion
    }
}