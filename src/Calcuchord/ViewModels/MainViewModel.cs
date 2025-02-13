using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using DialogHostAvalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Newtonsoft.Json;
using AvSnackbarHost = Material.Styles.Controls.SnackbarHost;

namespace Calcuchord {
    public class MainViewModel : ViewModelBase {

        #region Private Variables

        readonly object _matchCreateLock = new object();

        string _editInstrumentInitialStateJson;

        #endregion

        #region Constants

        #endregion

        #region Statics

        public static MainViewModel Instance { get; private set; }

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        MatchProvider MatchProvider { get; set; }

        #endregion

        #region View Models

        #region Matches

        public ObservableCollection<MatchViewModelBase> Matches { get; } = [];
        List<MatchViewModelBase> WorkingMatches { get; } = [];
        IEnumerable<MatchViewModelBase> FilteredMatches { get; set; } = [];
        List<NoteViewModel> LastNotes { get; set; } = [];
        IEnumerable<MatchViewModelBase> LastMatches { get; } = [];
        List<OptionViewModel> LastOptions { get; } = [];

        public MatchViewModelBase SelectedMatch {
            get => Matches.FirstOrDefault(x => x.IsSelected);
            set {
                Matches.ForEach(x => x.IsSelected = value == x);
                OnPropertyChanged(nameof(SelectedMatch));
            }
        }

        #endregion

        #region Instrument

        public InstrumentViewModel EditModeInstrument { get; private set; }


        public ObservableCollection<InstrumentViewModel> Instruments { get; } = [];

        public InstrumentViewModel SelectedInstrument {
            get => Instruments.FirstOrDefault(x => x.IsSelected);
            set {
                if(SelectedInstrument != value) {
                    Instruments.ForEach(x => x.IsSelected = x == value);
                    OnPropertyChanged(nameof(SelectedInstrument));
                    OnPropertyChanged(nameof(SelectedTuning));
                }
            }
        }

        public TuningViewModel SelectedTuning =>
            SelectedInstrument == null ? null : SelectedInstrument.SelectedTuning;

        TuningViewModel LastSelectedTuning { get; set; }

        #endregion

        #region Options

        #region Display Mode

        public OptionViewModel SearchOptionViewModel =>
            DisplayModeOptions.FirstOrDefault(x => x.OptionValue == DisplayModeType.Search.ToString());

        public OptionViewModel BookmarksOptionViewModel =>
            DisplayModeOptions.FirstOrDefault(x => x.OptionValue == DisplayModeType.Bookmarks.ToString());

        public OptionViewModel IndexOptionViewModel =>
            DisplayModeOptions.FirstOrDefault(x => x.OptionValue == DisplayModeType.Index.ToString());

        public ObservableCollection<OptionViewModel> DisplayModeOptions =>
            OptionLookup[OptionType.DisplayMode];

        OptionViewModel SelectedDisplayModeOption =>
            DisplayModeOptions.FirstOrDefault(x => x.IsChecked);

        #endregion

        #region Pattern

        public ObservableCollection<OptionViewModel> PatternOptions =>
            OptionLookup[OptionType.Pattern];

        OptionViewModel SelectedPatternOption =>
            PatternOptions.FirstOrDefault(x => x.IsChecked);

        #endregion

        #region Key

        public ObservableCollection<OptionViewModel> KeyOptions =>
            OptionLookup[OptionType.Key];

        IEnumerable<OptionViewModel> SelectedKeyOptions =>
            KeyOptions.Where(x => x.IsChecked);

        #endregion

        #region Sort

        public ObservableCollection<OptionViewModel> SortOptions =>
            OptionLookup[CurSortOptionType];

        #endregion

        #region Svg

        public ObservableCollection<OptionViewModel> SvgOptions =>
            OptionLookup[CurSvgOptionType];

        #endregion

        #region Suffix

        public IEnumerable<OptionViewModel> VisibleSuffixOptions =>
            OptionLookup[CurSuffixOptionType].Where(x => x.IsEnabled);

        public IEnumerable<OptionViewModel> SuffixOptions =>
            OptionLookup[CurSuffixOptionType];

        IEnumerable<OptionViewModel> SelectedSuffixOptions =>
            SuffixOptions.Where(x => x.IsChecked);

        #endregion

        public Dictionary<OptionType,ObservableCollection<OptionViewModel>> OptionLookup { get; } =
            new Dictionary<OptionType,ObservableCollection<OptionViewModel>>
            {
                { OptionType.Key,[] },
                { OptionType.Pattern,[] },
                { OptionType.DisplayMode,[] },

                { OptionType.ChordSvg,[] },
                { OptionType.ScaleSvg,[] },
                { OptionType.ModeSvg,[] },

                { OptionType.ModeSuffix,[] },
                { OptionType.ChordSuffix,[] },
                { OptionType.ScaleSuffix,[] },

                { OptionType.ModeSort,[] },
                { OptionType.ChordSort,[] },
                { OptionType.ScaleSort,[] }
            };

        #endregion

        #endregion

        #region Appearance

        public string MatchSvgCss { get; private set; } = string.Empty;

        public string BusyText { get; set; }

        /*

           Classes.piano="{Binding IsPianoSelected}"
           Classes.no-inst="{Binding IsInstrumentsEmpty}"
           Classes.no-tuning="{Binding IsTuningsEmpty}"
           Classes.bookmark-mode="{Binding IsBookmarkModeSelected}"
           Classes.search-mode="{Binding IsSearchModeSelected}"
         */

