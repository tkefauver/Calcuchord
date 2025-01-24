using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cairo;
using MonkeyPaste.Common;

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

        #endregion

        #region View Models

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

        TuningViewModel LastSelectedTuning { get; set; }

        public TuningViewModel SelectedTuning =>
            SelectedInstrument == null ? null : SelectedInstrument.SelectedTuning;

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

        public IEnumerable<OptionViewModel> SvgOptions =>
            OptionLookup[OptionType.Svg];

        IEnumerable<OptionViewModel> ChordSuffixOptions =>
            OptionLookup[OptionType.ChordSuffix];

        IEnumerable<OptionViewModel> ScaleSuffixOptions =>
            OptionLookup[OptionType.ScaleSuffix];

        IEnumerable<OptionViewModel> ModeSuffixOptions =>
            OptionLookup[OptionType.ModeSuffix];

        public IEnumerable<OptionViewModel> SuffixOptions =>
            SelectedPatternType switch {
                MusicPatternType.Chords => ChordSuffixOptions,
                MusicPatternType.Scales => ScaleSuffixOptions,
                MusicPatternType.Modes => ModeSuffixOptions,
                _ => throw new ArgumentOutOfRangeException()
            };

        Dictionary<OptionType,IEnumerable<OptionViewModel>> OptionLookup { get; set; }

        #endregion

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public int SelectedInstrumentIndex {
            get => Instruments.IndexOf(SelectedInstrument);
            set {
                if(value >= 0 && value < Instruments.Count) {
                    SelectedInstrument = Instruments[value];
                    OnPropertyChanged(nameof(SelectedInstrument));
                }
            }
        }


        public MusicPatternType SelectedPatternType =>
            SelectedPatternOption.OptionValue.ToEnum<MusicPatternType>();

        public DisplayModeType SelectedDisplayMode =>
            SelectedDisplayModeOption.OptionValue.ToEnum<DisplayModeType>();

        public InstrumentType SelectedInstrumentType =>
            SelectedInstrument.Instrument.InstrumentType;

        public bool IsPianoSelected =>
            SelectedInstrumentType == InstrumentType.Piano;

        public bool IsInstrumentVisible =>
            SelectedDisplayMode == DisplayModeType.Search;

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
            if(Prefs.Instance.Instruments is not IEnumerable<Instrument> instl ||
               !instl.Any()) {
                instl = CreateDefaultInstruments();
                Prefs.Instance.Instruments.AddRange(instl);
            }

            instl.ForEach(x => Instruments.Add(new(this,x)));

            // if(Instruments
            //        .FirstOrDefault(x => x.Instrument.InstrumentType == InstrumentType.Guitar) is { } guitar_vm) {
            //     if(guitar_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Open D") is not { } open_d) {
            //         guitar_vm.AddTuningCommand.Execute(
            //             new object[] {
            //                 new[] { "D3","A3","D3","F#3","A3","D4" }.Select(x => Note.Parse(x)),
            //                 0,
            //                 "Open D"
            //             });
            //     } else 
            //     if(guitar_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Standard") is { } std) {
            //         std.IsSelected = false;
            //         std.IsSelected = true;
            //     }
            // }

            // if(Instruments
            //        .FirstOrDefault(x => x.Instrument.InstrumentType == InstrumentType.Ukulele) is { } ukulele_vm) {
            //     SelectedInstrument = ukulele_vm;
            //     if(ukulele_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Standard") is { } std) {
            //         std.IsSelected = false;
            //         std.IsSelected = true;
            //     }
            // }
            // if(Instruments
            //        .FirstOrDefault(x => x.Instrument.InstrumentType == InstrumentType.Piano) is { } piano_vm) {
            //     SelectedInstrument = piano_vm;
            //     if(piano_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Standard") is { } std) {
            //         std.IsSelected = false;
            //         std.IsSelected = true;
            //     }
            // }

            //InitInstrumentAsync().FireAndForgetSafeAsync();
            InitOptions();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void MainViewModel_OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
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
            }
        }

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
                Dictionary<OptionType,Type> opt_lookup = new() {
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
                                (y,idx) => new OptionViewModel {
                                    IsChecked = idx == 0,
                                    OptionType = x.Key,
                                    OptionValue = y,
                                    Label = y
                                })));
            }
            OptionLookup = all_opts.GroupBy(x => x.OptionType).ToDictionary(x => x.Key,x => x.Select(y => y));
        }

        #endregion

        #region Instruments

        async Task InitInstrumentAsync() {
            if(!SelectedTuning.IsLoaded) {
                await SelectedTuning.InitCollectionsAsync();
            }

            OnPropertyChanged(nameof(SelectedInstrument));
            OnPropertyChanged(nameof(SelectedTuning));
            OnPropertyChanged(nameof(IsPianoSelected));

            if(false) {
                new PianoSvgBuilder().Test(
                    SelectedTuning.Tuning,SelectedTuning.Tuning.Chords.SelectMany(x => x.Groups));
                new PianoSvgBuilder().Test(
                    SelectedTuning.Tuning,SelectedTuning.Tuning.Scales.SelectMany(x => x.Groups));
            }
        }

        IEnumerable<Instrument> CreateDefaultInstruments() {
            var instl = new List<Instrument>();
            var std_inst_lookup = new Dictionary<InstrumentType,(string[],int,double?)> {
                { InstrumentType.Guitar,(["E2","A2","D3","G3","B3","E4"],23,25.5d) },
                { InstrumentType.Ukulele,(["G4","C4","E4","A4"],15,13d) },
                { InstrumentType.Piano,(["C3"],24,null) }
            };
            foreach(var kvp in std_inst_lookup) {
                Tuning tuning = new(
                    "Standard",
                    true,
                    true);
                tuning.OpenNotes.AddRange(
                    kvp.Value.Item1.Select(
                        (x,idx) =>
                            new InstrumentNote(
                                0,
                                idx,
                                Note.Parse(x))));
                Instrument inst = new(
                    kvp.Key.ToString(),
                    kvp.Key,
                    kvp.Value.Item2,
                    kvp.Value.Item1.Length,
                    kvp.Value.Item3);
                inst.Tunings.Add(tuning);
                if(kvp.Key == InstrumentType.Guitar) {
                    // select guitar by default
                    inst.IsSelected = true;
                    tuning.IsSelected = true;
                }
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

        #endregion

    }

    public class DesignMainViewModel : MainViewModel {
    }

    public class DesignFretboardTuningViewModel : TuningViewModel {
        public DesignFretboardTuningViewModel() {
            Instrument inst = new() {
                InstrumentType = InstrumentType.Guitar,
                FretCount = 23,
                StringCount = 6
            };

            Tuning tuning = new() {
                Name = "Standard",
                IsSelected = true
            };
            tuning.SetParent(inst);
            tuning.OpenNotes.AddRange(
                new[] { "E2","A2","D3","G3","B3","E4" }.Select(
                    (x,idx) => new InstrumentNote(0,idx,Note.Parse(x))));

            DesignMainViewModel mvm = new();
            InstrumentViewModel instvm = new(mvm,inst);
            Parent = instvm;
            Init(tuning);
        }
    }

}