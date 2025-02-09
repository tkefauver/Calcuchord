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
using Avalonia.Layout;
using Avalonia.Threading;
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

        #region Svg

        public Dictionary<MusicPatternType,ObservableCollection<OptionViewModel>> SvgDisplayModelLookup { get; } =
            new Dictionary<MusicPatternType,ObservableCollection<OptionViewModel>>
            {
                { MusicPatternType.Chords,[] },
                { MusicPatternType.Scales,[] },
                { MusicPatternType.Modes,[] }
            };

        public ObservableCollection<OptionViewModel> SvgOptions =>
            OptionLookup[CurSvgOptionType];

        IEnumerable<OptionViewModel> SelectedSvgOptions =>
            SvgOptions.Where(x => x.IsChecked);

        #endregion

        #region Suffix

        ObservableCollection<OptionViewModel> ChordSuffixOptions =>
            OptionLookup[OptionType.ChordSuffix];

        ObservableCollection<OptionViewModel> ScaleSuffixOptions =>
            OptionLookup[OptionType.ScaleSuffix];

        ObservableCollection<OptionViewModel> ModeSuffixOptions =>
            OptionLookup[OptionType.ModeSuffix];

        public IEnumerable<OptionViewModel> SuffixOptions =>
            SelectedPatternType switch
            {
                MusicPatternType.Chords => ChordSuffixOptions,
                MusicPatternType.Scales => ScaleSuffixOptions,
                MusicPatternType.Modes => ModeSuffixOptions,
                _ => throw new ArgumentOutOfRangeException()
            };

        public IEnumerable<OptionViewModel> AvailableSuffixOptions =>
            SuffixOptions.Where(x => AvailableSuffixes.Contains(x.OptionValue));

        IEnumerable<OptionViewModel> SelectedSuffixOptions =>
            SuffixOptions.Where(x => x.IsChecked);

        #endregion

        public Dictionary<OptionType,ObservableCollection<OptionViewModel>> OptionLookup { get; } =
            new Dictionary<OptionType,ObservableCollection<OptionViewModel>>
            {
                { OptionType.Key,[] },
                { OptionType.Pattern,[] },
                { OptionType.ChordSvg,[] },
                { OptionType.ScaleSvg,[] },
                { OptionType.ModeSvg,[] },
                { OptionType.ModeSuffix,[] },
                { OptionType.ChordSuffix,[] },
                { OptionType.ScaleSuffix,[] },
                { OptionType.DisplayMode,[] }
            };

        #endregion

        #endregion

        #region Appearance

        public string MatchSvgCss { get; private set; } = string.Empty;

        public string BusyText { get; set; }

        public string EmptyMatchLabel {
            get {
                if(IsDefaultSelection && IsSearchModeSelected) {
                    string suffix = IsPianoSelected ? "Keys" : "Frets";
                    return $"Select {suffix}";
                }

                if(IsBookmarkModeSelected) {
                    return "No bookmarks found";
                }

                return "No results";
                // switch(MpRandom.Rand.Next(4)) {
                //     default:
                //         return "Nothing";
                //     case 1:
                //         return "Nada";
                //     case 2:
                //         return "Zilch";
                //     case 3:
                //         return "Nope";
                // }
            }
        }

        #endregion

        #region Layout

        public double? RightDrawerExpandWidth =>
            IsRightDrawerOpen ? 0 : null;

        public static double DefaultMatchWidth => 350;
        public double MatchWidth { get; set; } = DefaultMatchWidth;

        #endregion

        #region State

        public bool IsLoaded { get; private set; }

        #region UI

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

        public MusicPatternType SelectedPatternType =>
            SelectedPatternOption == null ? 0 : SelectedPatternOption.OptionValue.ToEnum<MusicPatternType>();

        public DisplayModeType SelectedDisplayMode =>
            SelectedDisplayModeOption == null ? 0 : SelectedDisplayModeOption.OptionValue.ToEnum<DisplayModeType>();

        #endregion

        #region Instrument

        public bool CanFinishEdit =>
            EditModeInstrument != null &&
            EditModeInstrument.Tunings.Any();

        public bool IsInstrumentVisible =>
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

        public SvgFlags SelectedSvgFlags { get; private set; } = SvgBuilderBase.DefaultSvgFlags;

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

        IEnumerable<SvgFlags> AvailableSvgFlags { get; set; } = [];

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

        #region Instrument

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
                    UpdateSvgFlagsFromOptions();
                    break;
                case nameof(SelectedDisplayMode):
                    OnPropertyChanged(nameof(IsSearchModeSelected));
                    OnPropertyChanged(nameof(IsBookmarkModeSelected));
                    OnPropertyChanged(nameof(IsIndexModeSelected));
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
                DoIntroAsync().FireAndForgetSafeAsync();
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

        async Task DoIntroAsync() {
            // TODO show intro popup

            // allow to add instrument or cancel
            await AddInstrumentCommand.ExecuteAsync();

            while(EditModeInstrument != null) {
                await Task.Delay(100);
            }

            await Task.Delay(300);

            // add piano
            var instl = CreateDefaultInstruments([InstrumentType.Piano]);
            if(instl.FirstOrDefault() is not { } piano) {
                return;
            }

            EditModeInstrument = new InstrumentViewModel(this)
            {
                Instrument = piano
            };
            await EditModeInstrument.InitAsync(EditModeInstrument.Instrument);
            await FinishEditInstrumentCommand.ExecuteAsync();

            SelectedInstrument = Instruments.FirstOrDefault();
        }


        #region Matches

        public void UpdateMatches(MatchUpdateSource source) {
            if(source is
               MatchUpdateSource.NoteToggle or
               MatchUpdateSource.RootToggle) {
                UpdateMatchOverlays();
                return;
            }

            Task.Run(
                async () => {
                    await CancelMatchLoadAsync();
                    var sel_notes = SelectedTuning.SelectedNotes;
                    if(source is
                       MatchUpdateSource.FindClick or
                       MatchUpdateSource.InstrumentInit or
                       MatchUpdateSource.BookmarkToggle) {
                        lock(_matchCreateLock) {
                            CreateWorkingMatches(sel_notes);
                        }
                    }

                    LastDesiredRoot = DesiredRoot;
                    LastNotes = sel_notes.ToList();
                    FilteredMatches = GetFilterWorkingMatches();

                    await Dispatcher.UIThread.InvokeAsync(
                        async () => {
                            IsSearchInitiating = true;
                            IsLoadingMatches = true;
                            if(source == MatchUpdateSource.FindClick) {
                                string count_msg = $"{FilteredMatches.Count()} found";
                                AvSnackbarHost.Post(
                                    count_msg,
                                    null,
                                    DispatcherPriority.Background);
                            }

                            UpdateFilters();
                            MatchCts = new();

                            await LoadMatchesAsync(
                                FilteredMatches,
                                MatchCts.Token);

                            IsLoadingMatches = false;
                            IsSearchOverlayVisible = true;
                        },
                        DispatcherPriority.Background);
                });
        }

        void UpdateMatchOverlays() {
            OnPropertyChanged(nameof(IsSearchButtonVisible));
            OnPropertyChanged(nameof(EmptyMatchLabel));
        }

        void UpdateFilters() {
            KeyOptions.ForEach(x => x.IsEnabled = AvailableKeys.Any(y => y.ToString() == x.OptionValue));
            SuffixOptions.ForEach(x => x.IsEnabled = AvailableSuffixes.Contains(x.OptionValue));
            SvgOptions.ForEach(x => x.IsEnabled = AvailableSvgFlags.Any(y => y.ToString() == x.OptionValue));

            OnPropertyChanged(nameof(KeyOptions));
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(AvailableSuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));

            KeyOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            SuffixOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            SvgOptions.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
        }

        async Task LoadMatchesAsync(IEnumerable<MatchViewModelBase> matches,CancellationToken ct) {
            Matches.Clear();
            MatchCount = 0;
            if(!matches.Any()) {
                IsSearchInitiating = false;
            }

            var test2 = new List<string>();
            int test = 0;

            int init_count = Math.Max(1,MatchColCount * (MatchColCount - 1));

            int delay = ThemeViewModel.Instance.IsDesktop ? 0 : 50;

            foreach(MatchViewModelBase match in matches) {
                if(ct.IsCancellationRequested) {
                    IsLoadingMatches = false;
                    IsSearchInitiating = false;
                    return;
                }

                if(match.NoteGroup.FullName == "E maj11 #1") {
                    test2.Add(match.NoteGroup.Id);
                    test++;
                }

                if(test > 1) {

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

            (IncreaseMatchColumnsCommand as MpCommand)?.RaiseCanExecuteChanged();
            (DecreaseMatchColumnsCommand as MpCommand)?.RaiseCanExecuteChanged();
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
                    if(WorkingMatches.Where(x => x.NoteGroup.Id == "28106b4f-fba8-4061-97ec-62a3375c5f3a") is
                           { } test &&
                       test.Count() > 1) {

                    }

                    break;
            }

            AvailableKeys = WorkingMatches.Select(x => x.NoteGroup.Key).Distinct();
            AvailableSuffixes = WorkingMatches.Select(x => x.NoteGroup.SuffixKey).Distinct();
        }

        IEnumerable<MatchViewModelBase> GetFilterWorkingMatches() {
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
            switch(SelectedPatternType) {
                default:
                    return matches
                        .OrderByDescending(x => x.Score)
                        .ThenBy(x => x.NoteGroup.Position);
                case MusicPatternType.Chords:
                    return matches
                        .OrderByDescending(x => x.Score)
                        .ThenBy(x => GetChordSuffixSortOrder(x))
                        .ThenByDescending(x => GetChordProminanceScore(x))
                        .ThenBy(x => x.SecondaryLabel)
                        .ThenBy(x => x.NoteGroup.Position);

            }

            int GetChordProminanceScore(MatchViewModelBase match) {
                return match.NoteGroup.Notes.Count(x => !x.IsMute);
            }

            int GetChordSuffixSortOrder(MatchViewModelBase match) {
                if(match.NoteGroup.SuffixDisplayValue.StartsWith("maj")) {
                    return 0;
                }

                if(match.NoteGroup.SuffixDisplayValue.StartsWith("m")) {
                    return 1;
                }

                return 2;

            }

        }

        async Task CancelMatchLoadAsync() {
            if(MatchCts == null) {
                Debug.Assert(
                    !IsLoadingMatches,
                    "Match load state mismatch");
                return;
            }

            await MatchCts.CancelAsync();
            // while(IsLoadingMatches) {
            //     await Task.Delay(10);
            // }

        }

        void InitMatchProvider() {
            MatchProvider = new MatchProvider(
                SelectedPatternType,
                SelectedTuning == null ? null : SelectedTuning.Tuning);
        }

        public void DiscoverMatchColumnCount() {
            if(MatchesView.Instance is not { } mv ||
               mv.GetVisualDescendant<ItemsRepeater>() is not { } mir ||
               mir.Layout is not WrapLayout mir_wl) {
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
               mv.GetVisualDescendant<ItemsRepeater>() is not { } mir ||
               mir.Layout is not WrapLayout mir_wl) {
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

        void UpdateSvgFlagAvailability() {
            if(!OptionLookup.TryGetValue(
                   CurSvgOptionType,
                   out var svg_opts)) {
                return;
            }

            foreach((string key,int idx) in Enum.GetNames(typeof(SvgFlags)).WithIndex()) {
                SvgFlags flag = (SvgFlags)Math.Pow(2,idx);
                if(svg_opts.FirstOrDefault(x => x.OptionValue == key) is { } svg_opt) {
                    svg_opt.IsChecked = SelectedSvgFlags.HasFlag(flag);
                }
            }

            AvailableSvgFlags =
                Enum.GetNames(typeof(SvgFlags))
                    .Select(x => x.ToEnum<SvgFlags>())
                    .Where(
                        x => x.IsFlagEnabled(
                            SelectedInstrument.InstrumentType,
                            SelectedPatternType,
                            SelectedDisplayMode));
        }

        void UpdateSvgFlagsFromOptions() {
            int old_svg_val = (int)SelectedSvgFlags;
            int new_svg_val = 0;
            foreach((string key,int idx) in Enum.GetNames(typeof(SvgFlags)).WithIndex()) {
                if(SelectedSvgOptions.Any(x => x.OptionValue == key)) {
                    new_svg_val |= (int)Math.Pow(
                        2,
                        idx);
                }
            }

            if(new_svg_val == old_svg_val) {
                // no change
                return;
            }

            SelectedSvgFlags = (SvgFlags)new_svg_val;
            Prefs.Instance.Save();
            RefreshMatchesSvg();
        }

        public void UpdateMatchCss() {
            StringBuilder sb = new StringBuilder();

            if(SelectedSvgFlags.HasFlag(SvgFlags.Frets)) {
                sb.AppendLine(".fret-marker { display:none; }");
            } else {
                sb.AppendLine(".fret-labels { display:none; }");
            }

            if(SelectedSvgFlags.HasFlag(SvgFlags.Roots)) {
                sb.AppendLine(".root-open { stroke-width: 1.25; }");
            } else {
                sb.AppendLine(".root-box { display:none; }");
                sb.AppendLine(".root-open { stroke-width: 0.25; }");
            }

            if(!SelectedSvgFlags.HasFlag(SvgFlags.Tuning)) {
                sb.AppendLine(".string-tuning { display:none; }");
            }

            if(!SelectedSvgFlags.HasFlag(SvgFlags.Fingers)) {
                sb.AppendLine(".fingers-text { display:none; }");
            }

            if(!SelectedSvgFlags.HasFlag(SvgFlags.Colors)) {
                sb.AppendLine($".fingers-fill {{ fill: {ThemeViewModel.Instance.P[PaletteColorType.RootFretBg]}; }}");
            }

            if(!SelectedSvgFlags.HasFlag(SvgFlags.Matches)) {
                sb.AppendLine(".user-fill { fill: transparent; }");
            }

            if(!SelectedSvgFlags.HasFlag(SvgFlags.Notes)) {
                sb.AppendLine(".notes-text { display:none; }");
            }

            if(!SelectedSvgFlags.HasFlag(SvgFlags.Shadows)) {
                sb.AppendLine(".shadow-elm { display:none; }");
            }

            if(!SelectedSvgFlags.HasFlag(SvgFlags.Barres)) {
                sb.AppendLine(".barre-elm { display:none; }");
            }

            MatchSvgCss = sb.ToString();
        }

        public void RefreshMatchesSvg() {
            UpdateMatchCss();
            //Matches.ForEach(x => x.RefreshSvg());
        }

        void InitOptions() {
            var all_opts = Prefs.Instance.Options;
            if(!all_opts.Any()) {
                var opt_lookup = new Dictionary<OptionType,(Type,int)>
                {
                    { OptionType.Pattern,(typeof(MusicPatternType),0) },
                    { OptionType.DisplayMode,(typeof(DisplayModeType),0) },
                    { OptionType.ChordSuffix,(typeof(ChordSuffixType),-1) },
                    { OptionType.ScaleSuffix,(typeof(ScaleSuffixType),-1) },
                    { OptionType.ModeSuffix,(typeof(ModeSuffixType),-1) },
                    { OptionType.Key,(typeof(NoteType),-1) },
                    { OptionType.ChordSvg,(typeof(SvgFlags),-1) },
                    { OptionType.ScaleSvg,(typeof(SvgFlags),-1) },
                    { OptionType.ModeSvg,(typeof(SvgFlags),-1) }
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
                foreach((string key,int idx) in Enum.GetNames(typeof(SvgFlags)).WithIndex()) {
                    SvgFlags flag = (SvgFlags)(int)Math.Pow(
                        2,
                        idx);
                    if(SvgBuilderBase.DefaultSvgFlags.HasFlag(flag)) {
                        all_opts.Where(x => x.OptionValue == flag.ToString()).ForEach(x => x.IsChecked = true);
                    }
                }
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

            OnPropertyChanged(nameof(DisplayModeOptions));
            OnPropertyChanged(nameof(PatternOptions));
            OnPropertyChanged(nameof(KeyOptions));
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));

            OnPropertyChanged(nameof(SearchOptionViewModel));
            OnPropertyChanged(nameof(BookmarksOptionViewModel));
            OnPropertyChanged(nameof(IndexOptionViewModel));

            UpdateSvgFlagsFromOptions();
            UpdateSvgFlagAvailability();
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
            LastSelectedTuning = SelectedTuning;

            Debug.WriteLine("init instrument");


            if(SelectedTuning is { } sel_tvm) {
                sel_tvm.ResetSelection();
            }

            InitOptions();
            InitMatchProvider();

            OnPropertyChanged(nameof(SelectedInstrument));
            OnPropertyChanged(nameof(SelectedTuning));
            OnPropertyChanged(nameof(IsPianoSelected));
            OnPropertyChanged(nameof(IsInstrumentVisible));
            OnPropertyChanged(nameof(IsSearchButtonVisible));
            OnPropertyChanged(nameof(EmptyMatchLabel));

            UpdateMatches(MatchUpdateSource.InstrumentInit);
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

        public MpIAsyncCommand CancelEditInstrumentCommand => new MpAsyncCommand(
            async () => {

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

                _editInstrumentInitialStateJson = null;
                if(!emi_vm.IsActivated) {
                    // add new inst to list
                    Instruments.Add(emi_vm);
                    Prefs.Instance.Instruments.Add(emi_vm.Instrument);
                }

                if(emi_vm.Tunings.Where(x => !x.IsLoaded) is { } new_tuning_vms &&
                   new_tuning_vms.Any()) {
                    // gen any new tuning patterns
                    // TODO probably need progress thing here
                    foreach(TuningViewModel new_tuning_vm in new_tuning_vms) {
                        emi_vm.CurGenTuning = new_tuning_vm;
                        await Task.Delay(500);

                        bool success = await new_tuning_vm.InitAsync(new_tuning_vm.Tuning);
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

                if(SelectedInstrument != emi_vm) {
                    SelectedInstrument = emi_vm;
                }

                InitInstrument();
            });

        public MpIAsyncCommand<object> BeginEditInstrumentCommand => new MpAsyncCommand<object>(
            async args => {
                if(args is not InstrumentViewModel edit_inst_vm) {
                    if(args is not TuningViewModel tvm) {
                        return;
                    }

                    edit_inst_vm = tvm.Parent;
                }

                IsLeftDrawerOpen = false;
                IsRightDrawerOpen = false;

                await Task.Delay(300);

                // store full backup of instrument (existing only)
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
                await SetMatchColumnCountAsync(MatchColCount - 1);
                IsMatchZoomChanging = false;
            },
            () => {
                //return MatchColCount > 1;
                return true;
            });

        public ICommand IncreaseMatchColumnsCommand => new MpCommand(
            async () => {
                // minus btn
                IsMatchZoomChanging = true;
                await SetMatchColumnCountAsync(MatchColCount + 1);
                IsMatchZoomChanging = false;
            },
            () => {
                //return MatchColCount < Matches.Count;
                return true;
            });

        public ICommand ResetInstrumentCommand => new MpCommand(
            () => {
                SelectedTuning.ResetSelection();
                UpdateMatchOverlays();
            });

        public ICommand CloseRightDrawerCommand => new MpCommand(
            () => {
                IsRightDrawerOpen = false;
            });

        public ICommand SelectOptionCommand => new MpCommand<object>(
            (args) => {
                if(args is not OptionViewModel ovm ||
                   !ovm.OptionType.TryToEnum(out OptionType optionType)) {
                    return;
                }

                bool needs_save = false;

                switch(optionType) {
                    case OptionType.DisplayMode:
                    case OptionType.Pattern:
                        OptionLookup[optionType].ForEach(x => x.IsChecked = x == ovm);
                        InitInstrument();
                        needs_save = true;
                        break;
                    case OptionType.Key:
                    case OptionType.ChordSuffix:
                    case OptionType.ScaleSuffix:
                    case OptionType.ModeSuffix:
                        ovm.IsChecked = !ovm.IsChecked;
                        UpdateMatches(MatchUpdateSource.FilterToggle);
                        break;
                    case OptionType.ChordSvg:
                    case OptionType.ScaleSvg:
                    case OptionType.ModeSvg:
                        ovm.IsChecked = !ovm.IsChecked;
                        if(ovm.IsChecked &&
                           ovm.OptionValue.TryToEnum(out SvgFlags flag) &&
                           (flag == SvgFlags.Fingers || flag == SvgFlags.Notes)) {
                            SvgFlags other_flag = flag == SvgFlags.Fingers ? SvgFlags.Notes : SvgFlags.Fingers;
                            if(SvgOptions.FirstOrDefault(x => x.OptionValue == other_flag.ToString()) is
                               { } other_ovm) {
                                other_ovm.IsChecked = false;

                            }
                        }

                        UpdateSvgFlagsFromOptions();
                        // UpdateMatches(MatchUpdateSource.FilterToggle);
                        // InstrumentView.Instance.InvalidateAll();
                        needs_save = true;
                        break;
                }

                if(needs_save) {
                    Task.Run(() => Prefs.Instance.Save());
                }


            });

        public ICommand FindMatchesCommand => new MpCommand(
            () => {
                IsSearchOverlayVisible = false;
                UpdateMatches(MatchUpdateSource.FindClick);
            });

        #endregion

    }

}