using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using MonkeyPaste.Common;

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
        public string SuffixKey =>
            Parent.SuffixKey;

        [IgnoreDataMember]
        public string SuffixDisplayValue =>
            Parent.SuffixDisplayValue;

        [IgnoreDataMember]
        public NoteGroupCollection Parent { get; private set; }

        [IgnoreDataMember]
        public NoteType Key =>
            Parent.Key;

        [IgnoreDataMember]
        public string Name =>
            $"{Key} {SuffixDisplayValue}";

        [IgnoreDataMember]
        public string FullName =>
            $"{Name} #{Position + 1}";

        #endregion

        #endregion

        #region Constructors

        public NoteGroup() {
        }

        public NoteGroup(NoteGroupCollection ngc,int position) : this(ngc,position,[]) {
        }

        public NoteGroup(NoteGroupCollection ngc,int position,IEnumerable<PatternNote> notes) : this() {
            Position = position;
            Notes.AddRange(notes);
            SetParent(ngc);
        }

        #endregion

        #region Public Methods

        public void SetParent(NoteGroupCollection parent) {
            Parent = parent;
            foreach(PatternNote pn in Notes) {
                pn.SetParent(this);
            }
        }

        public override string ToString() {
            return FullName;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}