namespace Calcuchord {
    public class FretViewModel : ViewModelBase<StringRowViewModel> {

        #region Private Variables

        #endregion

        #region Constants

        public const int OPEN_FRET_NUM = 0;
        public const int MUTE_FRET_NUM = -1;
        public const int UNKNOWN_FRET_NUM = -2;

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        #endregion

        #region View Models

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsInUnknownState { get; set; }
        public bool IsInMuteState { get; set; }

        public bool IsOpenFret =>
            FretNum == 0;

        public int WorkingFretNum {
            get {
                if(!IsOpenFret) {
                    return FretNum;
                }
                if(IsInMuteState) {
                    return MUTE_FRET_NUM;
                }
                if(IsInUnknownState) {
                    return UNKNOWN_FRET_NUM;
                }
                return FretNum;
            }
        }

        public bool IsDefaultState {
            get {
                if(IsOpenFret) {
                    return WorkingFretNum == OPEN_FRET_NUM;
                }
                return true;
            }
        }

        public bool IsSelected { get; set; }

        #endregion

        #region Model

        public int FretNum =>
            FretNote.FretNum;

        public int StringNum =>
            FretNote.StringNum;

        public InstrumentNote FretNote { get; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public FretViewModel(StringRowViewModel parent,InstrumentNote fretNote) : base(parent) {
            FretNote = fretNote;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}