using System.Collections.Generic;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class NoteGroupCollection {

        #region Properties

        #region Members

        public string SuffixKey { get; set; }


        public NoteType Key { get; set; }


        public MusicPatternType PatternType { get; set; }


        public List<NoteGroup> Groups { get; set; } = [];

        #endregion

        #region Ignored

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
        public Tuning Parent { get; set; }

        #endregion

        #endregion

        #region Constructors

        public NoteGroupCollection() {
        }

        public NoteGroupCollection(MusicPatternType pt,NoteType key,string suffixKey) {
            PatternType = pt;
            Key = key;
            SuffixKey = suffixKey;
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