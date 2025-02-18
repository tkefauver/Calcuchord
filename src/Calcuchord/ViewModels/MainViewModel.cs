using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
        IEnumerable<MatchViewModelBase> UnfilteredMatches { get; set; } = [];
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

        public TuningViewModel LastSelectedTuning { get; private set; }

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

        #endregion

        #region Sort

        public ObservableCollection<OptionViewModel> SortOptions =>
            OptionLookup[CurSortOptionType];

        public OptionViewModel SortOption1 =>
            SortOptions.Any() ? SortOptions[0] : null;

        public OptionViewModel SortOption2 =>
            SortOptions.Any() ? SortOptions[1] : null;

        public OptionViewModel SortOption3 =>
            SortOptions.Any() ? SortOptions[2] : null;

        #endregion

        #region Svg

        public ObservableCollection<OptionViewModel> SvgOptions =>
            OptionLookup[CurSvgOptionType];

        #endregion

        #region Suffix

        public IEnumerable<OptionViewModel> SuffixOptions =>
            OptionLookup[CurSuffixOptionType];

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

        public string MainDialogHostName => "MainDialogHost";
        public string InstEditDialogHostName => "InstrumentEditorPopupHost";

        public bool IsDoingIntro { get; private set; }
        public bool IsLoaded { get; private set; }

        public bool IsLeftDrawerOpen { get; set; }
        public bool IsRightDrawerOpen { get; set; }

        #endregion

        #region Options

        DisplayModeType LastDisplayMode { get; set; }
        MusicPatternType LastPatternType { get; set; }

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

        IEnumerable<SvgOptionType> LastSelectedSvgOptionTypes { get; set; } = [];

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
            SelectedTuning != null &&
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
        public int MatchColCount { get; private set; } = 3;

        IEnumerable<NoteType> AvailableKeys { get; set; } = [];

        IEnumerable<NoteType> LastSelectedKeys { get; } = [];

        IEnumerable<NoteType> SelectedKeys { get; set; } = [];
        // IEnumerable<NoteType> SelectedKeys =>
        //     SelectedKeyOptions
        //         .Select(x => x.OptionValue.ToEnum<NoteType>())
        //         .Where(x => AvailableKeys.Contains(x));

        bool IsAllKeysSelected =>
            KeyOptions.All(x => x.IsChecked && x.IsEnabled);

        IEnumerable<string> AvailableSuffixes { get; set; } = [];


        IEnumerable<string> LastSelectedSuffixes { get; } = [];

        IEnumerable<string> SelectedSuffixes { get; set; } = [];
        // IEnumerable<string> SelectedSuffixes =>
        //     SelectedSuffixOptions
        //         .Select(x => x.OptionValue)
        //         .Where(x => AvailableSuffixes.Contains(x));

        bool IsAllSuffixesSelected =>
            SuffixOptions.All(x => x.IsChecked && x.IsEnabled);

        IEnumerable<(MatchSortType,bool)> SelectedMatchSort =>
            SortOptions.Select(x => (x.OptionValue.ToEnum<MatchSortType>(),x.IsChecked));

        IEnumerable<(MatchSortType,bool)> LastMatchSort { get; set; } = [];

        public bool CanIncreaseMatchColumnCount => MatchColCount < MatchCount;
        public bool CanDecreaseMatchColumnCount => MatchColCount > 1;
        public bool IsMatchZoomChanging { get; private set; }
        public bool IsLoadingMatches { get; private set; }
        public bool IsSearchInitiating { get; private set; }
        CancellationTokenSource MatchCts { get; set; }
        public CancellationTokenSource ZoomCts { get; private set; }

        public bool IsMatchesEmpty =>
            !Matches.Any() && !IsSearchInitiating;

        public bool IsSearchButtonVisible {
            get {
                if(!IsBusy &&
                   !IsSearchInitiating &&
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
            Matches.CollectionChanged += MatchesOnCollectionChanged;
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
                case nameof(MatchColCount):
                    Prefs.Instance.Save();
                    break;
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
                    OnPropertyChanged(nameof(IsMatchesEmpty));
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
                case nameof(IsLoaded):
                    if(IsLoaded /* && Prefs.Instance.IsInitialStartup*/) {
                        AssetMover.MoveAllAssets();
                    }

                    break;
            }
        }

        void MatchesOnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsMatchesEmpty));
        }

        async Task InitAsync(IEnumerable<Instrument> instl = null) {
            // var blah = MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/def.json");
            // File.WriteAllText(
            //     blah.ToBase64String(),"home/tkefauver/dev/projects/Calcuchord/src/Calcuchord/Assets/Text/def.json");

            while(!Prefs.IsLoaded) {
                await Task.Delay(100);
            }

            instl = instl ?? Prefs.Instance.Instruments;

            ThemeViewModel.Instance.Init();

            Prefs.Instance.IsSaveIgnored = true;
            MatchColCount = Prefs.Instance.MatchColCount;

            if(!instl.Any()) {
                if(OperatingSystem.IsBrowser()) {
                    ResetToDefaultsCommand.Execute(null);
                    return;
                }

                DoIntroCommand.ExecuteAsync().FireAndForgetSafeAsync();
                return;
            }


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
                while(true) {
                    if(MainView.Instance is { } mv &&
                       mv.DlgHost is { } mdh &&
                       mdh.IsLoaded) {
                        break;
                    }

                    await Task.Delay(100);
                }

                await DialogHost.Show(new WelcomeView(),Instance.MainDialogHostName);

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
                IsLoaded = true;

            });


        #region Matches

        public async Task UpdateMatchesAsync(MatchUpdateSource source) {
            PlatformWrapper.Services.Logger.WriteLine($"Updating matches. Source: '{source}'");
            if(
                SelectedTuning == null ||
                source is
                    MatchUpdateSource.NoteToggle or
                    MatchUpdateSource.RootToggle) {
                UpdateMatchOverlays();
                IsSearchInitiating = false;
                return;
            }

            try {
                await CancelMatchLoadAsync();
                await Dispatcher.UIThread.InvokeAsync(
                    async () => {
                        IsLoadingMatches = true;

                        await LoadMatchesAsync2(source,MatchCts.Token);

                        IsLoadingMatches = false;
                    },
                    DispatcherPriority.ApplicationIdle,
                    MatchCts.Token);

            } catch(Exception) {
                // ignored
            }
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
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));
            OnPropertyChanged(nameof(SortOptions));

            KeyOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            SuffixOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            SvgOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
        }

        async Task LoadMatchesAsync2(MatchUpdateSource source,CancellationToken ct) {
            IsSearchInitiating = true;
            Matches.Clear();
            MatchCount = 0;

            int init_count = Math.Max(1,MatchColCount);
            int delay = ThemeViewModel.Instance.IsDesktop ? 5 : 50;

            var sel_notes = SelectedTuning.SelectedNotes.ToArray();
            LastNotes = sel_notes.ToList();
            LastDesiredRoot = DesiredRoot;

            IEnumerable<MatchViewModelBase> GetResults(bool byFilter) {
                foreach(var kvp in MatchProvider.PatternLookup) {
                    if((DesiredRoot != null && DesiredRoot.Value != kvp.Key) ||
                       (SelectedKeys.Any() && !SelectedKeys.Contains(kvp.Key))) {
                        // some key(s) selected, this isn't one
                        if(byFilter) {
                            continue;
                        }
                    }

                    foreach(var kvp2 in kvp.Value) {
                        if(SelectedSuffixes.Any() && !SelectedSuffixes.Contains(kvp2.Key)) {
                            // some suffix(es) selected, this isn't one
                            if(byFilter) {
                                continue;
                            }
                        }

                        foreach(MatchViewModelBase mvm in kvp2.Value) {
                            if(IsSearchModeSelected) {
                                mvm.Score = MatchProvider.GetScore(mvm.NotePattern,sel_notes);
                                if(mvm.Score > 0) {
                                    yield return mvm;
                                }

                                continue;
                            }

                            if(IsBookmarkModeSelected) {
                                if(mvm.IsBookmarked) {
                                    yield return mvm;
                                }

                                continue;
                            }

                            yield return mvm;
                        }
                    }
                }
            }

            if(source == MatchUpdateSource.FindClick ||
               source == MatchUpdateSource.InstrumentInit) {
                var unfiltered_results = GetResults(false);

                AvailableKeys = unfiltered_results.Select(x => x.NotePattern.Key).Distinct();
                AvailableSuffixes = unfiltered_results.Select(x => x.NotePattern.SuffixKey).Distinct();
                UpdateFilters();
            }

            var results = GetResults(true);
            if(source is MatchUpdateSource.FindClick
               or MatchUpdateSource.FilterToggle) {
                AvSnackbarHost.Post(
                    $"{results.Count():n0} found",
                    null,
                    DispatcherPriority.Background);
            }

            var sorted_results = SortMatches(results);

            foreach(MatchViewModelBase mvm in sorted_results) {
                Matches.Add(mvm);
                MatchCount++;
                if(MatchCount >= init_count) {
                    IsSearchInitiating = false;
                    delay = 150;
                }

                await Task.Delay(delay,ct);
            }

            IsSearchInitiating = false;
        }

        async Task LoadMatchesAsync(IEnumerable<MatchViewModelBase> matches,CancellationToken ct) {
            Matches.Clear();
            MatchCount = 0;
            //int init_count = Math.Max(1,MatchColCount * (MatchColCount - 1));
            int init_count = Math.Max(1,MatchColCount);
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
                    delay = 150;
                }
            }

            IsSearchInitiating = false;
        }

        IEnumerable<MatchViewModelBase> SortMatches(IEnumerable<MatchViewModelBase> matches) {
            var sorts = SelectedMatchSort.ToList();
            LastMatchSort = sorts;

            var result = matches
                .OrderBy(x => GetSortOptionValue(sorts[0],x))
                .ThenBy(x => GetSortOptionValue(sorts[1],x))
                .ThenBy(x => GetSortOptionValue(sorts[2],x));
            return result;


            int GetSortOptionValue((MatchSortType field,bool desc) sort,MatchViewModelBase match) {
                int score = 0;
                switch(sort.field) {
                    case MatchSortType.Key:
                        score = (int)match.NotePattern.Key;
                        break;
                    case MatchSortType.Suffix:
                        Type suffix_type =
                            SelectedPatternType == MusicPatternType.Chords ?
                                typeof(ChordSuffixType) :
                                SelectedPatternType == MusicPatternType.Scales ?
                                    typeof(ScaleSuffixType) :
                                    typeof(ModeSuffixType);
                        //score = (int)match.NotePattern.SuffixKey.ToEnum(suffix_type);

                        break;
                    case MatchSortType.Position:
                        score = match.NotePattern.Position;
                        break;
                }

                return score * (sort.desc ? -1 : 1);
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

        public async Task CancelMatchZoomAsync() {
            if(ZoomCts == null) {
                ZoomCts = new CancellationTokenSource();
            } else {
                await ZoomCts.CancelAsync();
                ZoomCts.Dispose();
                ZoomCts = new CancellationTokenSource();
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
               !mv.IsLoaded ||
               mv.GetVisualDescendant<ItemsRepeater>() is not { } mir) {
                return;
            }

            double avail_w = mir.Bounds.Width;
            int cols = (int)Math.Max(1,avail_w / MatchWidth);
            MatchColCount = Math.Max(1,Math.Min(Matches.Count,cols));
        }

        public async Task SetMatchColumnCountAsync(int newColCount,CancellationToken ct) {
            if(MatchesView.Instance is not { } mv ||
               mv.MatchItemsRepeater is not { } mir) {
                return;
            }

            if(MatchCount > 0 && !IsLoadingMatches) {
                newColCount = Math.Min(MatchCount,newColCount);
            }

            double avail_w = mir.Bounds.Width;
            double new_match_w = avail_w / newColCount;
            if(!new_match_w.IsNumber() ||
               new_match_w <= 0) {
                return;
            }

            MatchColCount = newColCount;
            MatchWidth = new_match_w;

            try {
                await Task.Delay(20,ct);
                while(!mir.IsArrangeValid) {
                    await Task.Delay(100,ct);
                }

                if(SelectedMatch is { } sel_mtvm &&
                   mir.GetVisualDescendants<MatchView>().FirstOrDefault(x => x.DataContext == sel_mtvm) is { } sel_mv) {
                    sel_mv.BringIntoView();
                }

                ZoomCts?.Dispose();
                ZoomCts = null;
            } catch {
                // canceled
            }
        }

        #endregion

        #region Options

        public void UpdateMatchCss() {
            LastSelectedSvgOptionTypes = SelectedSvgOptionTypes.ToList();

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
            all_opts
                .Where(x => x.OptionType.ToString().Contains("Svg"))
                .ForEach(
                    x =>
                        x.IsChecked = SvgBuilderBase
                            .DefaultSvgOptionType
                            .Any(y => y.ToString() == x.OptionValue.ToString()));


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
                // reset filters

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

            OnPropertyChanged(nameof(SortOption1));
            OnPropertyChanged(nameof(SortOption2));
            OnPropertyChanged(nameof(SortOption3));

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
            IsSearchInitiating = true;
            if(IsIndexModeSelected) {
                DesiredRoot = null;
            }

            //PlatformWrapper.Services.Logger.WriteLine("init instrument");


            if(SelectedTuning is { } sel_tvm) {
                sel_tvm.ResetSelection();
            }

            LastSelectedTuning = SelectedTuning;

            InitOptions();
            InitMatchProvider();

            OnPropertyChanged(nameof(SelectedPatternType));
            OnPropertyChanged(nameof(SelectedInstrument));
            OnPropertyChanged(nameof(SelectedTuning));
            OnPropertyChanged(nameof(IsInstrumentVisible));

            UpdateMatchOverlays();

            LastDisplayMode = SelectedDisplayMode;
            LastPatternType = SelectedPatternType;

            if(SelectedTuning is { } st) {
                st.OnPropertyChanged(nameof(st.IsSelected));
            }


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

        #endregion

        #endregion

        #region Commands

        public ICommand ConvertMatchesCommand => new MpCommand<object>(
            async (args) => {
                await Task.Delay(1);
                if(MatchCount <= 0 ||
                   MatchColCount <= 0) {
                    return;
                }

                // int cc = MatchColCount;
                // int rc = (int)Math.Ceiling(MatchCount / (double)MatchColCount);
                // double ar = sample_cp.Bounds.Height / sample_cp.Bounds.Width;
                // double mw = 250;
                // double mh = mw * ar;
                // double tw = MatchColCount * mw;
                // double th = rc * mh;

                if(args.ToStringOrEmpty() == "html") {
                    ChordSvgBuilder cb = new ChordSvgBuilder();
                    cb.Test(SelectedTuning.Tuning,Matches.Select(x => x.NotePattern));
                }
            });

        public MpIAsyncCommand CancelEditInstrumentCommand => new MpAsyncCommand(
            async () => {
                // close editor
                DialogHost.Close(Instance.MainDialogHostName);

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
                DialogHost.Close(MainDialogHostName);

                if(IsDoingIntro) {
                    // show welcome2
                    await DialogHost.Show(new WelcomeView2(),MainDialogHostName);
                }

                await Task.Delay(300);

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


                        // wait a tid for dialog to init...

                        bool success = await new_tuning_vm.InitAsync(new_tuning_vm.Tuning);
                        emi_vm.CurGenTuning = null;
                        if(!success) {
                            // gen was canceled, restore edit view\
                            return;
                        }
                    }
                }

                Prefs.Instance.Save();

                EditModeInstrument = null;
                SelectedInstrument = emi_vm;
                SelectedInstrument.OnPropertyChanged(nameof(SelectedInstrument.SelectedTuning));
                OnPropertyChanged(nameof(SelectedTuning));
                InitInstrument();

                // if(is_new && SelectedDisplayMode != DisplayModeType.Index) {
                //     SelectOptionCommand.Execute(IndexOptionViewModel);
                // }
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

                await DialogHost.Show(
                    new InstrumentEditorView { DataContext = edit_inst_vm },Instance.MainDialogHostName);
            });

        public MpIAsyncCommand AddInstrumentCommand => new MpAsyncCommand(
            async () => {
                if(OperatingSystem.IsBrowser()) {
                    ResetToDefaultsCommand.Execute(null);
                    return;
                }

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
                await CancelMatchZoomAsync();
                await Task.Delay(500);
                await SetMatchColumnCountAsync(MatchColCount - 1,ZoomCts.Token);
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
                await CancelMatchZoomAsync();
                await Task.Delay(500);
                await SetMatchColumnCountAsync(MatchColCount + 1,ZoomCts.Token);
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

                UpdateMatchOverlays();
                InstrumentView.Instance.ScrollSelectionIntoView();
            });

        public ICommand RemoveInstrumentCommand => new MpCommand<object>(
            async (args) => {
                if(args is not InstrumentViewModel to_remove_ivm) {
                    return;
                }

                bool? confirmed = null;
                YesNoDialogView dlg_v = new YesNoDialogView
                {
                    DataContext = new DialogViewModel
                    {
                        Label = $"Are you sure you want to delete '{to_remove_ivm.Name}'?",
                        OkCommand = new MpCommand(
                            () => {
                                confirmed = true;
                            }),
                        CancelCommand = new MpCommand(
                            () => {
                                confirmed = false;
                            })
                    }
                };
                DialogHost.Show(dlg_v,Instance.MainDialogHostName).FireAndForgetSafeAsync();

                while(!confirmed.HasValue) {
                    await Task.Delay(100);
                }

                DialogHost.Close(Instance.MainDialogHostName);

                if(!confirmed.Value) {
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

        public ICommand ShowAboutCommand => new MpCommand(
            () => {
                if(TopLevel.GetTopLevel(MainView.Instance) is not { } tl) {
                    return;
                }

                void TopLevel_OnPointerPressed(object sender2,PointerPressedEventArgs e2) {
                    if(e2.Source is Control c && c.GetSelfAndVisualAncestors().OfType<AboutView>().Any()) {
                        // allow  about view click
                        return;
                    }

                    e2.Handled = true;

                    tl.RemoveHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed);
                    //tl.RemoveHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed);
                    try {
                        DialogHost.Close(Instance.MainDialogHostName);
                    } catch(Exception ex) {
                        ex.Dump();
                    }
                }

                tl.AddHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed,RoutingStrategies.Tunnel,true);
                //tl.AddHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed,RoutingStrategies.Bubble,true);
                DialogHost.Show(
                    new AboutView
                    {
                        DataContext = new AboutViewModel()
                    },Instance.MainDialogHostName).FireAndForgetSafeAsync();

            });

        DateTime? LastOptSelectDt { get; set; }

        public ICommand SelectOptionCommand => new MpCommand<object>(
            async (args) => {
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

                int delay_ms = 1_000;

                DateTime this_select_dt = DateTime.Now;
                LastOptSelectDt = this_select_dt;

                switch(optionType) {
                    case OptionType.DisplayMode:
                    case OptionType.Pattern:
                        OptionLookup[optionType].ForEach(x => x.IsChecked = x == ovm);
                        break;
                    case OptionType.Key:
                        ovm.IsChecked = !ovm.IsChecked;
                        SelectedKeys = KeyOptions.Where(x => x.IsChecked).Select(x => x.OptionValue.ToEnum<NoteType>());
                        break;
                    case OptionType.ChordSuffix:
                    case OptionType.ScaleSuffix:
                    case OptionType.ModeSuffix:
                        ovm.IsChecked = !ovm.IsChecked;
                        SelectedSuffixes = SuffixOptions.Where(x => x.IsChecked).Select(x => x.OptionValue);
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

                        break;
                    case OptionType.ChordSort:
                    case OptionType.ScaleSort:
                    case OptionType.ModeSort:
                        bool is_secondary = args is object[];
                        if(is_secondary) {
                            ovm.IsChecked = !ovm.IsChecked;
                        } else {
                            SortOptions.Move(SortOptions.IndexOf(ovm),SortOptions.Count - 1);
                            OnPropertyChanged(nameof(SortOption1));
                            OnPropertyChanged(nameof(SortOption2));
                            OnPropertyChanged(nameof(SortOption3));
                            await Task.Delay(700);
                        }

                        break;
                }

                while(true) {
                    if(LastOptSelectDt != this_select_dt) {
                        // new toggle don't do anything
                        PlatformWrapper.Services.Logger.WriteLine($"skipping '{ovm}'");
                        return;
                    }

                    if(DateTime.Now - this_select_dt > TimeSpan.FromMilliseconds(delay_ms)) {
                        break;
                    }

                    await Task.Delay(100);
                }

                if(LastDisplayMode != SelectedDisplayMode ||
                   LastPatternType != SelectedPatternType) {
                    PlatformWrapper.Services.Logger.WriteLine("disp or pattern change");
                    InitInstrument();
                } else {
                    MatchUpdateSource? mus = null;
                    if(SelectedKeys.Difference(LastSelectedKeys).Any() ||
                       SelectedSuffixes.Difference(LastSelectedSuffixes).Any()) {
                        PlatformWrapper.Services.Logger.WriteLine("suff or key change");
                        mus = MatchUpdateSource.FilterToggle;

                    } else if(Enumerable.Range(0,3).Any(
                                  x => SelectedMatchSort.ElementAtOrDefault(x).Item1 !=
                                       LastMatchSort.ElementAtOrDefault(x).Item1 ||
                                       SelectedMatchSort.ElementAtOrDefault(x).Item2 !=
                                       LastMatchSort.ElementAtOrDefault(x).Item2)) {
                        PlatformWrapper.Services.Logger.WriteLine("sort change");
                        mus = MatchUpdateSource.SortToggle;
                    }


                    if(SelectedSvgOptionTypes.Difference(LastSelectedSvgOptionTypes).Any()) {
                        PlatformWrapper.Services.Logger.WriteLine("svg change");
                        UpdateMatchCss();
                    }

                    if(mus is { } us) {
                        UpdateMatchesAsync(us).FireAndForgetSafeAsync();
                    }

                }

                Prefs.Instance.Save();
            });

        public ICommand FindMatchesCommand => new MpCommand(
            () => {
                UpdateMatchesAsync(MatchUpdateSource.FindClick).FireAndForgetSafeAsync();
            });


        public ICommand ResetToDefaultsCommand => new MpCommand(
            async () => {
                try {
                    while(true) {
                        if(MainView.Instance is not { } mv ||
                           !mv.DlgHost.IsLoaded) {
                            await Task.Delay(100);
                        }

                        break;
                    }

                    DialogHost.Show(new LoadingView(),MainDialogHostName).FireAndForgetSafeAsync();

                    await Task.Delay(1_000);

                    await Dispatcher.UIThread.InvokeAsync(
                        async () => {
//await DefaultDataBuilder.BuildAsync();
                            PlatformWrapper.Services.Logger.WriteLine("Clearing instruments");
                            Instruments.Clear();
                            string def_json =
                                MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/def.json");
                            var instl = JsonConvert.DeserializeObject<List<Instrument>>(def_json);
                            //var instl = def_json.DeserializeBase64Object<List<Instrument>>();//JsonConvert.DeserializeObject<List<Instrument>>(def_json);
                            PlatformWrapper.Services.Logger.WriteLine($"Adding {instl.Count} instruments");
                            await InitAsync(instl);
                        },DispatcherPriority.Background);

                    DialogHost.Close(MainDialogHostName);
                } catch(Exception ex) {
                    PlatformWrapper.Services.Logger.WriteLine(ex.ToString());
                }
            });

        #endregion

    }

}