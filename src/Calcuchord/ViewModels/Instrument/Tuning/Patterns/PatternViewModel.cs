using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public class PatternViewModel : ViewModelBase<TuningViewModel> {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        static PatternViewModel PlayingMatch { get; set; }
        static bool IsAnyMatchPlaying => PlayingMatch != null;

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        #endregion

        #region View Models

        public new TuningViewModel Parent { get; set; }

        #endregion

        #region Appearance

        // string _svgSource;
        //
        // public string SvgSource {
        //     get {
        //         if(_svgSource == null) {
        //             _svgSource =
        //                 PatternToSvgConverter.Instance.Convert(
        //                     NotePattern,null,typeof(string),CultureInfo.CurrentCulture) as string;
        //         }
        //
        //         return _svgSource;
        //     }
        // }

        public string BookmarkIcon =>
            IsBookmarked ? "Bookmark" : "BookmarkOutline";

        public string PlaybackIcon =>
            IsMatchPlaying ? "Pause" : "Play";

        public string PrimaryLabel =>
            NotePattern.Key.ToDisplayValue();

        public string SecondaryLabel =>
            NotePattern.SuffixDisplayValue;

        public string TertiaryLabel =>
            (NotePattern.Position + 1).ToString();

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsMatchPlaying { get; set; }


        public bool IsSelected { get; set; }

        #endregion

        #region Model

        public bool IsBookmarked {
            get => NotePattern.IsBookmarked;
            set {
                if(IsBookmarked != value) {
                    NotePattern.IsBookmarked = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsBookmarked));
                    OnPropertyChanged(nameof(BookmarkIcon));
                }

            }
        }

        public MusicPatternType PatternType =>
            NotePattern.PatternType;

        double Score { get; set; }

        public NotePattern NotePattern { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        protected PatternViewModel() {
        }

        protected PatternViewModel(TuningViewModel parent) : base(parent) {

        }

        #endregion

        #region Public Methods

        public async Task InitAsync(NotePattern notePattern) {
            await Task.Delay(1);
            NotePattern = notePattern;
        }

        #endregion

        #region Protected Methods

        protected void PlayGroupMidi() {
            if(PlatformWrapper.Services is not { } ps ||
               ps.MidiPlayer is not { } mp) {
                return;
            }

            if(PatternType == MusicPatternType.Chords) {
                mp.PlayChord(NotePattern.Notes.ToArray());
            } else {
                mp.PlayScale(NotePattern.Notes.ToArray());
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand ToggleBookmarkCommand => new MpCommand(
            () => {
                IsBookmarked = !IsBookmarked;

                if(MainViewModel.Instance.SelectedDisplayMode == DisplayModeType.Bookmarks) {
                    MainViewModel.Instance.UpdateMatchesAsync(MatchUpdateSource.BookmarkToggle)
                        .FireAndForgetSafeAsync();
                }
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

                bool was_playing = IsMatchPlaying;
                if(IsAnyMatchPlaying) {
                    mp.StopPlayback();
                    if(was_playing) {
                        return;
                    }
                }

                PlayingMatch = this;
                mp.Stopped += MidiPlayer_OnStopped;

                PlayGroupMidi();

                void MidiPlayer_OnStopped(object sender,EventArgs e) {
                    IsMatchPlaying = false;
                    PlayingMatch = null;
                    mp.Stopped -= MidiPlayer_OnStopped;
                }
            });

        public ICommand SetMatchToInstrumentCommand => new MpCommand(
            () => {
                if(MainViewModel.Instance is not { } mvm ||
                   mvm.SelectedTuning is not { } stvm) {
                    return;
                }

                if(!mvm.IsSearchModeSelected) {
                    // auto switch to search mode
                    mvm.SelectOptionCommand.Execute(mvm.SearchOptionViewModel);
                    //await Task.Delay(300);
                }

                stvm.ResetSelection();
                //await Task.Delay(150);

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

                InstrumentView.Instance.ScrollSelectionIntoView();
            });

        public ICommand SelectMatchCommand => new MpCommand(
            () => {
                if(MainViewModel.Instance is not { } mvm) {
                    return;
                }

                //mvm.SelectedMatch = this;

                if(MatchesView.Instance is not { } msv ||
                   msv.GetVisualDescendants<MatchView>().FirstOrDefault(x => x.DataContext == this) is not { } mv) {
                    return;
                }

                mv.BringIntoView();
            });

        #endregion

    }
}