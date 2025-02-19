using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public class MatchViewModel : ViewModelBase {

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

        public string Label1 =>
            NotePattern.Key.ToDisplayValue();

        public string Label2 =>
            NotePattern.SuffixDisplayValue;

        public string Label3 =>
            NotePattern.Position == 0 ? string.Empty : NotePattern.Position.ToString();

        public string Label4 =>
            NotePattern.SubPosition == 0 ? string.Empty : NotePattern.SubPosition.ToString();

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsMatchPlaying { get; set; }

        public MusicPatternType PatternType { get; protected set; }

        public bool IsSelected { get; set; }

        #endregion

        #region Model

        public bool IsBookmarked {
            get => NotePattern.IsBookmarked;
            set {
                if(IsBookmarked != value) {
                    NotePattern.IsBookmarked = value;
                    OnPropertyChanged(nameof(IsBookmarked));
                    OnPropertyChanged(nameof(BookmarkIcon));
                }

            }
        }

        public double Score { get; set; }

        public NotePattern NotePattern { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public MatchViewModel() {
        }

        public MatchViewModel(MusicPatternType patternType,NotePattern notePattern,double score) {
            PatternType = patternType;
            NotePattern = notePattern;
            Score = score;
        }

        #endregion

        #region Public Methods

        public void RefreshSvg() {
            OnPropertyChanged(nameof(NotePattern));
        }

        #endregion

        #region Protected Methods

        void PlayGroupMidi() {
            if(PlatformWrapper.Services is not { } ps ||
               ps.MidiPlayer is not { } mp) {
                return;
            }


            Dispatcher.UIThread.Post(
                () => {

                    if(PatternType == MusicPatternType.Chords) {
                        mp.PlayChord(NotePattern.Notes.ToArray());
                    } else {
                        mp.PlayScale(NotePattern.Notes.ToArray());
                    }
                },DispatcherPriority.Background);
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand ToggleBookmarkCommand => new MpCommand(
            async () => {
                IsBookmarked = !IsBookmarked;

                await Task.Delay(1_000);
                if(MainViewModel.Instance.SelectedDisplayMode == DisplayModeType.Bookmarks) {
                    MainViewModel.Instance.UpdateMatchesAsync(MatchUpdateSource.BookmarkToggle)
                        .FireAndForgetSafeAsync();
                }

                Prefs.Instance.Save();

            });

        public ICommand ToggleMatchPlaybackCommand => new MpCommand(
            () => {
                if(PlatformWrapper.Services is not { } ps ||
                   ps.MidiPlayer is not { } mp) {
                    return;
                }

#if DEBUG
                if(ThemeViewModel.Instance.IsDesktop &&
                   TopLevel.GetTopLevel(MainView.Instance) is { } tl &&
                   tl.Clipboard is { } cb &&
                   PatternToSvgConverter.Instance.Convert(NotePattern,null,"styled",null) is string svg) {
                    cb.SetTextAsync(svg.ToPrettyPrintXml()).FireAndForgetSafeAsync();
                }
#endif

                if(!mp.CanPlay) {
                    // TODO should probably show something here when can't play about local storage 

                    return;
                }

                PlayGroupMidi();
            });

        public ICommand SetMatchToInstrumentCommand => new MpCommand(
            async () => {
                if(MainViewModel.Instance is not { } mvm ||
                   mvm.SelectedTuning is not { } stvm) {
                    return;
                }

                if(!mvm.IsSearchModeSelected) {
                    // auto switch to search mode
                    mvm.SelectOptionCommand.Execute(mvm.SearchOptionViewModel);
                    await Task.Delay(500);
                }

                stvm.ResetSelection();
                await Task.Delay(300);

                foreach(NoteViewModel nvm in stvm.AllNotes.Where(x => x.IsRealNote)) {
                    if(NotePattern.Notes.FirstOrDefault(
                           x => x.RowNum == nvm.RowNum && Math.Max(0,x.ColNum) == nvm.NoteNum) is not { } ng_match) {
                        continue;
                    }

                    nvm.Parent.ToggleNoteSelectedCommand.Execute(nvm);
                    if(ng_match.IsMute) {
                        // toggle to mute
                        nvm.Parent.ToggleNoteSelectedCommand.Execute(nvm);
                    }
                }

                await Task.Delay(250);

                InstrumentView.Instance.ScrollSelectionIntoView();
            });

        public ICommand SelectMatchCommand => new MpCommand(
            () => {
                if(MainViewModel.Instance is not { } mvm) {
                    return;
                }

                mvm.SelectedMatch = this;

                if(MatchesView.Instance is not { } msv ||
                   msv.GetVisualDescendants<MatchView>().FirstOrDefault(x => x.DataContext == this) is not { } mv) {
                    return;
                }

                mv.BringIntoView();
            });

        #endregion

    }
}