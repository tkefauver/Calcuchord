using System.Runtime.Serialization;

namespace Calcuchord {
    public class PatternNote : InstrumentNote {
        #region Properties

        #region Members

        [DataMember]
        public int FingerNum { get; set; }

        #endregion

        #region Ignored

        [IgnoreDataMember]
        public override string FullName =>
            $"F{FingerNum} " + base.FullName;

        [IgnoreDataMember]
        public bool IsRoot =>
            Key == Parent.Key;

        [IgnoreDataMember]
        public NoteGroup Parent { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public PatternNote() {
        }

        public PatternNote(int fingerNum,InstrumentNote inn) : this(
            fingerNum,inn.FretNum,inn.StringNum,inn.Key,
            inn.Register) {
        }

        public PatternNote(int fingerNum,int fretNum,int stringNum,NoteType nt,int register) : base(
            fretNum,
            stringNum,nt,register) {
            FingerNum = fingerNum;
        }

        #endregion

        #region Public Methods

        public void SetParent(NoteGroup parent) {
            Parent = parent;
        }

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion
    }
}