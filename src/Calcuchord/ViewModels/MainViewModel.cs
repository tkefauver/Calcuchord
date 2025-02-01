using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using AvSnackbarHost = Material.Styles.Controls.SnackbarHost;

namespace Calcuchord {
    public class MainViewModel : ViewModelBase {

        #region Private Variables

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

        public ObservableCollection<InstrumentViewModel> Instruments { get; } = [];

        public InstrumentViewModel SelectedInstrument {
            get => Instruments.FirstOrDefault(x => x.IsSelected);
            set {
                Instruments.ForEach(x => x.IsSelected = x == value);
                OnPropertyChanged(nameof(SelectedInstrument));
                OnPropertyChanged(nameof(SelectedTuning));
            }
        }

        public TuningViewModel SelectedTuning =>
            SelectedInstrument == null ? null : SelectedInstrument.SelectedTuning;

        TuningViewModel LastSelectedTuning { get; set; }

        InstrumentViewModel DefaultInstrument =>
            Instruments.FirstOrDefault(x => x.InstrumentType == InstrumentType.Guitar);

        TuningViewModel DefaultTuning =>
            DefaultInstrument.Tunings.FirstOrDefault(x => x.Tuning.IsDefault);

        #endregion

        #region Options

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

        public ObservableCollection<OptionViewModel> PatternOptions =>
            OptionLookup[OptionType.Pattern];

        OptionViewModel SelectedPatternOption =>
            PatternOptions.FirstOrDefault(x => x.IsChecked);


        public ObservableCollection<OptionViewModel> KeyOptions =>
            OptionLookup[OptionType.Key];

        IEnumerable<OptionViewModel> SelectedKeyOptions =>
            KeyOptions.Where(x => x.IsChecked);

        public ObservableCollection<OptionViewModel> SvgOptions =>
            OptionLookup[OptionType.Svg];

        IEnumerable<OptionViewModel> SelectedSvgOptions =>
            SvgOptions.Where(x => x.IsChecked);

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

        Dictionary<OptionType,ObservableCollection<OptionViewModel>> OptionLookup { get; } =
            new Dictionary<OptionType,ObservableCollection<OptionViewModel>>
            {
                { OptionType.Key,[] },
                { OptionType.Pattern,[] },
                { OptionType.Svg,[] },
                { OptionType.ModeSuffix,[] },
                { OptionType.ChordSuffix,[] },
                { OptionType.ScaleSuffix,[] },
                { OptionType.DisplayMode,[] }
            };

        #endregion

        #endregion

        #region Appearance

        public string EmptyMatchLabel {
            get {
                string suffix = IsPianoSelected ? "Keys" : "Frets";
                if(IsDefaultSelection) {
                    return $"Select {suffix}";
                }

                switch(MpRandom.Rand.Next(4)) {
                    default:
                        return "Nothing";
                    case 1:
                        return "Nada";
                    case 2:
                        return "Zilch";
                    case 3:
                        return "Nope";
                }
            }
        }

        #endregion

        #region Layout

        public static double DefaultMatchWidth => 350;
        public double MatchWidth { get; set; } = DefaultMatchWidth;

        #endregion

        #region State

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

        public MusicPatternType SelectedPatternType =>
            SelectedPatternOption == null ? 0 : SelectedPatternOption.OptionValue.ToEnum<MusicPatternType>();

        public DisplayModeType SelectedDisplayMode =>
            SelectedDisplayModeOption == null ? 0 : SelectedDisplayModeOption.OptionValue.ToEnum<DisplayModeType>();

        #endregion

        #region Instrument

        public bool IsInstrumentVisible =>
            SelectedDisplayMode == DisplayModeType.Search;

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

        public NoteType? DesiredRoot { get; set; }

        public bool IsDefaultSelection =>
            SelectedTuning == null ? true : SelectedTuning.NoteRows.All(x => x.IsDefaultSelection);

        #endregion

        #region Matches

        public bool CanDecreaseMatchColCount =>
            MatchColCount > 1;

        public bool CanIncreaseMatchColCount =>
            MatchColCount < Matches.Count;