        public string PatternName =>
            SelectedPatternType.ToString();

        public string PatternSingularName =>
            PatternName.Substring(0,PatternName.Length - 1).ToLower();

        #endregion

        #region Layout

        public double? RightDrawerExpandWidth =>
            null; //!ThemeViewModel.Instance.IsPhone && IsRightDrawerOpen ? 0 : null;

        public static double DefaultMatchWidth => 350;
        public double MatchWidth { get; set; } = DefaultMatchWidth;

        #endregion

        #region State

        #region UI

        public bool IsDoingIntro { get; private set; }
        public bool IsLoaded { get; private set; }

        public bool IsLeftDrawerOpen { get; set; }
        public bool IsRightDrawerOpen { get; set; }

        #endregion

        #region Options

        public bool IsSearchModeSelected =>
            SelectedDisplayMode == DisplayModeType.Search;

        public bool IsBookmarkModeSelected =>
            SelectedDisplayMode == DisplayModeType.Bookmarks;

        public bool IsIndexModeSelected =>
            SelectedDisplayMode == DisplayModeType.Index;

        OptionType CurSvgOptionType =>
            SelectedPatternType == MusicPatternType.Chords ?
                OptionType.ChordSvg :
                SelectedPatternType == MusicPatternType.Scales ?
                    OptionType.ScaleSvg :
                    OptionType.ModeSvg;

        OptionType CurSuffixOptionType =>
            SelectedPatternType == MusicPatternType.Chords ?
                OptionType.ChordSuffix :
                SelectedPatternType == MusicPatternType.Scales ?
                    OptionType.ScaleSuffix :
                    OptionType.ModeSuffix;

        OptionType CurSortOptionType =>
            SelectedPatternType == MusicPatternType.Chords ?
                OptionType.ChordSort :
                SelectedPatternType == MusicPatternType.Scales ?
                    OptionType.ScaleSort :
                    OptionType.ModeSort;

        IEnumerable<SvgOptionType> SelectedSvgOptionTypes =>
            SvgOptions.Where(x => x.IsChecked).Select(x => x.OptionValue.ToEnum<SvgOptionType>());

        public MusicPatternType SelectedPatternType =>
            SelectedPatternOption == null ? 0 : SelectedPatternOption.OptionValue.ToEnum<MusicPatternType>();

        public bool IsChordsSelected =>
            SelectedPatternType == MusicPatternType.Chords;

        public bool IsScalesSelected =>
            SelectedPatternType == MusicPatternType.Scales;

        public bool IsModesSelected =>
            SelectedPatternType == MusicPatternType.Modes;

        public DisplayModeType SelectedDisplayMode =>
            SelectedDisplayModeOption == null ? 0 : SelectedDisplayModeOption.OptionValue.ToEnum<DisplayModeType>();

        #endregion

        #region Instrument

        public bool IsInstrumentsEmpty =>
            Instruments.None();


        public bool IsTuningsEmpty =>
            SelectedInstrument != null &&
            SelectedInstrument.Tunings.None();

        public bool CanFinishEdit =>
            EditModeInstrument != null &&
            EditModeInstrument.Tunings.Any();

        public bool IsInstrumentVisible =>
            Instruments.Any() &&
            SelectedDisplayMode == DisplayModeType.Search &&
            EditModeInstrument == null;

        public int SelectedInstrumentIndex {
            get => Instruments.IndexOf(SelectedInstrument);
            set {
                if(value >= 0 && value < Instruments.Count) {
                    SelectedInstrument = Instruments[value];
                    OnPropertyChanged(nameof(SelectedInstrument));
                }
            }
        }

        public InstrumentType SelectedInstrumentType =>
            SelectedInstrument == null ? 0 : SelectedInstrument.Instrument.InstrumentType;

        public bool IsPianoSelected =>
            SelectedInstrumentType == InstrumentType.Piano;

        public NoteType? LastDesiredRoot { get; set; }
        public NoteType? DesiredRoot { get; set; }

        public bool IsDefaultSelection =>
            SelectedTuning == null ? true : SelectedTuning.NoteRows.All(x => x.IsDefaultSelection);

        #endregion

        #region Matches

        int MatchCount { get; set; }
        public int MatchColCount { get; private set; } = 1;

        public double MatchZoom { get; set; } = 1.0;
        IEnumerable<NoteType> AvailableKeys { get; set; } = [];

        IEnumerable<NoteType> SelectedKeys =>
            SelectedKeyOptions
                .Select(x => x.OptionValue.ToEnum<NoteType>())
                .Where(x => AvailableKeys.Contains(x));

        IEnumerable<string> AvailableSuffixes { get; set; } = [];

        IEnumerable<string> SelectedSuffixes =>
            SelectedSuffixOptions
                .Select(x => x.OptionValue)
                .Where(x => AvailableSuffixes.Contains(x));

        public bool CanIncreaseMatchColumnCount => !IsMatchZoomChanging && MatchColCount < MatchCount;
        public bool CanDecreaseMatchColumnCount => !IsMatchZoomChanging && MatchColCount > 1;
        public bool IsMatchZoomChanging { get; private set; }
        public bool IsLoadingMatches { get; private set; }
        public bool IsSearchInitiating { get; private set; }
        CancellationTokenSource MatchCts { get; set; }

        public bool IsMatchesEmpty =>
            !FilteredMatches.Any();

