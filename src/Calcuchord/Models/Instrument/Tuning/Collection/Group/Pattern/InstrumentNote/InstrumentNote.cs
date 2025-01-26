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
        public int FretNum { get; set; }

        [DataMember]
        public int RowNum { get; set; }

        #endregion

        #region Ignored

        [IgnoreDataMember]
        public new InstrumentNote Next => new InstrumentNote(FretNum + 1,RowNum,base.Next);

        #endregion

        #endregion

        #region Constructors

        public InstrumentNote() {
        }

        public InstrumentNote(int fretNum,int rowNum,Note? n) : this(fretNum,rowNum,n?.Key,n?.Register) {
        }

        public InstrumentNote(int fretNum,int rowNum,NoteType? nt,int? register) : base(nt,register) {
            FretNum = fretNum;
            RowNum = rowNum;
        }

        #endregion

        #region Public Methods

        public new InstrumentNote Clone() {
            return new(FretNum,RowNum,Key,Register);
        }

        public override string ToString() {
            return $"[{RowNum}|{FretNum}] " + base.FullName;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}