        int MatchColCount { get; set; } = 1;

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
                // if(FilteredMatches.Difference(LastMatches).Any()) {
                //     return true;
                // }

                if(!IsBusy &&
                   !IsLoadingMatches &&
                   SelectedTuning != null &&
                   SelectedTuning.SelectedNotes.Any() &&
                   SelectedTuning.SelectedNotes.Difference(LastNotes).Any()) {
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

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void MainViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsRightDrawerOpen):
                    if(IsRightDrawerOpen) {
                        MainView.Instance.GetVisualDescendant<RightDrawerView>().GetVisualDescendants<ItemsControl>()
                            .ForEach(x => x.InvalidateAll());
                    }

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
                case nameof(IsSearchInitiating):
                    OnPropertyChanged(nameof(CanDecreaseMatchColCount));
                    OnPropertyChanged(nameof(CanIncreaseMatchColCount));
                    break;
                case nameof(SelectedPatternType):
                    InitMatchProvider();
                    break;
                case nameof(SelectedDisplayMode):

                    break;
                case nameof(IsBusy):
                    if(IsBusy) {

                    }

                    break;
            }
        }


        async Task InitAsync() {
            IsBusy = true;
            bool needs_save = false;
            if(Prefs.Instance.Instruments is not IEnumerable<Instrument> instl ||
               !instl.Any()) {
                instl = CreateDefaultInstruments();
                Prefs.Instance.Instruments.AddRange(instl);
                needs_save = true;
            }

            foreach(Instrument inst in instl) {
                InstrumentViewModel ivm = await CreateInstrumentAsync(inst);
                Instruments.Add(ivm);
            }

            if(needs_save) {
                Prefs.Instance.Save();
            }

            foreach(InstrumentViewModel inst in Instruments) {
                foreach(TuningViewModel tun in inst.Tunings) {
                    foreach(var col in tun.Tuning.Collections) {
                        Debug.WriteLine($"{tun.Tuning} {col.Key} {col.Value.SelectMany(x => x.Groups).Count()}");
                    }
                }
            }

            //TestInstruments();

            if(needs_save) {
                InitInstrument();
            }

            IsBusy = false;
        }


        #region Matches

        public void UpdateMatches(MatchUpdateSource source) {
            if(source is
               MatchUpdateSource.NoteToggle or
               MatchUpdateSource.RootToggle) {
                UpdateMatchOverlays();
                return;
            }

            Task.Run(async () => {
                await CancelMatchLoadAsync();
                var sel_notes = SelectedTuning.SelectedNotes;
                if(source is
                   MatchUpdateSource.FindClick or
                   MatchUpdateSource.InstrumentInit or
                   MatchUpdateSource.BookmarkToggle) {
                    CreateWorkingMatches(sel_notes);
                }

                LastNotes = sel_notes.ToList();
                FilteredMatches = GetFilterWorkingMatches();

                await Dispatcher.UIThread.InvokeAsync(async () => {
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

                    await LoadMatchesAsync(FilteredMatches,MatchCts.Token);

                    IsLoadingMatches = false;
                    IsSearchOverlayVisible = true;
                },DispatcherPriority.Background);
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
            if(!matches.Any()) {
                IsSearchInitiating = false;
            }

            foreach(MatchViewModelBase match in matches) {
                if(ct.IsCancellationRequested) {
                    IsLoadingMatches = false;
                    return;
                }

                Matches.Add(match);
                await Task.Delay(50,ct);
                IsSearchInitiating = false;
            }
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
            AvailableSuffixes = WorkingMatches.Select(x => x.NoteGroup.SuffixDisplayValue).Distinct();
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
                        .ThenBy(x => x.SecondaryLabel)
                        .ThenBy(x => x.NoteGroup.Position);

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
                Debug.Assert(!IsLoadingMatches,"Match load state mismatch");
                return;
            }

            await MatchCts.CancelAsync();
            // while(IsLoadingMatches) {
            //     await Task.Delay(10);
            // }

        }

        void InitMatchProvider() {
            MatchProvider = new(SelectedPatternType,SelectedTuning.Tuning);

        }

        public void DiscoverMatchColumnCount() {
            if(MatchesView.Instance is not { } mv ||
               mv.GetVisualDescendant<ItemsRepeater>() is not { } mir ||
               mir.Layout is not WrapLayout mir_wl) {
                return;
            }

            double avail_w = mir.Bounds.Width;
            int cols = (int)Math.Max(1,avail_w / MatchWidth);
            Debug.WriteLine($"Discovered {cols} columns. Was {MatchColCount}");
            MatchColCount = cols;
            OnPropertyChanged(nameof(CanDecreaseMatchColCount));
            OnPropertyChanged(nameof(CanIncreaseMatchColCount));
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

        void LoadSvgFlagsIntoOptions() {
            if(!OptionLookup.TryGetValue(OptionType.Svg,out var svg_opts)) {
                return;
            }

            foreach((string key,int idx) in Enum.GetNames(typeof(SvgFlags)).WithIndex()) {
                SvgFlags flag = (SvgFlags)Math.Pow(2,idx);
                if(svg_opts.FirstOrDefault(x => x.OptionValue == key) is { } svg_opt) {
                    svg_opt.IsChecked = Prefs.Instance.SelectedSvgFlags.HasFlag(flag);
                }
            }

        }

        void UpdateSvgFlagAvailability() {
            AvailableSvgFlags =
                Enum.GetNames(typeof(SvgFlags))
                    .Select(x => x.ToEnum<SvgFlags>())
                    .Where(x => x.IsFlagEnabled(SelectedInstrument.InstrumentType,SelectedPatternType));
        }

        void UpdateSvgFlagsFromOptions() {
            int old_svg_val = (int)Prefs.Instance.SelectedSvgFlags;
            int new_svg_val = 0;
            foreach((string key,int idx) in Enum.GetNames(typeof(SvgFlags)).WithIndex()) {
                if(SelectedSvgOptions.Any(x => x.OptionValue == key)) {
                    new_svg_val |= (int)Math.Pow(2,idx);
                }
            }

            if(new_svg_val != old_svg_val) {
                Prefs.Instance.SelectedSvgFlags = (SvgFlags)new_svg_val;
                Prefs.Instance.Save();
            }
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
                    { OptionType.Svg,(typeof(SvgFlags),-1) }
                };
                all_opts.AddRange(
                    opt_lookup.SelectMany(
                        x =>
                            Enum.GetNames(x.Value.Item1).Select(
                                (y,idx) => new OptionViewModel
                                {
                                    OptionType = x.Key,
                                    OptionValue = y,
                                    Label = y,
                                    IsChecked = x.Value.Item2 == idx
                                })));
            }


            foreach(var kvp in OptionLookup) {
                kvp.Value.Clear();
                kvp.Value.AddRange(all_opts.Where(x => x.OptionType == kvp.Key));
            }

            LoadSvgFlagsIntoOptions();

            OnPropertyChanged(nameof(DisplayModeOptions));
            OnPropertyChanged(nameof(PatternOptions));
            OnPropertyChanged(nameof(KeyOptions));
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));

            UpdateSvgFlagAvailability();

            OptionLookup.SelectMany(x => x.Value).ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
            OptionLookup.SelectMany(x => x.Value).ForEach(x => x.OnPropertyChanged(nameof(x.IsEnabled)));

        }

        #endregion

        #region Instruments

        void TestInstruments() {

            if(Instruments
                   .FirstOrDefault(x => x.Instrument.InstrumentType == InstrumentType.Guitar) is { } guitar_vm) {
                if(guitar_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Open D") is not { } open_d) {
                    guitar_vm.AddTuningCommand.Execute(
                        new object[]
                        {
                            new[] { "D3","A3","D3","F#3","A3","D4" }.Select(x => Note.Parse(x)),
                            0,
                            "Open D"
                        });
                } else if(guitar_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Standard") is { } std) {
                    std.IsSelected = false;
                    std.IsSelected = true;
                }
            }

            if(Instruments
                   .FirstOrDefault(x => x.Instrument.InstrumentType == InstrumentType.Ukulele) is { } ukulele_vm) {
                SelectedInstrument = ukulele_vm;
                if(ukulele_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Standard") is { } std) {
                    std.IsSelected = false;
                    std.IsSelected = true;
                }
            }

            if(Instruments
                   .FirstOrDefault(x => x.Instrument.InstrumentType == InstrumentType.Piano) is { } piano_vm) {
                SelectedInstrument = piano_vm;
                if(piano_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Standard") is { } std) {
                    std.IsSelected = false;
                    std.IsSelected = true;
                }
            }
        }

        void TestSvg() {

            new PianoSvgBuilder().Test(
                SelectedTuning.Tuning,SelectedTuning.Tuning.Chords.SelectMany(x => x.Groups));
            new PianoSvgBuilder().Test(
                SelectedTuning.Tuning,SelectedTuning.Tuning.Scales.SelectMany(x => x.Groups));
        }

        async Task<InstrumentViewModel> CreateInstrumentAsync(Instrument instrument) {
            InstrumentViewModel ivm = new InstrumentViewModel(this);
            await ivm.InitAsync(instrument);
            return ivm;
        }

        void InitSelection() {
            if(SelectedInstrument == null) {
                if(Instruments.FirstOrDefault(x => x.Tunings.Any(y => y.Id == Prefs.Instance.SelectedTuningId))
                   is { } sel_inst) {
                    SelectedInstrument = sel_inst;
                    if(SelectedTuning == null) {
                    }
                } else {
                    SelectedInstrument = DefaultInstrument;
                }
            }

            SelectedInstrumentIndex = Instruments.IndexOf(SelectedInstrument);
            LastSelectedTuning = SelectedTuning;
            ResetInstrumentCommand.Execute(null);
        }

        void InitInstrument() {

            //TestSvg();

            InitSelection();
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

        public static IEnumerable<Instrument> CreateDefaultInstruments() {
            var instl = new List<Instrument>();
            var std_inst_lookup = new Dictionary<InstrumentType,(string[],int,double?)>
            {
                { InstrumentType.Guitar,(["E2","A2","D3","G3","B3","E4"],23,25.5d) },
                { InstrumentType.Ukulele,(["G4","C4","E4","A4"],15,13d) },
                { InstrumentType.Piano,(["C3"],24,null) }
            };
            foreach(var kvp in std_inst_lookup) {
                Tuning tuning = new Tuning("Standard",true,true);
                tuning.Id = Guid.NewGuid().ToString();
                tuning.OpenNotes.AddRange(
                    kvp.Value.Item1.Select(
                        (x,idx) =>
                            new InstrumentNote(
                                0,
                                idx,
                                Note.Parse(x))));
                Instrument inst = new Instrument(
                    kvp.Key.ToString(),kvp.Key,kvp.Value.Item2,kvp.Value.Item1.Length,kvp.Value.Item3);
                inst.Tunings.Add(tuning);
                instl.Add(inst);
            }

            return instl;
        }

        #endregion

        #endregion

        #region Commands

        public ICommand DecreaseMatchColumnsCommand => new MpAsyncCommand(async () => {
            // plus btn
            IsMatchZoomChanging = true;
            await SetMatchColumnCountAsync(MatchColCount - 1);
            IsMatchZoomChanging = false;
        });

        public ICommand IncreaseMatchColumnsCommand => new MpAsyncCommand(async () => {
            // minus btn
            IsMatchZoomChanging = true;
            await SetMatchColumnCountAsync(MatchColCount + 1);
            IsMatchZoomChanging = false;
        });

        public ICommand ResetInstrumentCommand => new MpCommand(() => {
            SelectedTuning.ResetSelection();
            UpdateMatchOverlays();
        });

        public ICommand CloseRightDrawerCommand => new MpCommand(() => {
            IsRightDrawerOpen = false;
        });

        public ICommand SelectOptionCommand => new MpCommand<object>(
            args => {
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
                    case OptionType.Svg:
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
                        UpdateMatches(MatchUpdateSource.FilterToggle);
                        InstrumentView.Instance.InvalidateAll();
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