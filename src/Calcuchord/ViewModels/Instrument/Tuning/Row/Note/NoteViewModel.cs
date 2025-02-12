using System.ComponentModel;
using System.Windows.Input;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class NoteViewModel : ViewModelBase<NoteRowViewModel> {

        #region Private Variables

        #endregion

        #region Constants

        public const int OPEN_FRET_NUM = 0;
        public const int MUTE_FRET_NUM = -1;
        //public const int UNKNOWN_FRET_NUM = -2;

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

        public double StringChunkHeight => (RowCount - Parent.Parent.PitchSortedRows.IndexOf(Parent)) + 1;

        public string MarkerLabel {
            get {
                if(IsNoteNumLabel) {
                    return NoteNum.ToString();
                }

                if(IsRowKeyLabel) {
                    if(Parent.BaseNote == null) {
                        return string.Empty;
                    }

                    return Parent.BaseNote.FullName;
                }

                if(IsOpenNote) {
                    if(IsInMuteState) {
                        return "M";
                    }
                }

                return InstrumentNote.Name;
            }
        }

        public string MarkerDetail {
            get {
                if(!IsRealNote) {
                    return string.Empty;
                }

                if(IsInMuteState) {
                    return "Muted";
                }

                return InstrumentNote.FullName;
            }
        }

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsFirstFret =>
            NoteNum == 1;

        public bool IsLastFret =>
            NoteNum == Parent.Parent.TotalFretCount;

        public bool IsTopFret =>
            RowNum == 0;

        public bool IsBottomFret =>
            RowNum == Parent.Parent.RowCount - 1;

        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }

        public bool IsDesiredRoot =>
            MainViewModel.Instance.DesiredRoot.HasValue &&
            MainViewModel.Instance.DesiredRoot.Value == InstrumentNote.Key;

        public bool IsEnabled =>
            RowNum >= 0 &&
            NoteNum >= 0;

        public bool IsRowSolid =>
            !IsRowNylon && RowNum >= 4;

        public bool IsRowNylon =>
            Parent.Parent.Parent.InstrumentType == InstrumentType.Ukulele;

        public bool IsRowMuted =>
            Parent.IsMuted;

        public bool IsRealNote =>
            IsEnabled && NoteNum >= 0;

        public bool IsInMuteState =>
            WorkingNoteNum == MUTE_FRET_NUM;

        public bool IsOpenNote =>
            !Parent.Parent.Parent.IsKeyboard &&
            RowNum >= 0 &&
            NoteNum == Parent.BaseNoteNum;

        public bool IsCapoFret =>
            !Parent.Parent.Parent.IsKeyboard &&
            RowNum >= 0 &&
            Parent.BaseNoteNum > 0 &&
            NoteNum == Parent.BaseNoteNum;

        public bool IsNutFret =>
            !Parent.Parent.Parent.IsKeyboard &&
            RowNum >= 0 &&
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

        public int WorkingNoteNum {
            get {
                if(_workingFretNum is { } num) {
                    return num;
                }

                return NoteNum + Parent.BaseNoteNum;
            }
            set => _workingFretNum = value < -1 || value == OPEN_FRET_NUM ? null : value;
        }

        #region Dot

        public bool IsTopDotFret {
            get {
                int single_top_str_num = 0,dbl_top_str_num1 = 0,dbl_top_str_num2 = 0;

                switch(Parent.Parent.Parent.RowCount) {
                    case 3:
                        single_top_str_num = 1;
                        dbl_top_str_num1 = 0;
                        dbl_top_str_num2 = 2;
                        break;
                    case 4:
                        single_top_str_num = 1;
                        dbl_top_str_num1 = 0;
                        dbl_top_str_num2 = 2;
                        break;
                    case 5:
                        single_top_str_num = 2;
                        dbl_top_str_num1 = 0;
                        dbl_top_str_num2 = 3;
                        break;
                    case 6:
                        single_top_str_num = 2;
                        dbl_top_str_num1 = 1;
                        dbl_top_str_num2 = 3;
                        break;
                    case 7:
                        single_top_str_num = 3;
                        dbl_top_str_num1 = 1;
                        dbl_top_str_num2 = 4;
                        break;
                }

                if(RowNum == single_top_str_num &&
                   NoteNum is 3 or 5 or 7 or 9 or 15 or 17 or 19 or 21) {
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
                int single_bottom_str_num = 0,dbl_bottom_str_num1 = 0,dbl_bottom_str_num2 = 0;
                switch(Parent.Parent.Parent.RowCount) {
                    case 3:
                        single_bottom_str_num = 1;
                        dbl_bottom_str_num1 = 0;
                        dbl_bottom_str_num2 = 2;
                        break;
                    case 4:
                        single_bottom_str_num = 2;
                        dbl_bottom_str_num1 = 1;
                        dbl_bottom_str_num2 = 3;
                        break;
                    case 5:
                        single_bottom_str_num = 2;
                        dbl_bottom_str_num1 = 1;
                        dbl_bottom_str_num2 = 4;
                        break;
                    case 6:
                        single_bottom_str_num = 3;
                        dbl_bottom_str_num1 = 2;
                        dbl_bottom_str_num2 = 4;
                        break;
                    case 7:
                        single_bottom_str_num = 3;
                        dbl_bottom_str_num1 = 2;
                        dbl_bottom_str_num2 = 5;
                        break;
                }

                if(RowNum == single_bottom_str_num &&
                   NoteNum is 3 or 5 or 7 or 9 or 15 or 17 or 19 or 21) {
                    return true;
                }

                if((RowNum == dbl_bottom_str_num1 || RowNum == dbl_bottom_str_num2) && NoteNum == 12) {
                    return true;
                }

                return false;
            }
        }

        public bool IsFullDot =>
            IsTopDotFret && IsBottomDotFret;

        #endregion

        #endregion

        #region Model

        public int NoteId =>
            InstrumentNote.NoteId;

        public bool IsAltered =>
            InstrumentNote.IsAltered;

        public int NoteNum =>
            InstrumentNote.NoteNum;

        public int RowNum =>
            InstrumentNote.RowNum;

        public int RowCount =>
            Parent.Parent.Parent.RowCount;

        public InstrumentNote InstrumentNote { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public NoteViewModel(NoteRowViewModel parent,InstrumentNote fretInstrumentNote) : base(parent) {
            PropertyChanged += NoteViewModel_OnPropertyChanged;
            InstrumentNote = fretInstrumentNote;
            OnPropertyChanged(nameof(NoteNum));
        }

        #endregion

        #region Public Methods

        public override string ToString() {
            return InstrumentNote.ToString();
        }

        public void Reset() {
            _workingFretNum = null;
            OnPropertyChanged(nameof(IsDesiredRoot));
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void NoteViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(!IsSelected) {
                        Reset();
                    }

                    Parent.OnPropertyChanged(nameof(Parent.SelectedNotes));

                    break;
            }
        }

        void ChangePitch(int deltaId) {
            InstrumentNote.Adjust(deltaId);

            OnPropertyChanged(nameof(MarkerLabel));
            OnPropertyChanged(nameof(MarkerDetail));
        }

        #endregion

        #region Commands

        public ICommand IncreasePitchCommand => new MpCommand(
            () => {
                ChangePitch(1);
            },() => {
                return NoteId < Note.MAX_NOTE_ID;
            });

        public ICommand DecreasePitchCommand => new MpCommand(
            () => {
                ChangePitch(-1);
            },() => {
                return NoteId > Note.MIN_NOTE_ID;
            });

        public ICommand SelectThisOpenNoteCommand => new MpCommand(
            () => {
                Parent.Parent.SelectedOpenNoteIndex = Parent.Parent.OpenNotes.IndexOf(this);
            });

        #endregion

    }

}