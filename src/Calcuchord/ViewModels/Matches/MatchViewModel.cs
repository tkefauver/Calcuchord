using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using MonkeyPaste.Common;

namespace Calcuchord {
    public partial class MatchViewModel : ViewModelBase {

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
            IsBookmarked ?
                "Bookmark" :
                "BookmarkOutline";

        public string PlaybackIcon =>
            IsMatchPlaying ?
                "Pause" :
                "Play";

        public string Label1 =>
            NotePattern.Key.ToDisplayValue();

        public string Label2 =>
            NotePattern.SuffixDisplayValue;

        public string Label3 =>
            NotePattern.Position == 0 ?
                string.Empty :
                NotePattern.Position.ToString();

        public string Label4 =>
            NotePattern.SubPosition == 0 ?
                string.Empty :
                NotePattern.SubPosition.ToString();


        public int DiagramColCount =>
            PatternType == MusicPatternType.Chords ?
                NotePattern.Parent.Parent.Parent.RowCount + 1 :
                PatternGen.PATTERN_FRET_SPAN + 2;

        public int DiagramRowCount =>
            PatternType == MusicPatternType.Chords ?
                PatternGen.PATTERN_FRET_SPAN + 2 :
                NotePattern.Parent.Parent.Parent.RowCount + 1;

        #endregion

        #region Layout

        #endregion

        #region State

        SvgSource _svgSource;

        public SvgSource SvgSource {
            get {

                if(_svgSource == null &&
                   PatternToSvgConverter.Instance.Convert(
                       NotePattern,typeof(string),"styled",CultureInfo.CurrentCulture) is string svg_xml) {
                    _svgSource = SvgSource.LoadFromSvg(svg_xml);
                }

                return _svgSource;
            }
        }

        public string ShareTitle =>
            NotePattern.FullName.Replace("#","Sharp").Replace(" ","_");

        public bool IsMatchPlaying { get; set; }

        public MusicPatternType PatternType { get; protected set; }

        public bool IsTranslateSourceMatch =>
            MainViewModel.Instance.TranslateSourceMatchViewModel == this;

        public bool IsSelected { get; set; }

        #endregion

        #region Model

        public bool IsBookmarked {
            get => NotePattern.IsBookmarked;
            set {
                if(IsBookmarked != value) {
                    NotePattern.IsBookmarked = value;
                    OnPropertyChanged();
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
            // _svgSource = null;
            // OnPropertyChanged(nameof(SvgSource));
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void PlayGroupMidi() {
            if(PlatformWrapper.Services is not { } ps ||
               ps.MidiPlayer is not { } mp ||
               NotePattern.GetToneGroups() is not { } tgl) {
                return;
            }


            Dispatcher.UIThread.Post(
                () => {
                    if(PatternType == MusicPatternType.Chords) {
                        mp.PlayChord(tgl);
                    } else {
                        mp.PlayScale(tgl);
                    }
                },DispatcherPriority.Background);
        }

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

        public MpIAsyncCommand SetMatchToInstrumentCommand => new MpAsyncCommand(
            async () => {
                if(MainViewModel.Instance is not { } mvm ||
                   mvm.SelectedTuning is not { } stvm) {
                    return;
                }

                if(!mvm.IsSearchModeSelected) {
                    // switch to search mode
                    mvm.SelectOptionCommand.Execute(mvm.SearchOptionViewModel);

                    // wait for inst to load...
                    await Task.Delay(500);
                }

                stvm.ResetSelection();
                await Task.Delay(300);

                foreach(NoteViewModel nvm in stvm.AllNotes.Where(x => x.IsRealNote)) {
                    if(NotePattern.Notes.FirstOrDefault(
                           x => x.RowNum == nvm.RowNum && Math.Max(0,x.ColNum) == nvm.NoteNum) is not
                       { } ng_match) {
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

        public ICommand RefingerChordCommand => new MpCommand(
            () => {

            });

        public ICommand SelectMatchCommand => new MpCommand(
            () => {
                if(MainViewModel.Instance is not { } mvm) {
                    return;
                }

                mvm.SelectedMatch = this;

                MatchesView.Instance.ScrollItemIntoView(this);
            });

        #endregion

    }
}