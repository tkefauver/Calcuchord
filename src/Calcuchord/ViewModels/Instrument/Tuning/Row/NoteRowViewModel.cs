using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class NoteRowViewModel : ViewModelBase<TuningViewModel> {

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

        #endregion

        #region View Models

        public ObservableCollection<NoteViewModel> Notes { get; } = [];

        public NoteViewModel SelectedNote {
            get => SelectedNotes.FirstOrDefault();
            set {
                Notes.ForEach(x => x.IsSelected = x == value);
                OnPropertyChanged(nameof(SelectedNote));
                OnPropertyChanged(nameof(SelectedNotes));
            }
        }

        public IEnumerable<NoteViewModel> SelectedNotes {
            get => Notes.Where(x => x.IsSelected);
            set {
                Notes.ForEach(x => x.IsSelected = value == null ? false : value.Contains(x));
                OnPropertyChanged(nameof(SelectedNote));
                OnPropertyChanged(nameof(SelectedNotes));
            }
        }

        public NoteViewModel OpenNote =>
            Notes.FirstOrDefault(x => x.NoteNum == 0);


        NoteViewModel DefaultNote =>
            null; //!IsEnabled || IsKeyboard ? null : Notes.FirstOrDefault(x => x.NoteNum == 0);

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        bool IsEnabled =>
            RowNum >= 0;

        public bool IsMuted =>
            CanMute &&
            SelectedNote != null &&
            SelectedNote.IsInMuteState;

        public bool IsUnknown =>
            CanMute &&
            SelectedNote != null &&
            SelectedNote.IsInUnknownState;

        bool CanMute =>
            MainViewModel.Instance.SelectedPatternType == MusicPatternType.Chords;

        bool IsKeyboard =>
            Parent.Parent.IsKeyboard;

        bool IsSingleSelect =>
            !IsKeyboard && MainViewModel.Instance.SelectedPatternType == MusicPatternType.Chords;

        public bool IsDefaultSelection =>
            (DefaultNote == null && SelectedNote == null) ||
            (DefaultNote != null && SelectedNotes.All(x => x == DefaultNote));

        public int RowNum =>
            BaseNote == null ? -1 : BaseNote.RowNum;

        #endregion

        #region Model

        public InstrumentNote BaseNote { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public NoteRowViewModel(TuningViewModel parent,InstrumentNote baseNote) : base(parent) {
            PropertyChanged += NoteRowViewModel_OnPropertyChanged;
            BaseNote = baseNote;
            int min_fret_num = IsKeyboard ? 0 : -1;
            Notes.AddRange(
                Enumerable.Range(min_fret_num,Parent.LogicalFretCount)
                    .Select(x => CreateFretViewModel(x)));

            ResetSelection();
        }

        #endregion

        #region Public Methods

        public void ResetSelection() {
            if(DefaultNote is { } dn) {
                SelectedNotes = [dn];
            } else {
                SelectedNotes = [];
            }
        }

        public override string ToString() {
            return BaseNote == null ? base.ToString() : "Row " + BaseNote;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void NoteRowViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsMuted):
                case nameof(IsUnknown):
                    Notes.ForEach(x => x.OnPropertyChanged(nameof(x.IsRowMuted)));
                    Notes.ForEach(x => x.OnPropertyChanged(nameof(x.IsRowUnknown)));
                    Notes.ForEach(x => x.OnPropertyChanged(nameof(x.IsEnabled)));
                    break;
                case nameof(SelectedNote):
                case nameof(SelectedNotes):
                    MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.IsDefaultSelection));
                    break;
            }
        }

        NoteViewModel CreateFretViewModel(int fretNum) {
            InstrumentNote inn = new InstrumentNote(
                fretNum,RowNum,BaseNote == null ? null : BaseNote.Offset(fretNum));

            return new(this,inn);
        }

        NoteMarkerState GetNoteMarkerState(NoteViewModel nvm) {
            return
                nvm.IsDesiredRoot && nvm.IsSelected ?
                    NoteMarkerState.Root :
                    nvm.IsSelected ?
                        NoteMarkerState.On :
                        NoteMarkerState.Off;
        }

        void SetNoteMarkerState(NoteViewModel nvm,NoteMarkerState newState) {
            // cases:
            // off->on
            // off->root
            // on->off
            // on->root
            // root->on
            // root->off
            if(GetNoteMarkerState(nvm) == newState ||
               MainViewModel.Instance is not { } mvm) {
                return;
            }

            if(nvm.IsDesiredRoot) {
                mvm.DesiredRoot = null;
            } else if(newState == NoteMarkerState.Root) {
                mvm.DesiredRoot = nvm.InstrumentNote.Key;
            }

            if(newState == NoteMarkerState.Off) {
                nvm.IsSelected = false;
            } else {
                if(IsSingleSelect) {
                    SelectedNote = nvm;
                } else {
                    nvm.IsSelected = true;
                }
            }
        }

        void ToggleSelected(NoteViewModel nvm,bool root) {
            if(MainViewModel.Instance is not { } mvm) {
                return;
            }

            var last_sel = SelectedNotes.ToList();
            var last_root = mvm.DesiredRoot;
            NoteMarkerState cur_state = GetNoteMarkerState(nvm);
            NoteMarkerState next_state = cur_state;

            if(!root &&
               nvm.IsSelected &&
               nvm.IsNutFret &&
               CanMute) {
                nvm.WorkingNoteNum--;
                OnPropertyChanged(nameof(IsMuted));
                OnPropertyChanged(nameof(IsUnknown));
                if(nvm.WorkingNoteNum == 0) {
                    SetNoteMarkerState(nvm,NoteMarkerState.Off);
                }
            } else {
                switch(cur_state) {
                    case NoteMarkerState.Off:
                        next_state = root ? NoteMarkerState.Root : NoteMarkerState.On;
                        break;
                    case NoteMarkerState.On:
                        next_state = root ? NoteMarkerState.Root : NoteMarkerState.Off;
                        break;
                    case NoteMarkerState.Root:
                        next_state = root ? NoteMarkerState.On : NoteMarkerState.Off;
                        break;
                }

                SetNoteMarkerState(nvm,next_state);
            }

            if(SelectedNotes.Difference(last_sel).Any()) {
                Notes.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
            }

            if(mvm.DesiredRoot != null && Parent.AllNotes.All(x => !x.IsDesiredRoot)) {
                // remove desired root when nothing selected of key
                mvm.DesiredRoot = null;
            }

            if(root) {
                Parent.AllNotes.ForEach(x => x.OnPropertyChanged(nameof(x.IsDesiredRoot)));
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleNoteSelectedCommand => new MpCommand<object>(
            args => {
                if(args is not NoteViewModel nvm ||
                   !nvm.IsEnabled) {
                    return;
                }

                ToggleSelected(nvm,false);
                MainViewModel.Instance.UpdateMatches(MatchUpdateSource.NoteToggle);
            });

        public ICommand ToggleNoteAsDesiredRootCommand => new MpCommand<object>(
            args => {
                if(args is not NoteViewModel nvm ||
                   !nvm.IsEnabled) {
                    return;
                }

                ToggleSelected(nvm,true);
                MainViewModel.Instance.UpdateMatches(MatchUpdateSource.RootToggle);
            });

        #endregion

    }
}