using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Calcuchord {
    [DataContract]
    public class NoteGroup {
        #region Properties

        #region Members

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public ObservableCollection<PatternNote> Notes { get; set; } = [];

        #endregion

        #region Ignored

        [IgnoreDataMember]
        public string Suffix =>
            Parent.Suffix;

        [IgnoreDataMember]
        public NoteGroupCollection Parent { get; private set; }

        [IgnoreDataMember]
        public NoteType Key =>
            Parent.Key;

        #endregion

        #endregion

        #region Constructors

        public NoteGroup() {
            Id = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Methods

        public void SetParent(NoteGroupCollection parent) {
            Parent = parent;
            foreach(var pn in Notes) {
                pn.SetParent(this);
            }
        }

        public override string ToString() {
            return $"{Key} {Suffix} #{Position}";
        }

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion
    }
}