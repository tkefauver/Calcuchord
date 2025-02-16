using System.Collections.Generic;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class NotePattern : PrimaryModelBase {

        #region Properties

        #region Members

        [JsonProperty]
        public int Position { get; set; }

        [JsonProperty]
        public List<PatternNote> Notes { get; set; } = [];

        [JsonProperty]
        public bool IsBookmarked { get; set; }

        #endregion

        #region Ignored

        [JsonIgnore]
        public MusicPatternType PatternType =>
            Parent.PatternType;

        [JsonIgnore]
        public string SuffixKey =>
            Parent.SuffixKey;

        [JsonIgnore]
        string _suffixDisplayValue;

        [JsonIgnore]
        public string SuffixDisplayValue {
            get {
                if(string.IsNullOrEmpty(_suffixDisplayValue)) {
                    _suffixDisplayValue = PatternType.ToDisplayValue(SuffixKey);
                }

                return _suffixDisplayValue;
            }
        }

        [JsonIgnore]
        public PatternKeyCollection Parent { get; private set; }

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

        public NotePattern() {
        }

        public NotePattern(PatternKeyCollection ngc,int position) : this(ngc,position,[]) {
        }

        public NotePattern(PatternKeyCollection ngc,int position,IEnumerable<PatternNote> notes) : this() {
            Position = position;
            Notes.AddRange(notes);
            SetParent(ngc);
        }

        #endregion

        #region Public Methods

        public void SetParent(PatternKeyCollection parent) {
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