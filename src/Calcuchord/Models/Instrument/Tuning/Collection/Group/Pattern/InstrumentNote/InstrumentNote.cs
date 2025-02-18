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
            return Create(-1,stringNum,null);
        }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [JsonProperty]
        public int ColNum { get; set; }

        [JsonProperty]
        public int RowNum { get; set; }

        #endregion

        #region Ignored

        [JsonIgnore]
        public new InstrumentNote Next => Create(ColNum + 1,RowNum,base.Next);

        #endregion

        #endregion

        #region Constructors

        public static InstrumentNote Create(
            int colNum,
            int rowNum,
            Note? n) {
            return Create(
                colNum,
                rowNum,
                n?.Key,
                n?.Register);
        }

        public static InstrumentNote Create(
            int colNum,
            int rowNum,
            NoteType? nt,
            int? register) {
            InstrumentNote inn = new InstrumentNote
            {
                ColNum = colNum,
                RowNum = rowNum
            };
            if(nt is null || register is null) {
                inn.IsMute = true;
            } else {
                inn.Key = nt.Value;
                inn.Register = register.Value;
            }

            inn.NoteId = GetId(inn);
            return inn;
        }

        #endregion

        #region Public Methods

        public override void Adjust(int offset) {
            base.Adjust(offset);
            //ColNum -= offset;
        }

        // public new InstrumentNote Offset(int offset) {
        //     Note? offset_note = base.Offset(offset);
        //     return new InstrumentNote(ColNum + offset,RowNum,offset_note);
        // }

        public new InstrumentNote Clone() {
            return Create(ColNum,RowNum,Key,Register);
        }

        public override string ToString() {
            return $"[{RowNum}|{ColNum}] " + base.FullName;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}