        public bool IsSearchButtonVisible {
            get {
                if(!IsBusy &&
                   !IsLoadingMatches &&
                   SelectedTuning != null &&
                   SelectedTuning.SelectedNotes.Any() &&
                   SelectedTuning.SelectedNotes.Difference(LastNotes).Any()) {
                    return true;
                }

                if(DesiredRoot != LastDesiredRoot) {
                    return true;
                }

                return false;
            }
        }

        public bool IsSearchOverlayVisible { get; private set; }

        #endregion

        #endregion

        #region Model

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public MainViewModel() {
            PropertyChanged += MainViewModel_OnPropertyChanged;
            Instance = this;
            InitAsync().FireAndForgetSafeAsync();
        }

        #endregion

        #region Public Methods

        public string GetUniqueInstrumentName(string desiredName,InstrumentViewModel[] ignored) {
            string unique_name = desiredName;
            var other_instl = Instruments.Where(x => !ignored.Contains(x));

            int suffix = 1;
            while(other_instl.Any(x => x.Name.ToLower() == unique_name.ToLower())) {
                unique_name = $"{desiredName}{suffix++}";
            }

            return unique_name;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void MainViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(EditModeInstrument):
                    if(EditModeInstrument is { } em_ivm) {
                        em_ivm.OnPropertyChanged(nameof(em_ivm.IsEditModeEnabled));
                    }

                    OnPropertyChanged(nameof(IsInstrumentVisible));

                    break;
                case nameof(IsRightDrawerOpen):
                    OnPropertyChanged(nameof(RightDrawerExpandWidth));
                    break;
                case nameof(SelectedTuning):
                    if(SelectedTuning == LastSelectedTuning) {
                        break;
                    }

                    LastSelectedTuning = SelectedTuning;
                    InitInstrument();
                    break;
                case nameof(SelectedInstrumentIndex):
                    OnPropertyChanged(nameof(SelectedInstrument));

                    break;
                case nameof(SelectedPatternType):
                    InitMatchProvider();
                    OnPropertyChanged(nameof(CurSvgOptionType));
                    OnPropertyChanged(nameof(SvgOptions));
                    OnPropertyChanged(nameof(IsChordsSelected));
                    OnPropertyChanged(nameof(IsScalesSelected));
                    OnPropertyChanged(nameof(IsModesSelected));

                    break;
                case nameof(SelectedDisplayMode):
                    UpdateMatchOverlays();
                    break;
                case nameof(IsLoadingMatches):
                    OptionLookup.Values.SelectMany(x => x).ForEach(x => x.OnPropertyChanged(nameof(x.IsEnabled)));
                    break;
                case nameof(IsSearchInitiating):
                    if(!IsSearchInitiating &&
                       MatchesView.Instance is { } matchesView &&
                       matchesView.MatchItemsRepeater is { } mir) {
                        // BUG items repeater overlaps items on load intermittently
                        mir.InvalidateMeasure();
                    }

                    break;
                case nameof(IsBusy):
                    if(!IsBusy) {
                        BusyText = string.Empty;
                    }

                    break;
            }
        }

        async Task InitAsync() {
            Prefs.Instance.IsSaveIgnored = true;

            if(Prefs.Instance.Instruments is not IEnumerable<Instrument> instl ||
               !instl.Any()) {
                DoIntroCommand.ExecuteAsync().FireAndForgetSafeAsync();
                return;
            }

            MatchColCount = Prefs.Instance.MatchColCount;

            foreach(Instrument inst in instl) {
                InstrumentViewModel ivm = await CreateInstrumentAsync(inst);
                Instruments.Add(ivm);
            }

            IsLoaded = true;
            InitInstrument();

            Prefs.Instance.IsSaveIgnored = false;
        }

        public MpIAsyncCommand DoIntroCommand => new MpAsyncCommand(
            async () => {
                IsDoingIntro = true;

                while(true) {
                    if(MainView.Instance is { } mv &&
                       mv.IsLoaded) {
                        break;
                    }

                    await Task.Delay(100);
                }

                // show welcome message
                await DialogHost.Show(new WelcomeView(),MainView.DialogHostName);

                // show inst builder
                await AddInstrumentCommand.ExecuteAsync();

                while(EditModeInstrument != null) {
                    await Task.Delay(100);
                }

                await Task.Delay(300);

                SelectedInstrument = Instruments.FirstOrDefault();

                IsDoingIntro = false;

                Prefs.Instance.IsSaveIgnored = false;
                Prefs.Instance.Save();

            });


        #region Matches

        public async Task UpdateMatchesAsync(MatchUpdateSource source) {
            if(
                SelectedTuning == null ||
                source is
                    MatchUpdateSource.NoteToggle or
                    MatchUpdateSource.RootToggle) {
                UpdateMatchOverlays();
                return;
            }

            await CancelMatchLoadAsync();

            //await Task.Run(
            //    () => {


            await Dispatcher.UIThread.InvokeAsync(
                async () => {
                    IsSearchOverlayVisible = false;

                    var sel_notes = SelectedTuning.SelectedNotes;
                    if(source is
                       MatchUpdateSource.FindClick or
                       MatchUpdateSource.InstrumentInit or
                       MatchUpdateSource.BookmarkToggle) {
                        lock(_matchCreateLock) {
                            CreateWorkingMatches(sel_notes);
                        }
                    }

                    IsSearchInitiating = true;
                    IsLoadingMatches = true;
                    LastDesiredRoot = DesiredRoot;
                    LastNotes = sel_notes.ToList();
                    FilteredMatches = GetFilteredWorkingMatches();

                    if(source is MatchUpdateSource.FindClick
                       or MatchUpdateSource.FilterToggle) {
                        string count_msg = $"{FilteredMatches.Count()} found";
                        AvSnackbarHost.Post(
                            count_msg,
                            null,
                            DispatcherPriority.Background);
                        await Task.Delay(300);
                    }

                    UpdateFilters();

                    await LoadMatchesAsync(
                        FilteredMatches,
                        MatchCts.Token);

                    if(MatchesView.Instance is { } mtv &&
                       mtv.MatchItemsRepeater is { } mir) {
                        // BUG 
                        //mir.InvalidateArrange();
                    }

                    IsLoadingMatches = false;
                    IsSearchOverlayVisible = true;
                },
                DispatcherPriority.ApplicationIdle,
                MatchCts.Token);
            //},MatchCts.Token);
        }

