using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using MonkeyPaste.Common;

namespace Calcuchord {
    public abstract class MatchViewModelBase : ViewModelBase {

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

        #endregion

        #region Appearance

        public string BookmarkIcon =>
            IsBookmarked ? "Bookmark" : "BookmarkOutline";

        public string PlaybackIcon =>
            IsMatchPlaying ? "Pause" : "Play";

        public string PrimaryLabel =>
            NoteGroup.Key.ToDisplayValue();

        public string SecondaryLabel =>
            NoteGroup.SuffixDisplayValue;

        public string TertiaryLabel =>
            (NoteGroup.Position + 1).ToString();

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsBookmarked {
            get => Prefs.Instance.BookmarkIds.Contains(NoteGroup.Id);
            set {
                if(IsBookmarked != value) {
                    if(value) {
                        Prefs.Instance.BookmarkIds.Add(NoteGroup.Id);
                    } else {
                        Prefs.Instance.BookmarkIds.Remove(NoteGroup.Id);
                    }

                    Prefs.Instance.Save();
                    OnPropertyChanged(nameof(IsBookmarked));
                    OnPropertyChanged(nameof(BookmarkIcon));
                }

            }
        }


        public bool IsMatchPlaying { get; set; }

        public abstract MusicPatternType MatchPatternType { get; }

        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }

        #endregion

        #region Model

        public double Score { get; set; }

        public NoteGroup NoteGroup { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        protected MatchViewModelBase() {
        }

        protected MatchViewModelBase(NoteGroup noteGroup,double score) {
            NoteGroup = noteGroup;
            Score = score;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        protected abstract void PlayGroupMidi();

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand ToggleBookmarkCommand => new MpCommand(() => {
            IsBookmarked = !IsBookmarked;
            if(MainViewModel.Instance.SelectedDisplayMode == DisplayModeType.Bookmarks) {
                MainViewModel.Instance.UpdateMatches(MatchUpdateSource.BookmarkToggle);
            }
        });

        public ICommand ToggleMatchPlaybackCommand => new MpCommand(
            () => {
                if(TopLevel.GetTopLevel(MainView.Instance) is { } tl &&
                   tl.Clipboard is { } cb &&
                   MatchToSvgConverter.Instance.Convert(NoteGroup,null,null,null) is string svg) {
                    cb.SetTextAsync(svg.ToPrettyPrintXml()).FireAndForgetSafeAsync();
                }

                if(!MidiPlayer.Instance.CanPlay) {
                    return;
                }

                if(IsMatchPlaying) {
                    MidiPlayer.Instance.StopPlayback();
                    return;
                }

                MidiPlayer.Instance.Stopped += MidiPlayer_OnStopped;

                PlayGroupMidi();

                void MidiPlayer_OnStopped(object sender,EventArgs e) {
                    IsMatchPlaying = false;
                    MidiPlayer.Instance.Stopped -= MidiPlayer_OnStopped;
                }
            });

        public ICommand SetMatchToInstrumentCommand => new MpCommand(
            () => {
                if(MainViewModel.Instance is not { } mvm ||
                   mvm.SelectedTuning is not { } stvm) {
                    return;
                }

                // select only notes in group, use open note for mutes
                stvm.SelectedNotes =
                    stvm.AllNotes.Where(x => x.IsRealNote &&
                                             NoteGroup.Notes.Any(y =>
                                                 x.NoteNum == (y.NoteNum == -1 ? 0 : y.NoteNum) &&
                                                 x.RowNum == y.RowNum))
                        .ToList();

                // set selected open notes to mute when group note is -1
                NoteGroup.Notes.Where(x => x.NoteNum < 0).ForEach(x =>
                    stvm.SelectedNotes.Where(y => y.NoteNum == 0 && y.RowNum == x.RowNum)
                        .ForEach(x => x.WorkingNoteNum = NoteViewModel.MUTE_FRET_NUM));

                InstrumentView.Instance.ScrollSelectionIntoView();
            });

        public ICommand SelectMatchCommand => new MpCommand(
            () => {
                if(MainViewModel.Instance is not { } mvm) {
                    return;
                }

                mvm.SelectedMatch = this;
            });

        #endregion

    }
}