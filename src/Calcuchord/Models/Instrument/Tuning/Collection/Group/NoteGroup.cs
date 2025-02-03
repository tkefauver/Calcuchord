using System.Collections.Generic;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class NoteGroup : PrimaryModelBase {

        #region Properties

        #region Members

        public int Position { get; set; }


        public List<PatternNote> Notes { get; set; } = [];

        #endregion

        #region Ignored

        [JsonIgnore]
        public string SuffixKey =>
            Parent.SuffixKey;

        [JsonIgnore]
        public string SuffixDisplayValue =>
            Parent.SuffixDisplayValue;

        [JsonIgnore]
        public NoteGroupCollection Parent { get; private set; }

        [JsonIgnore]
        public NoteType Key =>
            Parent.Key;

        [JsonIgnore]
        public string Name =>
            $"{Key} {SuffixDisplayValue}";

        [JsonIgnore]
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