        void UpdateMatchOverlays() {
            OnPropertyChanged(nameof(PatternSingularName));

            OnPropertyChanged(nameof(IsSearchButtonVisible));
            OnPropertyChanged(nameof(IsPianoSelected));
            OnPropertyChanged(nameof(IsInstrumentsEmpty));
            OnPropertyChanged(nameof(IsTuningsEmpty));
            OnPropertyChanged(nameof(IsSearchModeSelected));
            OnPropertyChanged(nameof(IsBookmarkModeSelected));
            OnPropertyChanged(nameof(IsIndexModeSelected));
        }

        void UpdateFilters() {
            KeyOptions.ForEach(x => x.IsEnabled = AvailableKeys.Any(y => y.ToString() == x.OptionValue));
            SuffixOptions.ForEach(x => x.IsEnabled = AvailableSuffixes.Contains(x.OptionValue));

            OnPropertyChanged(nameof(KeyOptions));
            OnPropertyChanged(nameof(VisibleSuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));
            OnPropertyChanged(nameof(SortOptions));

            KeyOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            SuffixOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            SvgOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
        }

        async Task LoadMatchesAsync(IEnumerable<MatchViewModelBase> matches,CancellationToken ct) {
            Matches.Clear();
            MatchCount = 0;

            int init_count = Math.Max(1,MatchColCount * (MatchColCount - 1));
            int delay = ThemeViewModel.Instance.IsDesktop ? 5 : 50;

            foreach(MatchViewModelBase match in matches) {
                if(ct.IsCancellationRequested) {
                    return;
                }

                Matches.Add(match);
                MatchCount++;
                await Task.Delay(
                    delay,
                    ct);
                if(MatchCount >= init_count) {
                    IsSearchInitiating = false;
                }
            }

            IsSearchInitiating = false;
        }

        void CreateWorkingMatches(IEnumerable<NoteViewModel> notes) {
            WorkingMatches.Clear();
            switch(SelectedDisplayMode) {
                case DisplayModeType.Search:
                    WorkingMatches.AddRange(MatchProvider.GetMatches(notes));
                    break;
                case DisplayModeType.Bookmarks:
                    WorkingMatches.AddRange(MatchProvider.GetBookmarks());
                    break;
                case DisplayModeType.Index:
                    WorkingMatches.AddRange(MatchProvider.GetAll());
                    break;
            }

            AvailableKeys = WorkingMatches.Select(x => x.NoteGroup.Key).Distinct();
            AvailableSuffixes = WorkingMatches.Select(x => x.NoteGroup.SuffixKey).Distinct();
            SuffixOptions.ForEach(
                x => x.IsVisible = AvailableSuffixes.None() || AvailableSuffixes.Contains(x.OptionValue));
        }

        IEnumerable<MatchViewModelBase> GetFilteredWorkingMatches() {
            IEnumerable<MatchViewModelBase> result = WorkingMatches;
            if(DesiredRoot is { } dr) {
                result = result.Where(x => x.NoteGroup.Key == dr);
            } else if(SelectedKeys.Any()) {
                result = result.Where(x => SelectedKeys.Contains(x.NoteGroup.Key));
            }

            if(SelectedSuffixes.Any()) {
                result = result.Where(x => SelectedSuffixes.Contains(x.NoteGroup.SuffixDisplayValue));
            }

            return SortMatches(result);
        }

        IEnumerable<MatchViewModelBase> SortMatches(IEnumerable<MatchViewModelBase> matches) {
            (MatchSortType field,bool desc)[] sorts =
                SortOptions.Select(x => (x.OptionValue.ToEnum<MatchSortType>(),x.IsChecked)).ToArray();

            return matches
                .OrderBy(x => GetSortOptionValue(sorts[0],x))
                .ThenBy(x => GetSortOptionValue(sorts[1],x))
                .ThenBy(x => GetSortOptionValue(sorts[2],x));


            int GetSortOptionValue((MatchSortType field,bool desc) sort,MatchViewModelBase match) {
                int result = 0;
                switch(sort.field) {
                    case MatchSortType.Key:
                        result = (int)match.NoteGroup.Key;
                        break;
                    case MatchSortType.Suffix:
                        Type suffix_type =
                            SelectedPatternType == MusicPatternType.Chords ?
                                typeof(ChordSuffixType) :
                                SelectedPatternType == MusicPatternType.Scales ?
                                    typeof(ScaleSuffixType) :
                                    typeof(ModeSuffixType);
                        result = (int)match.NoteGroup.SuffixKey.ToEnum(suffix_type);
                        break;
                    case MatchSortType.Position:
                        result = match.NoteGroup.Position;
                        break;
                }

                return result * (sort.desc ? -1 : 1);
            }

        }

