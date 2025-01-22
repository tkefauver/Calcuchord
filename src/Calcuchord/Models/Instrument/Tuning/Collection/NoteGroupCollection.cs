using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Calcuchord {
    [DataContract]
    public class NoteGroupCollection {
        #region Properties

        #region Members

        [DataMember]
        public string Suffix { get; set; }

        [DataMember]
        public NoteType Key { get; set; }

        [DataMember]
        public MusicPatternType PatternType { get; set; }

        [DataMember]
        public ObservableCollection<NoteGroup> Groups { get; set; } = [];

        #endregion

        #region Ignored

        [IgnoreDataMember]
        public Tuning Parent { get; set; }

        #endregion

        #endregion

        #region Constructors

        public NoteGroupCollection() {
        }

        public NoteGroupCollection(MusicPatternType pt,NoteType key,string suffix) {
            PatternType = pt;
            Key = key;
            Suffix = suffix;
        }

        #endregion

        #region Public Methods

        public void SetParent(Tuning parent) {
            Parent = parent;
            foreach(NoteGroup g in Groups) {
                g.SetParent(this);
            }
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion
    }
}