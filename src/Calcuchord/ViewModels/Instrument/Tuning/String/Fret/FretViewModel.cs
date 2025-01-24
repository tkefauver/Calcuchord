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

        public string FretLabel {
            get {
                if(IsFretNumLabel) {
                    return FretNum.ToString();
                }
                if(IsStringKeyLabel) {
                    if(Parent.OpenNote == null) {
                        return string.Empty;
                    }
                    return Parent.OpenNote.FullName;
                }
                return FretNote.Name;
            }
        }


        public double StringHeight {
            get {
                if(!IsRealFret) {
                    return 0;
                }
                return (Parent.Parent.Parent.StringCount - StringNum) + 1;
            }
        }

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsEnabled =>
            StringNum >= 0 && FretNum >= 0;

        public bool IsSolid =>
            //Parent.Parent.Parent.InstrumentType == InstrumentType.Guitar &&
            StringNum >= 4;

        public bool IsRealFret =>
            IsEnabled && FretNum >= 0;

        public bool IsTopDotFret {
            get {
                bool desc = Parent.Parent.IsStringsDescending;
                double a = desc ? 3 : 2;
                double b = desc ? 2 : 1;
                double c = desc ? 4 : 3;

                if(StringNum == a &&
                   (FretNum == 3 ||
                    FretNum == 5 ||
                    FretNum == 7 ||
                    FretNum == 9 ||
                    FretNum == 15 ||
                    FretNum == 17 ||
                    FretNum == 19 ||
                    FretNum == 21)) {
                    return true;
                }
                if((StringNum == b || StringNum == c) && FretNum == 12) {
                    return true;
                }
                return false;
            }
        }

        public bool IsBottomDotFret {
            get {
                bool desc = Parent.Parent.IsStringsDescending;
                double a = desc ? 2 : 3;
                double b = desc ? 1 : 2;
                double c = desc ? 3 : 4;
                if(StringNum == a &&
                   (FretNum == 3 ||
                    FretNum == 5 ||
                    FretNum == 7 ||
                    FretNum == 9 ||
                    FretNum == 15 ||
                    FretNum == 17 ||
                    FretNum == 19 ||
                    FretNum == 21)) {
                    return true;
                }
                if((StringNum == b || StringNum == c) && FretNum == 12) {
                    return true;
                }
                return false;
            }
        }

        public bool IsInUnknownState { get; set; }
        public bool IsInMuteState { get; set; }

        public bool IsNutFret =>
            IsEnabled && FretNum == 0;

        public bool IsFretNumLabel =>
            Parent.StringNum == -1;

        public bool IsVisible {
            get {
                if(IsFretNumLabel) {
                    return FretNum >= 1;
                }
                if(IsStringKeyLabel) {
                    return Parent.StringNum >= 0;
                }
                return true;
            }
        }

        public bool IsStringKeyLabel =>
            FretNum == -1;

        public int WorkingFretNum {
            get {
                if(!IsNutFret) {
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
                if(IsNutFret) {
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

        public InstrumentNote FretNote { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public FretViewModel() {
        }

        public FretViewModel(StringRowViewModel parent,InstrumentNote fretNote) : base(parent) {
            Init(fretNote);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        public void Init(InstrumentNote fretNote) {
            FretNote = fretNote;

            OnPropertyChanged(nameof(FretNum));
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}