        async Task CancelMatchLoadAsync() {
            Dispatcher.UIThread.Post(
                () => {
                    IsSearchInitiating = false;
                    IsLoadingMatches = false;
                });

            if(MatchCts == null) {
                MatchCts = new CancellationTokenSource();
            } else {
                await MatchCts.CancelAsync();
                MatchCts.Dispose();
                MatchCts = new CancellationTokenSource();
            }


        }

        void InitMatchProvider() {
            if(MatchProvider != null &&
               SelectedTuning != null &&
               MatchProvider.PatternType == SelectedPatternType &&
               MatchProvider.Tuning == SelectedTuning.Tuning) {
                // already set
                return;
            }

            MatchProvider = new MatchProvider(
                SelectedPatternType,
                SelectedTuning == null ? null : SelectedTuning.Tuning);
        }

        public void DiscoverMatchColumnCount() {
            if(MatchesView.Instance is not { } mv ||
               mv.GetVisualDescendant<ItemsRepeater>() is not { } mir) {
                return;
            }

            double avail_w = mir.Bounds.Width;
            int cols = (int)Math.Max(
                1,
                avail_w / MatchWidth);
            MatchColCount = cols;
        }

        async Task SetMatchColumnCountAsync(int newColCount) {
            if(MatchesView.Instance is not { } mv ||
               mv.GetVisualDescendant<ItemsRepeater>() is not { } mir) {
                return;
            }

            double avail_w = mir.Bounds.Width;
            double new_match_w = avail_w / newColCount;
            if(!new_match_w.IsNumber() ||
               new_match_w <= 0) {
                return;
            }

            MatchColCount = newColCount;
            MatchWidth = new_match_w;

            await Task.Delay(20);
            while(!mir.IsArrangeValid) {
                await Task.Delay(100);
            }

            if(SelectedMatch is { } sel_mtvm &&
               mir.GetVisualDescendants<MatchView>().FirstOrDefault(x => x.DataContext == sel_mtvm) is { } sel_mv) {
                sel_mv.BringIntoView();
            }
        }

        #endregion

        #region Options

        public void UpdateMatchCss() {
            StringBuilder sb = new StringBuilder();

            if(SelectedSvgOptionTypes.Contains(SvgOptionType.Frets)) {
                sb.AppendLine(".fret-marker { display:none; }");
            } else {
                sb.AppendLine(".fret-labels { display:none; }");
            }

            if(SelectedSvgOptionTypes.Contains(SvgOptionType.Roots)) {
                sb.AppendLine(".root-open { stroke-width: 1.25; }");
                sb.AppendLine(".root-circle { display: none; }");
                if(SelectedInstrumentType == InstrumentType.Piano) {
                    // TODO should have better organization of these svg classes,
                    // this breaks conventions w/ chord svg
                    sb.AppendLine(".user-fill { display: none; }");
                }

            } else {
                sb.AppendLine(".root-box { display:none; }");
                sb.AppendLine(".root-open { stroke-width: 0.25; }");
            }

            if(!SelectedSvgOptionTypes.Contains(SvgOptionType.Tuning)) {
                sb.AppendLine(".string-tuning { display:none; }");
            }

            if(!SelectedSvgOptionTypes.Contains(SvgOptionType.Fingers)) {
                sb.AppendLine(".fingers-text { display:none; }");
            }

            if(!SelectedSvgOptionTypes.Contains(SvgOptionType.Colors)) {
                sb.AppendLine($".fingers-fill {{ fill: {ThemeViewModel.Instance.P[PaletteColorType.RootFretBg]}; }}");
            }

            if(!SelectedSvgOptionTypes.Contains(SvgOptionType.Matches)) {
                sb.AppendLine(".user-fill { fill: transparent; }");
            }

            if(!SelectedSvgOptionTypes.Contains(SvgOptionType.Notes)) {
                sb.AppendLine(".notes-text { display:none; }");
            }

            if(!SelectedSvgOptionTypes.Contains(SvgOptionType.Shadows)) {
                sb.AppendLine(".shadow-elm { display:none; }");
            }

            if(!SelectedSvgOptionTypes.Contains(SvgOptionType.Barres)) {
                sb.AppendLine(".barre-elm { display:none; }");
            }

            MatchSvgCss = sb.ToString();
        }

        public void ResetMatchSvg() {
            Matches.ForEach(x => x.RefreshSvg());
        }

