#nullable enable
using System.Runtime.Serialization;

namespace Calcuchord {
    [DataContract]
    public class InstrumentNote : Note {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        public static InstrumentNote Mute(int stringNum) {
            return new(-1,stringNum,null);
        }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [DataMember]
        public int NoteNum { get; set; }

        [DataMember]
        public int RowNum { get; set; }

        #endregion

        #region Ignored

        [IgnoreDataMember]
        public new InstrumentNote Next => new InstrumentNote(NoteNum + 1,RowNum,base.Next);

        #endregion

        #endregion

        #region Constructors

        public InstrumentNote() {
        }

        public InstrumentNote(int noteNum,int rowNum,Note? n) : this(noteNum,rowNum,n?.Key,n?.Register) {
        }

        public InstrumentNote(int noteNum,int rowNum,NoteType? nt,int? register) : base(nt,register) {
            NoteNum = noteNum;
            RowNum = rowNum;
        }

        #endregion

        #region Public Methods

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