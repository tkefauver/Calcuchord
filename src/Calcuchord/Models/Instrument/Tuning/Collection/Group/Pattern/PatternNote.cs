using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class PatternNote : InstrumentNote {

        #region Properties

        #region Members

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
        public NoteGroup Parent { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public PatternNote() {
        }

        public PatternNote(int fingerNum,InstrumentNote inn) : this(
            fingerNum,inn.NoteNum,inn.RowNum,inn.Key,
            inn.Register) {
        }

        public PatternNote(int fingerNum,int noteNum,int rowNum,NoteType nt,int register) : base(
            noteNum,
            rowNum,nt,register) {
            FingerNum = fingerNum;
        }

        #endregion

        #region Public Methods

        public void SetParent(NoteGroup parent) {
            Parent = parent;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}