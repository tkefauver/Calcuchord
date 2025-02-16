using System.Diagnostics;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class PatternNote : InstrumentNote {

        #region Properties

        #region Members

        [JsonProperty]
        public int FingerNum { get; set; }

        #endregion

        #region Ignored

        [JsonIgnore]
        public override string FullName =>
            $"F{FingerNum} " + base.FullName;

        [JsonIgnore]
        public bool IsRoot =>
            Key == Parent.Key;

        [JsonIgnore]
        public NotePattern Parent { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public PatternNote() {
        }

        public PatternNote(
            int fingerNum,
            InstrumentNote inn) :
            base(
                inn.NoteNum,
                inn.RowNum,
                inn.Key,
                inn.Register) {
            Debug.Assert(NoteNum >= 0 == !IsMute,"Mute mismatch");
            FingerNum = fingerNum;
        }

        #endregion

        #region Public Methods

        public void SetParent(NotePattern parent) {
            Parent = parent;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}