using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Calcuchord {
    [JsonObject]
    public class PatternKeyCollection {

        #region Properties

        #region Members

        [JsonProperty]
        public string SuffixKey { get; set; }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public NoteType Key { get; set; }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public MusicPatternType PatternType { get; set; }


        [JsonProperty]
        public List<NotePattern> Patterns { get; set; } = [];

        #endregion

        #region Ignored

        [JsonIgnore]
        int _suffixId = -1;

        [JsonIgnore]
        public int SuffixId {
            get {
                if(_suffixId < 0) {
                    switch(PatternType) {
                        case MusicPatternType.Chords:
                            _suffixId = (int)SuffixKey.ToEnum<ChordSuffixType>();
                            break;
                        case MusicPatternType.Scales:
                            _suffixId = (int)SuffixKey.ToEnum<ScaleSuffixType>();
                            break;
                        case MusicPatternType.Modes:
                            _suffixId = (int)SuffixKey.ToEnum<ModeSuffixType>();
                            break;
                    }
                }

                return _suffixId;
            }
        }

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

        public static PatternKeyCollection Create(MusicPatternType pt,NoteType key,string suffixKey) {
            return new PatternKeyCollection
            {
                PatternType = pt,
                Key = key,
                SuffixKey = suffixKey
            };
        }

        #endregion

        #region Public Methods

        public void SetParent(Tuning parent) {
            Parent = parent;
            foreach(NotePattern g in Patterns) {
                g.SetParent(this);
            }
        }

        public void SetPositions() {
            // set position to minimum root col
            if(Patterns.Where(x => x.Notes.None()) is { } empty_patterns &&
               empty_patterns.Any()) {

            }

            Patterns.ForEach(
                x => x.Position = x.Notes.Where(y => !y.IsMute && y.Key == Key).MinOrDefault(y => y.ColNum));
            // set subposition by minimum root row
            var pos_groups = Patterns.GroupBy(x => x.Position);
            foreach(var pos_group in pos_groups) {
                pos_group.OrderBy(x => x.Notes.Where(y => !y.IsMute && y.Key == Key).MinOrDefault(y => y.RowNum))
                    .ForEach((x,idx) => x.SubPosition = idx);
            }
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}