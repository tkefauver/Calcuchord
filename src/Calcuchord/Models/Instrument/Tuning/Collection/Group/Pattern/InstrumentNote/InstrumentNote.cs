using System.Runtime.Serialization;

namespace Calcuchord {
    [DataContract]
    public class InstrumentNote : Note {
        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        [DataMember]
        public int FretNum { get; set; }

        [DataMember]
        public int StringNum { get; set; }

        #endregion

        #region Ignored

        [IgnoreDataMember]
        public new InstrumentNote Next =>
            new(FretNum + 1,StringNum,base.Next);

        #endregion

        #endregion

        #region Constructors

        public InstrumentNote() {
        }

        public InstrumentNote(int fretNum,int stringNum,Note n) : this(fretNum,stringNum,n.Key,n.Register) {
        }

        public InstrumentNote(int fretNum,int stringNum,NoteType nt,int register) : base(nt,register) {
            FretNum = fretNum;
            StringNum = stringNum;
        }

        #endregion

        #region Public Methods

        public new InstrumentNote Clone() {
            return new(FretNum,StringNum,Key,Register);
        }

        public override string ToString() {
            return $"[{StringNum}|{FretNum}] " + base.FullName;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion
    }
}