using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DialogHostAvalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Newtonsoft.Json;
using Svg.Skia;
using AvSnackbarHost = Material.Styles.Controls.SnackbarHost;
using AvSvg = Avalonia.Svg.Skia.Svg;

#if DEBUG
using System.Diagnostics;
#endif

namespace Calcuchord {
    public partial class MainViewModel : ViewModelBase {

        #region Private Variables

        readonly object _matchCreateLock = new object();

        string _editInstrumentInitialStateJson;

        #endregion

        #region Constants

        public const int DEFAULT_MATCH_COL_COUNT = 3;

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

        public ObservableCollection<MatchViewModel> Matches { get; private set; } = [];
        IEnumerable<MatchViewModel> UnfilteredMatches { get; set; } = [];
        IEnumerable<NoteViewModel> LastNotes { get; set; } = [];
        IEnumerable<MatchViewModel> LastMatches { get; } = [];
        IEnumerable<OptionViewModel> LastOptions { get; } = [];

        public MatchViewModel SelectedMatch {
            get => Matches.FirstOrDefault(x => x.IsSelected);
            set {
                Matches.ForEach(x => x.IsSelected = value == x);
                OnPropertyChanged();
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
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedTuning));
                }
            }
        }

        public TuningViewModel SelectedTuning =>
            SelectedInstrument == null ? null : SelectedInstrument.SelectedTuning;

        public TuningViewModel LastSelectedTuning { get; private set; }

        InstrumentViewModel LastSelectedInstrument { get; set; }

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

        Dictionary<NoteType,OptionViewModel> KeyOptLookup { get; set; } = [];

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

        public OptionViewModel SortOption4 =>
            SortOptions.Any() ? SortOptions[3] : null;

        #endregion

        #region Svg

        public ObservableCollection<OptionViewModel> SvgOptions =>
            OptionLookup[CurSvgOptionType];

        #endregion

        #region Suffix

        public IEnumerable<OptionViewModel> SuffixOptions =>
            OptionLookup[CurSuffixOptionType];

        Dictionary<string,OptionViewModel> SuffixOptLookup { get; set; } = [];

        #endregion

        #region Degree

        public IEnumerable<OptionViewModel> DegreeOptions =>
            OptionLookup[OptionType.Degree];

        #endregion

        public Dictionary<OptionType,ObservableCollection<OptionViewModel>> OptionLookup { get; } =
            new Dictionary<OptionType,ObservableCollection<OptionViewModel>>
            {
                { OptionType.DisplayMode,[] },
                { OptionType.Pattern,[] },
                { OptionType.Key,[] },
                { OptionType.Degree,[] },

                { OptionType.ModeSuffix,[] },
                { OptionType.ChordSuffix,[] },
                { OptionType.ScaleSuffix,[] },

                { OptionType.ModeSort,[] },
                { OptionType.ChordSort,[] },
                { OptionType.ScaleSort,[] },

                { OptionType.ChordSvg,[] },
                { OptionType.ScaleSvg,[] },
                { OptionType.ModeSvg,[] },
            };

        #endregion

        #endregion

        #region Appearance

        public string SelectedKeyDisplayName =>
            SelectedKey.HasValue ? SelectedKey.Value.ToDisplayValue() : string.Empty;

        public string MatchSvgCss { get; private set; } = string.Empty;

        public string PatternName =>
            SelectedPatternType.ToString();

        public string PatternSingularName =>
            PatternName.Substring(0,PatternName.Length - 1).ToLower();

        #endregion

        #region Layout

        public double DefaultMatchWidth => 350;
        public double MatchesViewFixedWidth => DefaultMatchWidth * 3;

        #endregion

        #region State

        #region UI

        public DrawerPageType CurrentDrawerPage { get; set; } = DrawerPageType.Main;

        public string MainDialogHostName => "MainDialogHost";
        public string InstEditDialogHostName => "InstrumentEditorPopupHost";

        public bool IsDoingIntro { get; private set; }
        public bool IsLoaded { get; private set; }

        public bool IsDrawerOpen { get; set; }

        #endregion

        #region Options

        public bool IsInEditInstrumentMode =>
            SelectedInstrument != null &&
            SelectedInstrument == EditModeInstrument;

        public bool IsExactMatchOnly { get; set; } = true;


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

        public IEnumerable<SvgOptionType> SelectedSvgOptionTypes =>
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

        public bool IsKeySelected =>
            SelectedKey is not null;

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
            !IsInstrumentChanging &&
            SelectedTuning != null &&
            SelectedDisplayMode == DisplayModeType.Search &&
            EditModeInstrument == null;

        bool IsInstrumentChanging { get; set; }

        public int SelectedInstrumentIndex {
            get => Instruments.IndexOf(SelectedInstrument);
            set {
                if(value >= 0 && value < Instruments.Count) {
                    SelectedInstrument = Instruments[value];
                } else {
                    SelectedInstrument = null;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedInstrument));
            }
        }

        public InstrumentType SelectedInstrumentType =>
            SelectedInstrument == null ? 0 : SelectedInstrument.Instrument.InstrumentType;

        public bool IsPianoSelected =>
            SelectedInstrumentType == InstrumentType.Piano;

        ChordKeyDegreeType SelectedKeyDegree { get; set; } = ChordKeyDegreeType.I;

        public bool IsDefaultSelection =>
            SelectedTuning == null ? true : SelectedTuning.NoteRows.All(x => x.IsDefaultSelection);

        #endregion

        #region Matches

        public int MatchColCount { get; private set; } = DEFAULT_MATCH_COL_COUNT;
        int MaxMatchColCount => 8;

        //IEnumerable<ChordKeyDegreeType> AvailableDegrees { get; set; } = [];
        IEnumerable<NoteType> AvailableKeys { get; } = [];

        NoteType? LastSelectedKey { get; set; }

        public NoteType? SelectedKey { get; private set; }

        IEnumerable<string> AvailableSuffixes { get; } = [];

        IEnumerable<string> SelectedSuffixes { get; set; } = [];

        IEnumerable<(MatchSortType,bool)> SelectedMatchSort =>
            SortOptions.Select(x => (x.OptionValue.ToEnum<MatchSortType>(),x.IsChecked));

        public bool CanIncreaseMatchColumnCount =>
            true; //MatchColCount < Matches.Count;

        public bool CanDecreaseMatchColumnCount =>
            MatchColCount > 1;

        public bool IsMatchZoomChanging { get; private set; }
        public bool IsLoadingMatches { get; private set; }

        public bool IsSearchInitiating { get; private set; }
        public CancellationTokenSource ZoomCts { get; private set; }
        public CancellationTokenSource MatchCts { get; private set; }

        public bool IsMatchesEmpty { get; private set; }


        public bool IsSearchButtonVisible {
            get {
                if(!IsBusy &&
                   !IsSearchInitiating &&
                   !IsDefaultSelection &&
                   SelectedTuning.SelectedNotes.Difference(LastNotes).Any()) {
                    return true;
                }

                if(SelectedKey != LastSelectedKey) {
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

        public event EventHandler InstrumentInitialized;

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
            while(other_instl.Any(x => string.Equals(x.Title,unique_name,StringComparison.CurrentCultureIgnoreCase))) {
                unique_name = $"{desiredName}{suffix++}";
            }

            return unique_name;
        }

        public void SetSelectedKey(NoteType? nt) {
            if(SelectedKey == nt) {
                return;
            }

            NoteType key = nt ?? SelectedKey.Value;
            object[] args = ["dummy",KeyOptions.FirstOrDefault(x => x.OptionValue == key.ToString())];
            SelectOptionCommand.Execute(args);
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void MainViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedKey):
                    if(SelectedTuning is { } st &&
                       st.SelectedNotes is { } sn) {
                        sn.ForEach(x => x.RaisePropertyChanged(nameof(x.IsSelectedKey)));
                    }

                    break;
                case nameof(MatchColCount):
                    OnPropertyChanged(nameof(CanDecreaseMatchColumnCount));
                    OnPropertyChanged(nameof(CanIncreaseMatchColumnCount));
                    Prefs.Instance.Save();

                    break;
                case nameof(EditModeInstrument):
                    if(EditModeInstrument is { } em_ivm) {
                        em_ivm.RaisePropertyChanged(nameof(em_ivm.IsEditModeEnabled));
                    }

                    OnPropertyChanged(nameof(IsInstrumentVisible));

                    break;
                case nameof(IsDrawerOpen):
                    if(MatchesView.Instance is not { } mtv ||
                       SelectedMatch == null) {
                        break;
                    }

                    mtv.ScrollItemIntoView(SelectedMatch);
                    break;
                case nameof(SelectedTuning):
                    if(SelectedTuning == LastSelectedTuning) {
                        break;
                    }

                    LastSelectedTuning = SelectedTuning;
                    if(IsInEditInstrumentMode) {
                        break;
                    }

                    InitInstrumentAsync(InstrumentInitSource.TuningChanged).FireAndForgetSafeAsync();
                    break;
                case nameof(SelectedInstrumentIndex):
                    OnPropertyChanged(nameof(SelectedInstrument));
                    if(!IsLoaded) {
                        ForwardCommand.Execute(null);
                    }


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
                    OptionLookup.Values.SelectMany(x => x).ForEach(x => x.RaisePropertyChanged(nameof(x.IsEnabled)));
                    break;
                case nameof(IsSearchInitiating):
                    //OnPropertyChanged(nameof(IsMatchesEmpty));
                    if(!IsSearchInitiating &&
                       MatchesView.Instance is { } matchesView &&
                       matchesView.MatchItemsRepeater is { } mir) {
                        // BUG items repeater overlaps items on load intermittently
                        mir.InvalidateMeasure();
                    }

                    break;
                case nameof(IsBusy):
                    // if(MainView.Instance is not { } mv ||
                    //    mv.MainContainerBusyOverlay is not { } mcbo) {
                    //     break;
                    // }
                    //
                    // mcbo.IsVisible = IsBusy;
                    if(IsBusy) {


                    }

                    //PlatformWrapper.Services.Logger.WriteLine($"Busy: {IsBusy}");

                    break;
            }
        }

        void MatchesOnCollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(IsMatchesEmpty));
            //OnPropertyChanged(nameof(CanIncreaseMatchColumnCount));F
        }

        async Task InitAsync(IEnumerable<Instrument> instl = null) {
            bool is_reset = instl != null;
            IsBusy = !is_reset;

            await Task.Delay(500);

            while(!Prefs.IsLoaded) {
                await Task.Delay(100);
            }


            instl = instl ?? Prefs.Instance.Instruments;

            ThemeViewModel.Instance.Init();
            MatchColCount = Prefs.Instance.MatchColCount;

            if(!instl.Any()) {
                IsBusy = false;
                ResetToDefaultsCommand.Execute(null);
                return;
            }


            foreach(Instrument inst in instl) {
                InstrumentViewModel ivm = await CreateInstrumentAsync(inst);
                Instruments.Add(ivm);
            }

            await InitInstrumentAsync(InstrumentInitSource.Startup,is_reset);

            if(is_reset) {
                Prefs.Instance.Save();
            }

            IsLoaded = true;
            IsBusy = false;
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
            if(SelectedTuning == null ||
               source is
                   MatchUpdateSource.NoteToggle or
                   MatchUpdateSource.RootToggle ||
               (source is MatchUpdateSource.FilterToggle &&
                IsSearchModeSelected &&
                SelectedTuning != null &&
                SelectedTuning.SelectedNotes.None())) {
                // dont search when notes clicked or
                // toggling options when instrument has empty selection
                UpdateMatchOverlays();
                IsSearchInitiating = false;
                return;
            }

            IsMatchesEmpty = false;
            IsLoadingMatches = true;
            CancelMatchFilter();
            try {
                await Task.Run(
                    () => {
                        try {
                            LoadMatches(source);
                        } catch {
                            // ignored
                        }
                    },MatchCts.Token);
            } catch {
                //ignored
            }

            IsLoadingMatches = false;
        }

        void UpdateMatchOverlays() {

            OnPropertyChanged(nameof(SelectedPatternType));
            OnPropertyChanged(nameof(SelectedInstrumentIndex));
            OnPropertyChanged(nameof(SelectedInstrument));
            OnPropertyChanged(nameof(SelectedTuning));
            OnPropertyChanged(nameof(IsDefaultSelection));
            OnPropertyChanged(nameof(IsInstrumentVisible));

            OnPropertyChanged(nameof(PatternSingularName));

            OnPropertyChanged(nameof(IsSearchButtonVisible));
            OnPropertyChanged(nameof(IsPianoSelected));
            OnPropertyChanged(nameof(IsInstrumentsEmpty));
            OnPropertyChanged(nameof(IsTuningsEmpty));
            OnPropertyChanged(nameof(IsSearchModeSelected));
            OnPropertyChanged(nameof(IsBookmarkModeSelected));
            OnPropertyChanged(nameof(IsIndexModeSelected));
            OnPropertyChanged(nameof(IsMatchesEmpty));
        }

        void UpdateFilters() {
            //DegreeOptions.ForEach(x => x.IsEnabled = AvailableDegrees.Any(y => y.ToString() == x.OptionValue));
            KeyOptions.ForEach(x => x.IsEnabled = AvailableKeys.Any(y => y.ToString() == x.OptionValue));
            SuffixOptions.ForEach(x => x.IsEnabled = AvailableSuffixes.Contains(x.OptionValue));

            OnPropertyChanged(nameof(DegreeOptions));
            OnPropertyChanged(nameof(KeyOptions));
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));
            OnPropertyChanged(nameof(SortOptions));

            DegreeOptions.ForEach(x => x.RaisePropertyChanged(nameof(x.IsChecked)));
            KeyOptions.ForEach(x => x.RaisePropertyChanged(nameof(x.IsChecked)));
            SuffixOptions.ForEach(x => x.RaisePropertyChanged(nameof(x.IsChecked)));
            SvgOptions.ForEach(x => x.RaisePropertyChanged(nameof(x.IsChecked)));
        }

        IEnumerable<MatchViewModel> GetMatchResults(NoteViewModel[] sel_notes) {
            // prefer desired root over key?
            var target_key = SelectedKey;

            if(IsChordsSelected &&
               target_key is { } tk &&
               SelectedKeyDegree is { } td) {
                // shift to sel degree
                target_key = tk.ToDegree(td);
            }

            var unavail_keyl = KeyOptions.ToList();
            var unavail_suffl = SuffixOptions.ToList();
            foreach(var kvp in MatchProvider.PatternLookup) {
                bool omitted_key = target_key is { } tk2 && tk2 != kvp.Key;

                foreach(var kvp2 in kvp.Value) {
                    bool omitted_suff = SelectedSuffixes.Any() && !SelectedSuffixes.Contains(kvp2.Key);
                    foreach(MatchViewModel mvm in kvp2.Value) {
                        bool valid = false;
                        if(IsSearchModeSelected) {
                            mvm.Score = MatchProvider.GetScore(mvm.NotePattern,sel_notes);
                            if(mvm.Score > 0 || sel_notes.Length == 0) {
                                valid = true;
                            }
                        } else if(IsBookmarkModeSelected && mvm.IsBookmarked) {
                            valid = true;
                        } else {
                            valid = true;
                        }

                        if(!valid) {
                            continue;
                        }

                        unavail_keyl.Remove(KeyOptLookup[kvp.Key]);
                        unavail_suffl.Remove(SuffixOptLookup[kvp2.Key]);
                        if(!omitted_key &&
                           !omitted_suff &&
                           (!IsSearchModeSelected || mvm.Score > 0)) {
                            yield return mvm;
                        }

                    }
                }
            }

            Dispatcher.UIThread.Invoke(
                () => {
                    KeyOptions.ForEach(x => x.IsEnabled = !unavail_keyl.Contains(x));
                    SuffixOptions.ForEach(x => x.IsEnabled = !unavail_suffl.Contains(x));
                },DispatcherPriority.Normal,MatchCts.Token);

        }

        void LoadMatches(MatchUpdateSource source) {
            Dispatcher.UIThread.Invoke(
                () => {
                    IsSearchInitiating = true;
                },DispatcherPriority.Background,MatchCts.Token);

            var sel_notes = SelectedTuning.SelectedNotes.ToArray();
            LastNotes = sel_notes.ToList();
            LastSelectedKey = SelectedKey;

            IEnumerable<MatchViewModel> sorted_results = null;
            var results = GetMatchResults(sel_notes);
            if(source is MatchUpdateSource.FindClick
               or MatchUpdateSource.FilterToggle) {

                AvSnackbarHost.Post(
                    $"{results.Count():n0} found",
                    MainView.SnackbarHostName,
                    DispatcherPriority.Background);
            }

            sorted_results = SortMatches(results);
            Dispatcher.UIThread.Invoke(
                () => {
                    Matches = new ObservableCollection<MatchViewModel>(sorted_results);
                    SelectedMatch = Matches.FirstOrDefault();
                    if(MatchesView.Instance is { } mv) {
                        mv.MatchesScrollViewer.ScrollToHome();
                    }

                    OnPropertyChanged(nameof(CanIncreaseMatchColumnCount));
                    OnPropertyChanged(nameof(CanDecreaseMatchColumnCount));

                    IsMatchesEmpty = Matches.None();

                    IsSearchInitiating = false;
                },DispatcherPriority.Background,MatchCts.Token);
        }

        IEnumerable<MatchViewModel> SortMatches(IEnumerable<MatchViewModel> matches) {
            var sorts = SelectedMatchSort.ToList();
            var result = matches
                .OrderBy(x => GetSortOptionValue(sorts[0],x))
                .ThenBy(x => GetSortOptionValue(sorts[1],x))
                .ThenBy(x => GetSortOptionValue(sorts[2],x))
                .ThenBy(x => GetSortOptionValue(sorts[3],x));
            return result;


            double GetSortOptionValue((MatchSortType field,bool desc) sort,MatchViewModel match) {
                double score = 0;
                switch(sort.field) {
                    case MatchSortType.Key:
                        score = (double)match.NotePattern.Key;
                        break;
                    case MatchSortType.Suffix:
                        score = match.NotePattern.Parent.SuffixId;
                        break;
                    case MatchSortType.Position:
                        score = match.NotePattern.Position;
                        break;
                    case MatchSortType.Score:
                        score = IsSearchModeSelected && !IsExactMatchOnly ? match.Score : 1;
                        break;
                }

                return score * (sort.desc ? -1 : 1);
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

        async Task CancelMatchFilterAsync() {
            if(MatchCts == null) {
                MatchCts = new CancellationTokenSource();
            } else {
                await MatchCts.CancelAsync();
                MatchCts.Dispose();
                MatchCts = new CancellationTokenSource();
            }
        }

        void CancelMatchFilter() {
            if(MatchCts == null) {
                MatchCts = new CancellationTokenSource();
            } else {
                MatchCts.Cancel();
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

        public int DiscoverMatchColumnCount() {
            if(MatchesView.Instance is not { } mv ||
               !mv.IsLoaded) {
                return MatchColCount;
            }

            return mv.GetVisualColCount();
            // if(MatchesView.Instance is not { } mv ||
            //    !mv.IsLoaded ||
            //    mv.GetVisualDescendant<ItemsRepeater>() is not { } mir) {
            //     return MatchColCount;
            // }
            //
            // double avail_w = mir.Bounds.Width;
            // int cols = (int)Math.Max(1,avail_w / MatchWidth);
            // return Math.Max(1,Math.Min(Matches.Count,cols));
        }

        public bool SetMatchColumnCount(int newColCount) {
            if(Matches.Count > 0 && !IsLoadingMatches) {
                newColCount = Math.Min(MaxMatchColCount,newColCount);
            }

            MatchColCount = newColCount;
            return true;
        }

        public async Task SetMatchColumnCountAsync(int newColCount,CancellationToken ct) {
            if(!SetMatchColumnCount(newColCount) ||
               MatchesView.Instance is not { } mv ||
               mv.MatchItemsRepeater is not { } mir) {
                return;
            }

            try {
                await Task.Delay(20,ct);
                while(!mir.IsArrangeValid) {
                    await Task.Delay(100,ct);
                }

                if(SelectedMatch is { } sel_mtvm) {
                    mv.ScrollItemIntoView(sel_mtvm);
                }

                ZoomCts?.Dispose();
                ZoomCts = null;
            } catch(OperationCanceledException) {
                // canceled
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
                { OptionType.ModeSort,(typeof(MatchSortType),-1) },
                { OptionType.Degree,(typeof(ChordKeyDegreeType),0) },
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
                                IsChecked = x.Value.Item2 == idx,
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

        bool VerifyOptions(List<OptionViewModel> opts) {
            if(!Prefs.Instance.WasOptionsOutOfDateOnStartup) {
                // options up to date
                PlatformWrapper.Services.Logger.WriteLine("Options up-to-date");
                return true;
            }

            // just reset them..
            PlatformWrapper.Services.Logger.WriteLine(
                $"Options expired! This Version: {Prefs.Instance.LastPrefsVersion} Needed Version: {Prefs.Instance.LastOptionsUpdatedPrefsVersion}");
            opts.Clear();
            opts.AddRange(CreateOptions());
            PlatformWrapper.Services.Logger.WriteLine("Options reset");
            return false;
        }

        void InitOptions(bool reset) {
            var all_opts = Prefs.Instance.Options;
            if(all_opts.None()) {
                // initial startup case
                all_opts.AddRange(CreateOptions());
            }

            if(OptionLookup.Values.SelectMany(x => x).None()) {
                // startup
                if(!VerifyOptions(all_opts) && !string.IsNullOrEmpty(Prefs.Instance.LastPrefsVersion)) {
                    // out of date (and not initial startup)
#if DEBUG
                    Debugger.Break();
#endif

                }

                // create option lookup (on startup)
                foreach(var kvp in OptionLookup) {
                    kvp.Value.Clear();
                    kvp.Value.AddRange(all_opts.Where(x => x.OptionType == kvp.Key));

                }
            }

            if(reset) {
                foreach(OptionViewModel ovm in OptionLookup.Values.SelectMany(x => x)) {
                    switch(ovm.OptionType) {
                        case OptionType.Degree:
                            ovm.IsChecked = ovm.OptionValue == ChordKeyDegreeType.I.ToString();
                            break;
                        case OptionType.ChordSuffix:
                        case OptionType.ScaleSuffix:
                        case OptionType.ModeSuffix:
                        case OptionType.Key:
                            ovm.IsChecked = false;
                            break;

                    }
                }
            }

            KeyOptLookup = KeyOptions.ToDictionary(x => x.OptionValue.ToEnum<NoteType>(),x => x);
            SuffixOptLookup = SuffixOptions.ToDictionary(x => x.OptionValue,x => x);

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
            OnPropertyChanged(nameof(DegreeOptions));
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));

            OnPropertyChanged(nameof(SearchOptionViewModel));
            OnPropertyChanged(nameof(BookmarksOptionViewModel));
            OnPropertyChanged(nameof(IndexOptionViewModel));

            OnPropertyChanged(nameof(SortOption1));
            OnPropertyChanged(nameof(SortOption2));
            OnPropertyChanged(nameof(SortOption3));
            OnPropertyChanged(nameof(SortOption4));

            UpdateMatchCss();

            OptionLookup.SelectMany(x => x.Value).ForEach(x => x.RaisePropertyChanged(nameof(x.IsChecked)));
            OptionLookup.SelectMany(x => x.Value).ForEach(x => x.RaisePropertyChanged(nameof(x.IsEnabled)));
        }

        #endregion

        #region Instruments

        async Task<InstrumentViewModel> CreateInstrumentAsync(Instrument instrument) {
            InstrumentViewModel ivm = new InstrumentViewModel(this);
            await ivm.InitAsync(instrument);
            return ivm;
        }

        async Task InitInstrumentAsync2(InstrumentInitSource source,bool isStartupReset = false) {
            await Task.Delay(1);
            Task.Run(
                () => {
                    bool reset_opts = source == InstrumentInitSource.Startup &&
                                      SelectedDisplayMode == DisplayModeType.Search;

                    if(SelectedTuning != null) {

                        InitOptions(reset_opts);
                        InitMatchProvider();

                    }

                    Dispatcher.UIThread.Post(
                        () => {
                            UpdateMatchOverlays();

                            if(SelectedTuning is { } st) {
                                st.RaisePropertyChanged(nameof(st.IsSelected));
                                InstrumentInitialized?.Invoke(this,EventArgs.Empty);
                                //UpdateMatchesAsync(MatchUpdateSource.InstrumentInit).FireAndForgetSafeAsync();

                            }
                        });

                });
            // NOTE isStartupReset prevents double spinners on initial startup...
            PlatformWrapper.Services.Logger.WriteLine($"Init instrument. Source: {source}");
            Matches.Clear();

            UpdateMatchOverlays();
            if(SelectedTuning == LastSelectedTuning) {
                return;
            }

            LastSelectedTuning = SelectedTuning;
            LastSelectedInstrument = SelectedInstrument;

            if(SelectedTuning != LastSelectedTuning) {
                SelectedTuning.ResetSelection();
            }

            bool reset_opts = source == InstrumentInitSource.Startup &&
                              SelectedDisplayMode == DisplayModeType.Search;


            InitOptions(reset_opts);
            Task.Run(
                () => {
                    InitMatchProvider();
                });

        }

        async Task InitInstrumentAsync(InstrumentInitSource source,bool isStartupReset = false) {
            // NOTE isStartupReset prevents double spinners on initial startup...
            PlatformWrapper.Services.Logger.WriteLine($"Init instrument. Source: {source}");
            IsBusy = !isStartupReset; //!(isStartupReset && source == InstrumentInitSource.Startup);
            if(IsBusy) {
                await Task.Delay(300);
            }


            await Dispatcher.UIThread.InvokeAsync(
                async () => {
                    if(source == InstrumentInitSource.Startup && IsIndexModeSelected) {
                        //await Task.Delay(3_000);
                    }


                    Matches.Clear();

                    bool do_sel_reset =
                        SelectedTuning != LastSelectedTuning;
                    if(SelectedTuning is { } sel_tvm && do_sel_reset) {
                        sel_tvm.ResetSelection();
                    }

                    if(SelectedInstrument != LastSelectedInstrument &&
                       LastSelectedInstrument != null &&
                       SelectedInstrument != null) {
                        IsInstrumentChanging = true;
                    }

                    LastSelectedInstrument = SelectedInstrument;
                    LastSelectedTuning = SelectedTuning;

                    bool reset_opts = source == InstrumentInitSource.Startup &&
                                      SelectedDisplayMode == DisplayModeType.Search;

                    if(SelectedTuning != null) {

                        InitOptions(reset_opts);
                        InitMatchProvider();

                    }


                    if(IsInstrumentChanging) {
                        await Task.Delay(500);
                    }

                    UpdateMatchOverlays();

                    if(SelectedTuning is { } st) {
                        st.RaisePropertyChanged(nameof(st.IsSelected));

                    }

                    InstrumentInitialized?.Invoke(this,EventArgs.Empty);
                    UpdateMatchesAsync(MatchUpdateSource.InstrumentInit).FireAndForgetSafeAsync();

                    await Task.Delay(200);

                },DispatcherPriority.Background);


            while(IsSearchInitiating) {
                await Task.Delay(100);
            }

            if(IsInstrumentChanging &&
               InstrumentView.Instance is { } iv) {
                await Task.Delay(500);
                while(!iv.IsArrangeValid) {
                    await Task.Delay(100);
                }

                iv.MeasureInstrument();
                IsInstrumentChanging = false;
            } else if(InstrumentView.Instance is { } iv2) {
                iv2.MeasureInstrument();
            }

            UpdateMatchOverlays();

            if(source != InstrumentInitSource.Startup ||
               MatchesView.Instance is not { } mtv) {
                IsBusy = false;
                return;
            }

            while(!mtv.MatchItemsRepeater.IsArrangeValid) {
                await Task.Delay(100);
            }

            mtv.InvalidateArrange();


            IsBusy = false;
        }

        #endregion

        #endregion

        #region Commands

        public ICommand ExportMatchesCommand => new MpCommand<object>(
            async (args) => {
                await Task.Delay(1);
                if(args.ToString() is not { } exp_type) {
                    return;
                }

                if(exp_type == "MIDI") {
                    if(PlatformWrapper.Services.ShareMidi is { } sm) {
                        await sm.ShareMidiAsync(
                            SelectedMatch.NotePattern.GetToneGroups(),
                            SelectedMatch.NotePattern.PatternType != MusicPatternType.Chords,
                            SelectedMatch.ShareTitle);
                    }

                    return;
                }

                if(exp_type == "PDF") {
                    if(PlatformWrapper.Services.SharePdf is { } sp &&
                       SelectedMatch is { } sm &&
                       PatternToSvgConverter.Instance.Convert(
                           sm.NotePattern,typeof(string),"styled|titled",
                           CultureInfo.CurrentCulture) is string sel_svg_str) {
                        using(SKSvg svg = new SKSvg()) {

                            if(svg.FromSvg(sel_svg_str) is not null) {
                                await sp.SharePdfAsync(svg,SelectedMatch.ShareTitle);
                            }
                        }
                    }

                    return;
                }

                if(Matches.FirstOrDefault() is not { } first_match ||
                   first_match.NotePattern is not { } np ||
                   PatternToSvgConverter.Instance.GetBuilder(np,false) is not { } builder) {
                    return;
                }

                bool is_done = false;
                LoadingView busy_view = new LoadingView
                {
                    MessageTextBlock =
                    {
                        Text = "Please wait...(This may take a while)",
                    },
                };
                CancellationTokenSource batch_cts = new CancellationTokenSource();
                busy_view.CancelButton.IsVisible = true;
                busy_view.CancelButton.Command = new MpCommand(
                    () => {
                        batch_cts.Cancel();
                        is_done = true;
                    });
                busy_view.WasmWarningBorder.IsVisible = OperatingSystem.IsBrowser();

                DialogHost.Show(busy_view,MainDialogHostName).FireAndForgetSafeAsync();

                try {
                    _ = Task.Run(
                        async () => {
                            await Task.Delay(1_500);
                            var npl = GetMatchResults(SelectedTuning.SelectedNotes.ToArray());
                            string title = SelectedTuning.FullName.RemoveInvalidPathChars().Replace(" - "," ");

                            if(exp_type == "HTML" && PlatformWrapper.Services.ShareHtml is { } shtml) {
                                string html = builder.GetBatchHtml(
                                    SelectedTuning.Tuning,npl.Select(x => x.NotePattern));
                                await shtml.ShareHtmlAsync(html,title);
                            } else if(exp_type == "FULLPDF" && PlatformWrapper.Services.SharePdf is { } spdf) {
                                string svg_html = builder.GetBatchSvg(
                                    SelectedTuning.Tuning,npl.Select(x => x.NotePattern),MatchColCount);
#if DEBUG
                                // if(ThemeViewModel.Instance.IsDesktop &&
                                //    TopLevel.GetTopLevel(MainView.Instance) is { } tl &&
                                //    tl.Clipboard is { } cb) {
                                //     cb.SetTextAsync(svg_html.ToPrettyPrintXml()).FireAndForgetSafeAsync();
                                // }
#endif
                                using(SKSvg svg = new SKSvg()) {
                                    if(svg.FromSvg(svg_html) is not null) {
                                        await spdf.SharePdfAsync(svg,title);
                                    }
                                }
                            }

                            is_done = true;
                        },batch_cts.Token);
                } catch(Exception ex) {
                    // canceled
                    ex.Dump();
                }

                while(!is_done) {
                    await Task.Delay(100);
                }

                DialogHost.Close(MainDialogHostName);
            });

        public MpIAsyncCommand CancelEditInstrumentCommand => new MpAsyncCommand(
            async () => {
                // close editor
                DialogHost.Close(Instance.MainDialogHostName);

                if(EditModeInstrument is not { } inst_to_restore_vm) {
                    _editInstrumentInitialStateJson = null;
                    return;
                }

                if(string.IsNullOrEmpty(_editInstrumentInitialStateJson)) {
                    return;
                }

                bool no_changes =
                    JsonConvert.SerializeObject(EditModeInstrument.Instrument) == _editInstrumentInitialStateJson;
                EditModeInstrument = null;

                if(no_changes) {
                    _editInstrumentInitialStateJson = null;
                    return;
                }

                Instrument inst_to_restore =
                    JsonConvert.DeserializeObject<Instrument>(_editInstrumentInitialStateJson);

                Prefs.Instance.Instruments.Remove(inst_to_restore_vm.Instrument);
                Prefs.Instance.Instruments.Add(inst_to_restore);

                await inst_to_restore_vm.InitAsync(inst_to_restore);
                Prefs.Instance.Save();
                _editInstrumentInitialStateJson = null;
                InitInstrumentAsync(InstrumentInitSource.EditorCanceled).FireAndForgetSafeAsync();
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
                        bool success = await new_tuning_vm.InitAsync(new_tuning_vm.Tuning);
                        emi_vm.CurGenTuning = null;
                        if(!success) {
                            // gen was canceled, restore edit view
                            return;
                        }
                    }
                }

                bool no_changes =
                    JsonConvert.SerializeObject(EditModeInstrument.Instrument) == _editInstrumentInitialStateJson;
                EditModeInstrument = null;
                _editInstrumentInitialStateJson = null;

                if(no_changes) {
                    return;
                }

                Prefs.Instance.Save();

                EditModeInstrument = null;
                SelectedInstrument = emi_vm;
                SelectedInstrument.RaisePropertyChanged(nameof(SelectedInstrument.SelectedTuning));
                OnPropertyChanged(nameof(SelectedTuning));
                InitInstrumentAsync(InstrumentInitSource.EditorDone).FireAndForgetSafeAsync();
                ForwardCommand.Execute(null);
            });

        public MpIAsyncCommand<object> BeginEditInstrumentCommand => new MpAsyncCommand<object>(
            async args => {
                if(args is not Control c ||
                   c.DataContext is not InstrumentViewModel edit_inst_vm) {
                    if(args is not InstrumentViewModel ivm) {
                        return;
                    }

                    edit_inst_vm = ivm;
                }

                Extensions.CloseFlyout(args);

                _editInstrumentInitialStateJson =
                    edit_inst_vm.IsActivated ? JsonConvert.SerializeObject(edit_inst_vm.Instrument) : null;

                edit_inst_vm.SelectedTuning ??= edit_inst_vm.Tunings.FirstOrDefault();

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
                EditModeInstrument = new InstrumentViewModel(this)
                {
                    Instrument = Instrument.CreateByType(InstrumentType.Guitar),
                };
                EditModeInstrument.Title = GetUniqueInstrumentName(EditModeInstrument.Title,[]);
                await EditModeInstrument.InitAsync(EditModeInstrument.Instrument);
                await BeginEditInstrumentCommand.ExecuteAsync(EditModeInstrument);
            });

        public ICommand DecreaseMatchColumnsCommand => new MpCommand(
            async () => {
                // plus btn
                IsMatchZoomChanging = true;
                await CancelMatchZoomAsync();
                await Task.Delay(500);
                if(ZoomCts == null) {
                    return;
                }

                int new_col_count = Math.Clamp(DiscoverMatchColumnCount() - 1,1,MaxMatchColCount);
                await SetMatchColumnCountAsync(new_col_count,ZoomCts.Token);
                IsMatchZoomChanging = false;
            },
            () => {
                return CanDecreaseMatchColumnCount;
            });

        public ICommand IncreaseMatchColumnsCommand => new MpCommand(
            async () => {
                // minus btn
                IsMatchZoomChanging = true;
                await CancelMatchZoomAsync();
                await Task.Delay(500);
                if(ZoomCts == null) {
                    return;
                }

                int new_col_count = Math.Clamp(DiscoverMatchColumnCount() + 1,1,MaxMatchColCount);
                await SetMatchColumnCountAsync(new_col_count,ZoomCts.Token);
                IsMatchZoomChanging = false;
            },
            () => {
                return CanIncreaseMatchColumnCount;
            });

        public ICommand ResetInstrumentCommand => new MpCommand(
            () => {
                SelectedKey = null;
                SelectedTuning.ResetSelection();
                LastNotes = [];

                Matches.Clear();

                UpdateMatchOverlays();
                InstrumentView.Instance.ScrollSelectionIntoView();
            });

        public ICommand RemoveInstrumentCommand => new MpCommand<object>(
            async (args) => {
                if(args is not Control c ||
                   c.DataContext is not InstrumentViewModel to_remove_ivm) {
                    return;
                }

                Extensions.CloseFlyout(args);

                bool? confirmed = null;
                YesNoDialogView dlg_v = new YesNoDialogView
                {
                    DataContext = new DialogViewModel
                    {
                        Label = $"Are you sure you want to delete '{to_remove_ivm.Title}'?",
                        OkCommand = new MpCommand(
                            () => {
                                confirmed = true;
                            }),
                        CancelCommand = new MpCommand(
                            () => {
                                confirmed = false;
                            }),
                    },
                };
                DialogHost.Show(dlg_v,Instance.MainDialogHostName).FireAndForgetSafeAsync();

                while(!confirmed.HasValue) {
                    await Task.Delay(100);
                }

                DialogHost.Close(Instance.MainDialogHostName);

                if(!confirmed.Value) {
                    // canceled
                    return;
                }

                Instruments.Remove(to_remove_ivm);

                Prefs.Instance.Save();
                BackCommand.Execute(null);
                await Task.Delay(300);
                InitInstrumentAsync(InstrumentInitSource.InstrumentRemoved).FireAndForgetSafeAsync();
            });

        public ICommand CloseDrawerCommand => new MpCommand(
            () => {
                IsDrawerOpen = false;
            });

        public ICommand BackCommand => new MpCommand(
            () => {
                switch(CurrentDrawerPage) {
                    case DrawerPageType.Options:
                        //SelectedInstrumentIndex = -1;
                        CurrentDrawerPage = DrawerPageType.Main;
                        break;
                    case DrawerPageType.Main:
                        IsDrawerOpen = false;
                        break;
                }
            });

        public ICommand ForwardCommand => new MpCommand(
            () => {
                switch(CurrentDrawerPage) {
                    case DrawerPageType.Options:
                        break;
                    case DrawerPageType.Main:
                        CurrentDrawerPage = DrawerPageType.Options;
                        break;
                }
            },() => {
                return SelectedTuning != null;
            });

        public ICommand ShowAboutCommand => new MpCommand(
            () => {
                bool needs_window = ThemeViewModel.Instance.IsDesktopOs;
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
                    try {
                        DialogHost.Close(Instance.MainDialogHostName);
                    } catch(Exception ex) {
                        ex.Dump();
                    }
                }

                AboutView about_view = new AboutView
                {
                    DataContext = new AboutViewModel(),
                };

                if(needs_window && about_view.Content is Control abc) {
                    // desktop only
                    about_view.CloseButton.IsVisible = true;

                    about_view.Width = about_view.Height = double.NaN;
                    abc.Margin = new();
                    Window about_win = new Window
                    {
                        Background = Brushes.Transparent,
                        Content = about_view,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        ShowInTaskbar = false,
                        SystemDecorations = SystemDecorations.BorderOnly,
                        CanResize = false,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    };
#if DEBUG
                    about_win.AttachDevTools();
#endif
                    about_win.Show();
                    MoveWindowExtension.SetIsEnabled(about_win,true);
                    return;
                }

                tl.AddHandler(InputElement.PointerPressedEvent,TopLevel_OnPointerPressed,RoutingStrategies.Tunnel,true);
                DialogHost.Show(about_view,Instance.MainDialogHostName).FireAndForgetSafeAsync();

            });

        public ICommand SelectOptionCommand => new MpCommand<object>(
            (args) => {
                bool suppress_action = false;
                if(args is not OptionViewModel ovm) {
                    if(args is object[] arg_parts &&
                       arg_parts.Any() &&
                       arg_parts[0] is OptionViewModel sort_ovm) {
                        ovm = sort_ovm;

                    } else if(args is object[] arg_parts2 &&
                              arg_parts2.Length == 2 &&
                              arg_parts2[0] is string dummy &&
                              arg_parts2[1] is OptionViewModel key_ovm) {
                        // suppress
                        ovm = key_ovm;
                        suppress_action = true;

                    } else {
                        return;
                    }
                }

                if(!ovm.OptionType.TryToEnum(out OptionType optionType)) {
                    return;
                }

                switch(optionType) {
                    case OptionType.DisplayMode:
                    case OptionType.Pattern:
                        OptionLookup[optionType].ForEach(x => x.IsChecked = x == ovm);
                        InitInstrumentAsync(
                            optionType == OptionType.Pattern
                                ? InstrumentInitSource.PatternChanged
                                : InstrumentInitSource.TabChanged).FireAndForgetSafeAsync();
                        break;
                    case OptionType.Key:
                        if(ovm.IsChecked) {
                            OptionLookup[optionType].ForEach(x => x.IsChecked = false);
                        } else {
                            OptionLookup[optionType].ForEach(x => x.IsChecked = x == ovm);
                        }

                        if(OptionLookup[optionType].FirstOrDefault(x => x.IsChecked) is { } sel_key_ovm) {
                            SelectedKey = sel_key_ovm.OptionValue.ToEnum<NoteType>();
                        } else {
                            SelectedKey = null;
                        }

                        if(suppress_action) {
                            PlatformWrapper.Services.Logger.WriteLine("query suppressed");
                            UpdateMatchOverlays();
                            return;
                        }

                        UpdateMatchesAsync(MatchUpdateSource.FilterToggle).FireAndForgetSafeAsync();

                        break;
                    case OptionType.Degree:
                        if(ovm.IsChecked) {
                            OptionLookup[optionType].ForEach(
                                x => x.IsChecked = x.OptionValue == ChordKeyDegreeType.I.ToString());
                        } else {
                            OptionLookup[optionType].ForEach(x => x.IsChecked = x == ovm);
                        }

                        if(OptionLookup[optionType].FirstOrDefault(x => x.IsChecked) is { } sel_deg_ovm) {
                            SelectedKeyDegree = sel_deg_ovm.OptionValue.ToEnum<ChordKeyDegreeType>();
                        }

                        UpdateMatchesAsync(MatchUpdateSource.FilterToggle).FireAndForgetSafeAsync();

                        break;
                    case OptionType.ChordSuffix:
                    case OptionType.ScaleSuffix:
                    case OptionType.ModeSuffix:
                        ovm.IsChecked = !ovm.IsChecked;
                        SelectedSuffixes = SuffixOptions.Where(x => x.IsChecked).Select(x => x.OptionValue);
                        UpdateMatchesAsync(MatchUpdateSource.FilterToggle).FireAndForgetSafeAsync();
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
                            SortOptions.Move(SortOptions.IndexOf(ovm),SortOptions.Count - 1);
                            OnPropertyChanged(nameof(SortOption1));
                            OnPropertyChanged(nameof(SortOption2));
                            OnPropertyChanged(nameof(SortOption3));
                            OnPropertyChanged(nameof(SortOption4));
                            //await Task.Delay(SortAnimDelayMs);
                        }

                        UpdateMatchesAsync(MatchUpdateSource.SortToggle).FireAndForgetSafeAsync();

                        break;
                }

                Prefs.Instance.Save();
            });

        public ICommand FindMatchesCommand => new MpCommand(
            () => {
                UpdateMatchesAsync(MatchUpdateSource.FindClick).FireAndForgetSafeAsync();
            });


        public ICommand ResetToDefaultsCommand => new MpCommand(
            async () => {
                LoadingView lv = new LoadingView();
                try {
                    while(true) {
                        if(MainView.Instance is not { } mv ||
                           !mv.DlgHost.IsLoaded) {
                            await Task.Delay(100);
                        }

                        break;
                    }

                    DialogHost.Show(lv,MainDialogHostName).FireAndForgetSafeAsync();
                    await Task.Delay(1_000);
                    //await DefaultDataBuilder.BuildAsync();
                    PlatformWrapper.Services.Logger.WriteLine("Clearing instruments");
                    Instruments.Clear();

                    _ = Task.Run(
                        () => {
                            List<Instrument> instl = null;
                            try {
                                string def_json =
                                    MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/def.json");
                                instl = Prefs.Parse(def_json).Instruments;

                                Dispatcher.UIThread.Post(
                                    async () => {
                                        try {
                                            await InitAsync(instl);
                                            DialogHost.Close(MainDialogHostName);
                                        } catch(Exception ex) {
                                            lv.MessageTextBlock.Text = ex.ToString();
                                            lv.MessageTextBlock.FontSize = 5;
                                            ex.Dump();
                                        }
                                    },DispatcherPriority.Background);
                            } catch(Exception ex) {
                                Dispatcher.UIThread.Post(
                                    () => {
                                        lv.MessageTextBlock.Text = ex.ToString();
                                        lv.MessageTextBlock.FontSize = 5;
                                    });
                                ex.Dump();
                            }
                        });

                } catch(Exception ex) {
                    Dispatcher.UIThread.Post(
                        () => {

                            lv.MessageTextBlock.Text = ex.ToString();
                            lv.MessageTextBlock.FontSize = 5;

                        });
                    ex.Dump();
                }
            });

        public ICommand OpenAppInstallerCommand => new MpCommand(
            () => {
                if(ThemeViewModel.Instance.IsDesktop) {
                }
            });

        public ICommand ToggleSearchTypeCommand => new MpCommand<object>(
            (args) => {
                if((IsExactMatchOnly && args.ToString() == "EXACT") ||
                   (!IsExactMatchOnly && args.ToString() == "VOICE")) {
                    // ignore
                    return;
                }

                IsExactMatchOnly = !IsExactMatchOnly;
                if(IsSearchButtonVisible) {
                    return;
                }

                UpdateMatchesAsync(MatchUpdateSource.FilterToggle).FireAndForgetSafeAsync();
            });

        #endregion

    }

}