using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Cairo;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

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

        public IEnumerable<OptionViewModel> PatternOptions =>
            OptionLookup[OptionType.Pattern];

        OptionViewModel SelectedPatternOption =>
            PatternOptions.FirstOrDefault(x => x.IsChecked);

        public IEnumerable<OptionViewModel> DisplayModeOptions =>
            OptionLookup[OptionType.DisplayMode];

        OptionViewModel SelectedDisplayModeOption =>
            DisplayModeOptions.FirstOrDefault(x => x.IsChecked);

        public IEnumerable<OptionViewModel> KeyOptions =>
            OptionLookup[OptionType.Key];

        IEnumerable<OptionViewModel> SelectedKeyOptions =>
            KeyOptions.Where(x => x.IsChecked);

        public IEnumerable<OptionViewModel> SvgOptions =>
            OptionLookup[OptionType.Svg];

        IEnumerable<OptionViewModel> SelectedSvgOptions =>
            SvgOptions.Where(x => x.IsChecked);

        IEnumerable<OptionViewModel> ChordSuffixOptions =>
            OptionLookup[OptionType.ChordSuffix];

        IEnumerable<OptionViewModel> ScaleSuffixOptions =>
            OptionLookup[OptionType.ScaleSuffix];

        IEnumerable<OptionViewModel> ModeSuffixOptions =>
            OptionLookup[OptionType.ModeSuffix];

        public IEnumerable<OptionViewModel> SuffixOptions =>
            SelectedPatternType switch
            {
                MusicPatternType.Chords => ChordSuffixOptions,
                MusicPatternType.Scales => ScaleSuffixOptions,
                MusicPatternType.Modes => ModeSuffixOptions,
                _ => throw new ArgumentOutOfRangeException()
            };

        IEnumerable<OptionViewModel> SelectedSuffixOptions =>
            SuffixOptions.Where(x => x.IsChecked);

        Dictionary<OptionType,IEnumerable<OptionViewModel>> OptionLookup { get; set; }

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

        #endregion

        #region State

        #region UI

        public bool IsLeftDrawerOpen { get; set; }
        public bool IsRightDrawerOpen { get; set; }

        #endregion

        #region Options

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

        bool IsDefaultSelection =>
            SelectedTuning == null ? true : SelectedTuning.NoteRows.All(x => x.IsDefaultSelection);

        #endregion

        #region Matches

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


        bool IsLoadingMatches { get; set; }
        CancellationTokenSource MatchCts { get; set; }

        public bool IsMatchesEmpty =>
            !FilteredMatches.Any();

        public bool IsSearchButtonVisible {
            get {
                // if(FilteredMatches.Difference(LastMatches).Any()) {
                //     return true;
                // }

                if(SelectedTuning != null &&
                   SelectedTuning.SelectedNotes.Any() &&
                   SelectedTuning.SelectedNotes.Difference(LastNotes).Any()) {
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
                case nameof(SelectedInstrument):
                    // if(SelectedInstrument == null) {
                    //     SelectedInstrument = DefaultInstrument;
                    // }
                    break;
                case nameof(SelectedTuning):
                    if(SelectedTuning == LastSelectedTuning) {
                        break;
                    }

                    LastSelectedTuning = SelectedTuning;
                    InitInstrumentAsync().FireAndForgetSafeAsync();
                    break;
                case nameof(SelectedInstrumentIndex):
                    OnPropertyChanged(nameof(SelectedInstrument));

                    break;
                case nameof(SelectedPatternType):
                    InitMatchProvider();
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

            Instruments.AddRange(await Task.WhenAll(instl.Select(x => CreateInstrumentAsync(x))));
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

            await InitInstrumentAsync();
            IsBusy = false;
        }


        #region Matches

        public void UpdateMatches(MatchUpdateSource source) {
            if(source == MatchUpdateSource.NoteToggle ||
               source == MatchUpdateSource.RootToggle) {
                OnPropertyChanged(nameof(IsSearchButtonVisible));
                OnPropertyChanged(nameof(EmptyMatchLabel));
                return;
            }

            Task.Run(async () => {
                await CancelMatchLoadAsync();
                var sel_notes = SelectedTuning.SelectedNotes;
                if(source == MatchUpdateSource.FindClick) {
                    CreateWorkingMatches(sel_notes);
                }

                LastNotes = sel_notes.ToList();
                FilteredMatches = GetFilterWorkingMatches();

                await Dispatcher.UIThread.InvokeAsync(async () => {
                    UpdateFilters();
                    MatchCts = new();
                    await LoadMatchesAsync(FilteredMatches,MatchCts.Token);
                },DispatcherPriority.Background);
            });
        }

        void UpdateFilters() {
            KeyOptions.ForEach(x => x.IsEnabled = AvailableKeys.Any(y => y.ToString() == x.OptionValue));
            SuffixOptions.ForEach(x => x.IsEnabled = AvailableSuffixes.Contains(x.OptionValue));
            SvgOptions.ForEach(x => x.IsEnabled = AvailableSvgFlags.Any(y => y.ToString() == x.OptionValue));

            OnPropertyChanged(nameof(KeyOptions));
            OnPropertyChanged(nameof(SuffixOptions));
            OnPropertyChanged(nameof(SvgOptions));
        }

        async Task LoadMatchesAsync(IEnumerable<MatchViewModelBase> matches,CancellationToken ct) {
            IsLoadingMatches = true;
            Matches.Clear();
            foreach(MatchViewModelBase match in matches) {
                if(ct.IsCancellationRequested) {
                    IsLoadingMatches = false;
                    return;
                }

                Matches.Add(match);
                await Task.Delay(100,ct);
            }

            IsLoadingMatches = false;
        }

        void CreateWorkingMatches(IEnumerable<NoteViewModel> notes) {
            WorkingMatches.Clear();
            WorkingMatches.AddRange(MatchProvider.GetMatches(notes));

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
            return matches
                .OrderByDescending(x => x.Score);
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

            AvailableSvgFlags =
                Enum.GetNames(typeof(SvgFlags))
                    .Select(x => x.ToEnum<SvgFlags>())
                    .Where(x => x.IsFlagEnabled(SelectedInstrument.InstrumentType,SelectedPatternType));
        }

        bool IsAutoUpdateEnabled(MatchUpdateSource source) {
            if(ThemeViewModel.Instance.IsDesktop) {
                return true;
            }

            return
                source != MatchUpdateSource.NoteToggle &&
                source != MatchUpdateSource.RootToggle;
        }

        #endregion

        #region Options

        void InitOptions() {
            /*
             public ObservableCollection<OptionViewModel<MusicPatternType>> PatternOptions { get; } = [];
               public ObservableCollection<OptionViewModel<DisplayModeType>> DisplayModeOptions { get; } = [];
               public ObservableCollection<OptionViewModel<string>> SuffixOptions { get; } = [];
               public ObservableCollection<OptionViewModel<NoteType>> KeyOptions { get; } = [];
               public ObservableCollection<OptionViewModel<SvgFlags>> SvgOptions { get; } = [];
             */
            var all_opts = Prefs.Instance.Options;
            if(!all_opts.Any()) {
                var opt_lookup = new Dictionary<OptionType,Type>
                {
                    { OptionType.Pattern,typeof(PatternType) },
                    { OptionType.DisplayMode,typeof(DisplayModeType) },
                    { OptionType.ChordSuffix,typeof(ChordSuffixType) },
                    { OptionType.ScaleSuffix,typeof(ScaleSuffixType) },
                    { OptionType.ModeSuffix,typeof(ModeSuffixType) },
                    { OptionType.Key,typeof(NoteType) },
                    { OptionType.Svg,typeof(SvgFlags) }
                };
                all_opts.AddRange(
                    opt_lookup.SelectMany(
                        x =>
                            Enum.GetNames(x.Value).Select(
                                (y,idx) => new OptionViewModel
                                {
                                    OptionType = x.Key,
                                    OptionValue = y,
                                    Label = y
                                })));
            }

            OptionLookup = all_opts.GroupBy(x => x.OptionType).ToDictionary(x => x.Key,x => x.Select(y => y));
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
        }

        async Task InitInstrumentAsync() {
            IsBusy = true;
            await Task.Delay(1);

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

            MainView.Instance.InvalidateAll();
            IsBusy = false;
        }

        IEnumerable<Instrument> CreateDefaultInstruments() {
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

        public ICommand SelectDisplayModeCommand => new MpCommand<object>(
            args => {
            });

        public ICommand SelectOptionCommand => new MpCommand<object>(
            args => {
            });

        public ICommand FindMatchesCommand => new MpCommand(
            () => {
                UpdateMatches(MatchUpdateSource.FindClick);
            });

        #endregion

    }

}