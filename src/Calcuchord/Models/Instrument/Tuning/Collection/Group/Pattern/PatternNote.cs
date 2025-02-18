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

        public static PatternNote Create(
            int fingerNum,
            InstrumentNote inn) {
            PatternNote pn = new PatternNote
            {
                RowNum = inn.RowNum,
                ColNum = inn.ColNum,
                Key = inn.Key,
                Register = inn.Register,
                NoteId = inn.NoteId,
                IsMute = inn.IsMute,
                FingerNum = fingerNum
            };

            return pn;
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