        IEnumerable<OptionViewModel> CreateOptions() {
            var all_opts = new List<OptionViewModel>();
            var opt_lookup = new Dictionary<OptionType,(Type,int)>
            {
                { OptionType.Pattern,(typeof(MusicPatternType),0) },
                { OptionType.DisplayMode,(typeof(DisplayModeType),0) },
                { OptionType.ChordSuffix,(typeof(ChordSuffixType),-1) },
                { OptionType.ScaleSuffix,(typeof(ScaleSuffixType),-1) },
                { OptionType.ModeSuffix,(typeof(ModeSuffixType),-1) },
                { OptionType.Key,(typeof(NoteType),-1) },
                { OptionType.ChordSvg,(typeof(SvgOptionType),-1) },
                { OptionType.ScaleSvg,(typeof(SvgOptionType),-1) },
                { OptionType.ModeSvg,(typeof(SvgOptionType),-1) },
                { OptionType.ChordSort,(typeof(MatchSortType),-1) },
                { OptionType.ScaleSort,(typeof(MatchSortType),-1) },
                { OptionType.ModeSort,(typeof(MatchSortType),-1) }
            };

            string GetOptionLabel(OptionType opt,string key) {
                switch(opt) {
                    case OptionType.Key:
                        return key.ToEnum<NoteType>().ToDisplayValue();
                    case OptionType.ChordSuffix:
                        return MusicPatternType.Chords.ToDisplayValue(key);
                    case OptionType.ScaleSuffix:
                        return MusicPatternType.Scales.ToDisplayValue(key);
                    case OptionType.ModeSuffix:
                        return MusicPatternType.Modes.ToDisplayValue(key);
                    default:
                        return key;
                }
            }

            // create all options, labels and default values
            all_opts.AddRange(
                opt_lookup.SelectMany(
                    x =>
                        Enum.GetNames(x.Value.Item1).Select(
                            (y,idx) => new OptionViewModel
                            {
                                OptionType = x.Key,
                                OptionValue = y,
                                Label = GetOptionLabel(
                                    x.Key,
                                    y),
                                IsChecked = x.Value.Item2 == idx
                            })));

            // set all svg sets to default
            foreach((string key,int idx) in Enum.GetNames(typeof(SvgOptionType)).WithIndex()) {
                SvgOptionType optionType = (SvgOptionType)(int)Math.Pow(
                    2,
                    idx);
                if(SvgBuilderBase.DefaultSvgOptionType.HasFlag(optionType)) {
                    all_opts.Where(x => x.OptionValue == optionType.ToString()).ForEach(x => x.IsChecked = true);
                }
            }

            return all_opts;
        }

