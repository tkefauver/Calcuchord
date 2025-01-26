using System.ComponentModel;

namespace Calcuchord {
    public class NoteViewModel : ViewModelBase<NoteRowViewModel> {

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

        public string MarkerLabel {
            get {
                if(IsNoteNumLabel) {
                    return NoteNum.ToString();
                }

                if(IsRowKeyLabel) {
                    if(Parent.OpenNote == null) {
                        return string.Empty;
                    }

                    return Parent.OpenNote.FullName;
                }

                if(IsNutFret) {
                    if(IsInMuteState) {
                        return "M";
                    }

                    if(IsInUnknownState) {
                        return "?";
                    }
                }

                return Note.Name;
            }
        }

        public string MarkerDetail {
            get {
                if(!IsRealNote) {
                    return string.Empty;
                }

                return Note.FullName;
            }
        }

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }

        public bool IsDesiredRoot =>
            MainViewModel.Instance.DesiredRoot.HasValue &&
            MainViewModel.Instance.DesiredRoot.Value == Note.Key;

        public bool IsEnabled =>
            RowNum >= 0 && NoteNum >= 0;

        public bool IsRowSolid =>
            RowNum >= 4;

        public bool IsRowMuted =>
            Parent.IsMuted;

        public bool IsRowUnknown =>
            Parent.IsUnknown;

        public bool IsRealNote =>
            IsEnabled && NoteNum >= 0;


        public bool IsInUnknownState =>
            WorkingFretNum == UNKNOWN_FRET_NUM;

        public bool IsInMuteState =>
            WorkingFretNum == MUTE_FRET_NUM;

        public bool IsNutFret =>
            !Parent.Parent.Parent.IsKeyboard &&
            IsEnabled &&
            NoteNum == 0;

        public bool IsNoteNumLabel =>
            Parent.RowNum == -1;

        public bool IsVisible {
            get {
                if(IsNoteNumLabel) {
                    return NoteNum >= 1;
                }

                if(IsRowKeyLabel) {
                    return Parent.RowNum >= 0;
                }

                return true;
            }
        }

        public bool IsRowKeyLabel =>
            NoteNum == -1;

        int? _workingFretNum;

        public int WorkingFretNum {
            get {
                if(_workingFretNum is { } num) {
                    return num;
                }

                return NoteNum;
            }
            set => _workingFretNum = value < -2 || value == OPEN_FRET_NUM ? null : value;
        }

        public bool IsDefaultState {
            get {
                if(IsNutFret) {
                    return WorkingFretNum == OPEN_FRET_NUM;
                }

                return true;
            }
        }

        #region Dot

        public bool IsTopDotFret {
            get {
                bool desc = Parent.Parent.IsStringsDescending;
                double single_top_str_num = 0,dbl_top_str_num1 = 0,dbl_top_str_num2 = 0;

                switch(Parent.Parent.Parent.StringCount) {
                    case 6:
                        single_top_str_num = desc ? 3 : 2;
                        dbl_top_str_num1 = desc ? 2 : 1;
                        dbl_top_str_num2 = desc ? 4 : 3;
                        break;
                    case 4:
                        single_top_str_num = desc ? 2 : 1;
                        dbl_top_str_num1 = desc ? 3 : 0;
                        dbl_top_str_num2 = desc ? 1 : 2;
                        break;
                    case 3:
                        single_top_str_num = desc ? 2 : 1;
                        dbl_top_str_num1 = desc ? 3 : 0;
                        dbl_top_str_num2 = desc ? 1 : 2;
                        break;
                }

                if(RowNum == single_top_str_num &&
                   (NoteNum == 3 ||
                    NoteNum == 5 ||
                    NoteNum == 7 ||
                    NoteNum == 9 ||
                    NoteNum == 15 ||
                    NoteNum == 17 ||
                    NoteNum == 19 ||
                    NoteNum == 21)) {
                    return true;
                }

                if((RowNum == dbl_top_str_num1 || RowNum == dbl_top_str_num2) && NoteNum == 12) {
                    return true;
                }

                return false;
            }
        }

        public bool IsBottomDotFret {
            get {
                bool desc = Parent.Parent.IsStringsDescending;
                double single_bottom_str_num = 0,dbl_bottom_str_num1 = 0,dbl_bottom_str_num2 = 0;
                switch(Parent.Parent.Parent.StringCount) {
                    case 6:
                        single_bottom_str_num = desc ? 2 : 3;
                        dbl_bottom_str_num1 = desc ? 1 : 2;
                        dbl_bottom_str_num2 = desc ? 3 : 4;
                        break;
                    case 4:
                        single_bottom_str_num = desc ? 1 : 2;
                        dbl_bottom_str_num1 = desc ? 2 : 1;
                        dbl_bottom_str_num2 = desc ? 0 : 3;
                        break;
                }

                if(RowNum == single_bottom_str_num &&
                   (NoteNum == 3 ||
                    NoteNum == 5 ||
                    NoteNum == 7 ||
                    NoteNum == 9 ||
                    NoteNum == 15 ||
                    NoteNum == 17 ||
                    NoteNum == 19 ||
                    NoteNum == 21)) {
                    return true;
                }

                if((RowNum == dbl_bottom_str_num1 || RowNum == dbl_bottom_str_num2) && NoteNum == 12) {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #endregion

        #region Model

        public bool IsAltered =>
            Note.IsAltered;

        public int NoteNum =>
            Note.FretNum;

        public int RowNum =>
            Note.RowNum;

        public int RowCount =>
            Parent.Parent.Parent.StringCount;

        public InstrumentNote Note { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public NoteViewModel() {
        }

        public NoteViewModel(NoteRowViewModel parent,InstrumentNote fretNote) : base(parent) {
            PropertyChanged += NoteViewModel_OnPropertyChanged;
            Note = fretNote;
            OnPropertyChanged(nameof(NoteNum));
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void NoteViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(!IsSelected) {
                        _workingFretNum = null;
                    }

                    break;
            }
        }

        #endregion

        #region Commands

        #endregion

    }

}