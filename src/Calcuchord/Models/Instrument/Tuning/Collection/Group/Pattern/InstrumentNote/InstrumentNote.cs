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
        public override string FullName =>
            $"[{StringNum}|{FretNum}]" + base.FullName;

        #endregion

        #endregion

        #region Constructors

        public InstrumentNote() { }
        public InstrumentNote(int fretNum, int stringNum, Note n) : this(fretNum, stringNum, n.Key, n.Register) { }

        public InstrumentNote(int fretNum, int stringNum, NoteType nt, int register) : base(nt, register) {
            FretNum = fretNum;
            StringNum = stringNum;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion
    }
}