        void InitOptions() {
            var all_opts = Prefs.Instance.Options;
            if(all_opts.None()) {
                all_opts.AddRange(CreateOptions());
            }

            if(OptionLookup.Values.SelectMany(x => x).None()) {
                // create option lookup (on startup)
                foreach(var kvp in OptionLookup) {
                    kvp.Value.Clear();
                    kvp.Value.AddRange(all_opts.Where(x => x.OptionType == kvp.Key));
                }
            } else {
                // reset match filters
                OptionLookup.Where(x => x.Key.ToString().EndsWith("Suffix"))
                    .SelectMany(x => x.Value)
                    .ForEach(x => x.IsChecked = false);
            }

            SvgOptions.ForEach(
                x => x.IsEnabled = SelectedInstrument == null
                    ? false
                    : x.OptionValue.ToEnum<SvgOptionType>().IsFlagEnabled(
                        SelectedInstrument.InstrumentType,
                        SelectedPatternType,
                        SelectedDisplayMode));


            OnPropertyChanged(nameof(DisplayModeOptions));
            OnPropertyChanged(nameof(PatternOptions));
            OnPropertyChanged(nameof(KeyOptions));
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));

            OnPropertyChanged(nameof(SearchOptionViewModel));
            OnPropertyChanged(nameof(BookmarksOptionViewModel));
            OnPropertyChanged(nameof(IndexOptionViewModel));

            UpdateMatchCss();

            OptionLookup.SelectMany(x => x.Value).ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            OptionLookup.SelectMany(x => x.Value).ForEach(x => x.OnPropertyChanged(nameof(x.IsEnabled)));
        }

        #endregion

        #region Instruments

        async Task<InstrumentViewModel> CreateInstrumentAsync(Instrument instrument) {
            InstrumentViewModel ivm = new InstrumentViewModel(this);
            await ivm.InitAsync(instrument);
            return ivm;
        }

        void InitInstrument() {

            // Dispatcher.UIThread.Post(
            //     async () => {
            //         bool was_vis = IsSearchOverlayVisible;
            //         IsSearchOverlayVisible = false;
            //         await Task.Delay(2_000);
            //         IsSearchOverlayVisible = true;
            //     });
            LastSelectedTuning = SelectedTuning;
            if(IsIndexModeSelected) {
                DesiredRoot = null;
            }


            Debug.WriteLine("init instrument");


            if(SelectedTuning is { } sel_tvm) {
                sel_tvm.ResetSelection();
            }

            InitOptions();
            InitMatchProvider();

            OnPropertyChanged(nameof(SelectedPatternType));
            OnPropertyChanged(nameof(SelectedInstrument));
            OnPropertyChanged(nameof(SelectedTuning));
            OnPropertyChanged(nameof(IsInstrumentVisible));
            UpdateMatchOverlays();

            Dispatcher.UIThread.Post(
                async () => {
                    await Task.Delay(500);
                    await UpdateMatchesAsync(MatchUpdateSource.InstrumentInit);

                    if(MainView.Instance is { } mv) {
                        mv.RefreshMainGrid();
                    }

                    if(InstrumentView.Instance is { } iv) {
                        iv.MeasureInstrument();
                    }

                    if(MatchesView.Instance is { } mtv) {
                        // BUG setting busy here creates keeps it stuck on
                        // somehow, race condition maybe?
                        //IsBusy = true;
                        while(!mtv.MatchItemsRepeater.IsArrangeValid) {
                            await Task.Delay(100);
                        }

                        //IsBusy = false;
                        mtv.InvalidateArrange();
                    }

                });
        }

        public static IEnumerable<Instrument> CreateDefaultInstruments(InstrumentType[] def_itl = default) {
            def_itl = def_itl ?? [InstrumentType.Guitar,InstrumentType.Ukulele,InstrumentType.Piano];
            var instl = new List<Instrument>();
            foreach(InstrumentType def_it in def_itl) {
                Instrument def_inst = Instrument.CreateByType(
                    def_it,
                    true,
                    chordsFromFile: def_it != InstrumentType.Piano,
                    isInstrumentSelected: def_it == InstrumentType.Guitar);
                instl.Add(def_inst);
            }

            return instl;
        }

        #endregion

        #endregion

        #region Commands

        public ICommand ConvertMatchesCommand => new MpCommand<object>(
            async (args) => {
                await Task.Delay(1);
                if(MatchCount <= 0 ||
                   MatchColCount <= 0 ||
                   MatchesView.Instance is not { } mv ||
                   mv.MatchItemsRepeater.GetVisualDescendants<MatchView>().FirstOrDefault() is not { } sample_cp) {
                    return;
                }

                int cc = MatchColCount;
                int rc = (int)Math.Ceiling(MatchCount / (double)MatchColCount);
                double ar = sample_cp.Bounds.Height / sample_cp.Bounds.Width;
                double mw = 250;
                double mh = mw * ar;
                double tw = MatchColCount * mw;
                double th = rc * mh;

                ChordSvgBuilder cb = new ChordSvgBuilder();
                cb.Test(SelectedTuning.Tuning,Matches.Select(x => x.NoteGroup));
            });

        public MpIAsyncCommand CancelEditInstrumentCommand => new MpAsyncCommand(
            async () => {
                // close editor
                DialogHost.Close(MainView.DialogHostName);

                if(EditModeInstrument is not { } inst_to_restore_vm) {
                    _editInstrumentInitialStateJson = null;
                    return;
                }

                EditModeInstrument = null;
                if(string.IsNullOrEmpty(_editInstrumentInitialStateJson)) {
                    return;
                }

                Instrument inst_to_restore =
                    JsonConvert.DeserializeObject<Instrument>(_editInstrumentInitialStateJson);

                Prefs.Instance.Instruments.Remove(inst_to_restore_vm.Instrument);
                Prefs.Instance.Instruments.Add(inst_to_restore);

                await inst_to_restore_vm.InitAsync(inst_to_restore);
                Prefs.Instance.Save();

                InitInstrument();

                _editInstrumentInitialStateJson = null;
            });

        public MpIAsyncCommand FinishEditInstrumentCommand => new MpAsyncCommand(
            async () => {
                if(EditModeInstrument is not { } emi_vm) {
                    return;
                }

                // close inst editor
                DialogHost.Close(MainView.DialogHostName);

                if(IsDoingIntro) {
                    // show welcome2
                    await DialogHost.Show(new WelcomeView2(),MainView.DialogHostName);
                }

                _editInstrumentInitialStateJson = null;
                bool is_new = !emi_vm.IsActivated;
                if(is_new) {
                    // add new inst to list
                    Instruments.Add(emi_vm);
                }

                if(emi_vm.Tunings.Where(x => !x.IsLoaded) is { } new_tuning_vms &&
                   new_tuning_vms.Any()) {
                    // gen any new tuning patterns

                    foreach(TuningViewModel new_tuning_vm in new_tuning_vms) {
                        emi_vm.CurGenTuning = new_tuning_vm;
                        // show progress

                        DialogHost.Show(
                                new TuningGenProgressView { DataContext = new_tuning_vm },MainView.DialogHostName)
                            .FireAndForgetSafeAsync();

                        // wait a tid for dialog to init...
                        await Task.Delay(300);

                        bool success = await new_tuning_vm.InitAsync(new_tuning_vm.Tuning);
                        DialogHost.Close(MainView.DialogHostName);
                        if(!success) {
                            // gen was canceled, restore edit view
                            emi_vm.CurGenTuning = null;
                            return;
                        }
                    }

                    emi_vm.CurGenTuning = null;
                }

                Prefs.Instance.Save();
                EditModeInstrument = null;

                if(SelectedInstrument == emi_vm) {
                    InitInstrument();
                    return;
                }

                SelectedInstrument = emi_vm;

                if(is_new && SelectedDisplayMode != DisplayModeType.Index) {
                    SelectOptionCommand.Execute(IndexOptionViewModel);
                }
            });

        public MpIAsyncCommand<object> BeginEditInstrumentCommand => new MpAsyncCommand<object>(
            async args => {
                if(args is not InstrumentViewModel edit_inst_vm) {
                    if(args is not TuningViewModel tvm) {
                        return;
                    }

                    edit_inst_vm = tvm.Parent;
                }

                _editInstrumentInitialStateJson =
                    edit_inst_vm.IsActivated ? JsonConvert.SerializeObject(edit_inst_vm.Instrument) : null;

                if(edit_inst_vm.SelectedTuning == null) {
                    edit_inst_vm.SelectedTuning = edit_inst_vm.Tunings.FirstOrDefault();
                }

                EditModeInstrument = edit_inst_vm;
                if(EditModeInstrument.IsActivated) {
                    EditModeInstrument.IsInstrumentTabSelected = false;
                    EditModeInstrument.IsTuningTabSelected = true;
                } else {
                    EditModeInstrument.IsInstrumentTabSelected = true;
                    EditModeInstrument.IsTuningTabSelected = false;
                }

                await DialogHost.Show(new InstrumentEditorView { DataContext = edit_inst_vm },MainView.DialogHostName);
            });

        public MpIAsyncCommand AddInstrumentCommand => new MpAsyncCommand(
            async () => {
                EditModeInstrument = new InstrumentViewModel(this)
                {
                    Instrument = Instrument.CreateByType(InstrumentType.Guitar)
                };
                await EditModeInstrument.InitAsync(EditModeInstrument.Instrument);
                await BeginEditInstrumentCommand.ExecuteAsync(EditModeInstrument);
            });

        public ICommand DecreaseMatchColumnsCommand => new MpCommand(
            async () => {
                // plus btn
                IsMatchZoomChanging = true;
                await Task.Delay(500);
                await SetMatchColumnCountAsync(MatchColCount - 1);
                IsMatchZoomChanging = false;
            },
            () => {
                //return MatchColCount > 1;
                return CanDecreaseMatchColumnCount;
            });

        public ICommand IncreaseMatchColumnsCommand => new MpCommand(
            async () => {
                // minus btn
                IsMatchZoomChanging = true;
                await Task.Delay(500);
                await SetMatchColumnCountAsync(MatchColCount + 1);
                IsMatchZoomChanging = false;
            },
            () => {
                //return MatchColCount < Matches.Count;
                return CanIncreaseMatchColumnCount;
            });

        public ICommand ResetInstrumentCommand => new MpCommand(
            () => {
                DesiredRoot = null;
                SelectedTuning.ResetSelection();
                LastNotes.Clear();

                Matches.Clear();
                FilteredMatches = [];

                UpdateMatchOverlays();
                InstrumentView.Instance.ScrollSelectionIntoView();
            });

        public ICommand RemoveInstrumentCommand => new MpCommand<object>(
            (args) => {
                if(args is not InstrumentViewModel to_remove_ivm) {
                    return;
                }

                Instruments.Remove(to_remove_ivm);

                Prefs.Instance.Save();
                InitInstrument();
            });

        public ICommand CloseRightDrawerCommand => new MpCommand(
            () => {
                IsRightDrawerOpen = false;
            });

        public ICommand SelectOptionCommand => new MpCommand<object>(
            (args) => {

                if(args is not OptionViewModel ovm) {
                    if(args is object[] arg_parts &&
                       arg_parts.Any() &&
                       arg_parts[0] is OptionViewModel sort_ovm) {
                        ovm = sort_ovm;

                    } else {
                        return;
                    }
                }

                if(!ovm.OptionType.TryToEnum(out OptionType optionType)) {
                    return;
                }

                bool needs_save = true;

                switch(optionType) {
                    case OptionType.DisplayMode:
                    case OptionType.Pattern:
                        IsSearchOverlayVisible = false;
                        OptionLookup[optionType].ForEach(x => x.IsChecked = x == ovm);
                        InitInstrument();
                        break;
                    case OptionType.Key:
                    case OptionType.ChordSuffix:
                    case OptionType.ScaleSuffix:
                    case OptionType.ModeSuffix:
                        ovm.IsChecked = !ovm.IsChecked;
                        UpdateMatchesAsync(MatchUpdateSource.FilterToggle).FireAndForgetSafeAsync();
                        needs_save = false;
                        break;
                    case OptionType.ChordSvg:
                    case OptionType.ScaleSvg:
                    case OptionType.ModeSvg:
                        ovm.IsChecked = !ovm.IsChecked;
                        if(ovm.IsChecked &&
                           ovm.OptionValue.TryToEnum(out SvgOptionType flag) &&
                           (flag == SvgOptionType.Fingers || flag == SvgOptionType.Notes)) {
                            SvgOptionType otherOptionType = flag == SvgOptionType.Fingers
                                ? SvgOptionType.Notes
                                : SvgOptionType.Fingers;
                            if(SvgOptions.FirstOrDefault(x => x.OptionValue == otherOptionType.ToString()) is
                               { } other_ovm) {
                                other_ovm.IsChecked = false;
                            }
                        }

                        UpdateMatchCss();
                        break;
                    case OptionType.ChordSort:
                    case OptionType.ScaleSort:
                    case OptionType.ModeSort:
                        bool is_secondary = args is object[];
                        if(is_secondary) {
                            ovm.IsChecked = !ovm.IsChecked;
                        } else {
                            // shift sort opt to end of list
                            // int opt_idx = SortOptions.IndexOf(ovm);
                            //
                            // (string,bool,string) Swap(int from,int to,(string,bool,string)? force = null) {
                            //     string label = SortOptions[to].Label;
                            //     bool is_checked = SortOptions[to].IsChecked;
                            //     string ov = SortOptions[to].OptionValue;
                            //     SortOptions[to].Label = force == null ? SortOptions[from].Label : force.Value.Item1;
                            //     SortOptions[to].IsChecked =
                            //         force == null ? SortOptions[from].IsChecked : force.Value.Item2;
                            //     SortOptions[to].OptionValue =
                            //         force == null ? SortOptions[from].OptionValue : force.Value.Item3;
                            //     return (label,is_checked,ov);
                            // }
                            //
                            // (string,bool,string) temp = Swap(opt_idx,opt_idx);
                            // for(int i = opt_idx; i < 3; i++) {
                            //     if(i == 2) {
                            //         Swap(i,0,temp);
                            //     } else {
                            //         Swap(i,i + 1);
                            //     }
                            //
                            // }

                            SortOptions.Move(SortOptions.IndexOf(ovm),SortOptions.Count - 1);
                        }

                        UpdateMatchesAsync(MatchUpdateSource.SortToggle).FireAndForgetSafeAsync();
                        break;
                }

                if(needs_save) {
                    Prefs.Instance.Save();
                }


            });

        public ICommand FindMatchesCommand => new MpCommand(
            () => {
                UpdateMatchesAsync(MatchUpdateSource.FindClick).FireAndForgetSafeAsync();
            });

        #endregion

    }

}