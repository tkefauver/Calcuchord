#nullable enable
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class InstrumentNote : Note {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        public static InstrumentNote Mute(int stringNum) {
            return new InstrumentNote(-1,stringNum,null);
        }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [JsonProperty]
        public int NoteNum { get; set; }

        [JsonProperty]
        public int RowNum { get; set; }

        #endregion

        #region Ignored

        [JsonIgnore]
        public new InstrumentNote Next => new InstrumentNote(NoteNum + 1,RowNum,base.Next);
        // [JsonIgnore]
        // public InstrumentString Parent { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public InstrumentNote() {
        }

        public InstrumentNote(
            int noteNum,
            int rowNum,
            Note? n) :
            this(
                noteNum,
                rowNum,
                n?.Key,
                n?.Register) {
        }

        public InstrumentNote(
            int noteNum,
            int rowNum,
            NoteType? nt,
            int? register) :
            base(
                nt,
                register) {
            NoteNum = noteNum;
            RowNum = rowNum;
            IsMute = NoteNum < 0;
        }

        #endregion

        #region Public Methods

        public override void Adjust(int offset) {
            base.Adjust(offset);
            //NoteNum -= offset;
        }

        // public new InstrumentNote Offset(int offset) {
        //     Note? offset_note = base.Offset(offset);
        //     return new InstrumentNote(NoteNum + offset,RowNum,offset_note);
        // }

        public new InstrumentNote Clone() {
            return new(NoteNum,RowNum,Key,Register);
        }

        public override string ToString() {
            return $"[{RowNum}|{NoteNum}] " + base.FullName;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}