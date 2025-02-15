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

        public int BaseNoteNum =>
            Parent.CapoNum;

        bool IsEnabled =>
            RowNum >= 0;

        public bool IsMuted =>
            CanMute &&
            SelectedNote != null &&
            SelectedNote.IsInMuteState;

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
                    .Select(x => CreateNoteViewModel(x)));

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

            Notes.ForEach(x => x.Reset());
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
                    Notes.ForEach(x => x.OnPropertyChanged(nameof(x.IsRowMuted)));
                    Notes.ForEach(x => x.OnPropertyChanged(nameof(x.IsEnabled)));
                    break;
                case nameof(SelectedNote):
                case nameof(SelectedNotes):
                    MainViewModel.Instance.OnPropertyChanged(nameof(MainViewModel.Instance.IsDefaultSelection));
                    break;
            }
        }

        NoteViewModel CreateNoteViewModel(int fretNum) {
            InstrumentNote inn = null;
            // NOTE open note needs to be preserved so it maps 
            // to the tuning the model (for tuning or capo adjustment)
            // if(fretNum == BaseNoteNum &&
            //    BaseNote != null) {
            //     inn = BaseNote;
            // } else if(BaseNote != null) {
            //     inn = BaseNote.Offset(fretNum - BaseNoteNum);
            //     //
            // } else {
            //     inn = new InstrumentNote(fretNum - BaseNoteNum, RowNum, null);
            // }
            if(fretNum == 0 && BaseNote != null) {
                inn = BaseNote;
            } else {
                inn = new InstrumentNote(fretNum,RowNum,BaseNote?.Offset(fretNum));
            }

            return new NoteViewModel(this,inn);
        }

        NoteMarkerState GetNoteMarkerState(NoteViewModel nvm) {
            return
                nvm.IsDesiredRoot && nvm.IsSelected ?
                    NoteMarkerState.Root :
                    nvm.IsSelected ?
                        NoteMarkerState.On :
                        NoteMarkerState.Off;
        }

        void SetNoteMarkerState(NoteViewModel nvm,NoteMarkerState newState,bool root) {
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

            if(nvm.IsDesiredRoot &&
               (root || Parent.AllNotes.Where(x => x.IsSelected).None(x => x != nvm && x.IsDesiredRoot))) {
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
               nvm.IsOpenNote &&
               CanMute) {
                nvm.WorkingNoteNum--;
                OnPropertyChanged(nameof(IsMuted));
                if(nvm.WorkingNoteNum == 0) {
                    SetNoteMarkerState(nvm,NoteMarkerState.Off,root);
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

                SetNoteMarkerState(nvm,next_state,root);
            }

            if(SelectedNotes.Difference(last_sel).Any()) {
                Notes.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
            }

            bool was_root = false;
            if(mvm.DesiredRoot != last_root &&
               last_root != null &&
               Parent.AllNotes.Where(x => x.IsSelected).All(x => !x.IsDesiredRoot)) {
                // remove desired root when nothing selected of key
                was_root = true;
            }

            if(root || was_root) {
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
                MainViewModel.Instance.UpdateMatchesAsync(MatchUpdateSource.NoteToggle).FireAndForgetSafeAsync();
            });

        public ICommand ToggleNoteAsDesiredRootCommand => new MpCommand<object>(
            args => {
                if(args is not NoteViewModel nvm ||
                   !nvm.IsEnabled) {
                    return;
                }

                ToggleSelected(nvm,true);
                MainViewModel.Instance.UpdateMatchesAsync(MatchUpdateSource.RootToggle).FireAndForgetSafeAsync();
            });

        #endregion

    }
}