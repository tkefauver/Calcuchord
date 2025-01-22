using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

        public ObservableCollection<InstrumentViewModel> Instruments { get; } = [];

        public InstrumentViewModel SelectedInstrument {
            get => Instruments.OrderByDescending(x => x.SelectedTuning.Tuning.LastSelectedDt).FirstOrDefault();
            set {
                Instruments.ForEach(x => x.IsSelected = value == x);
                OnPropertyChanged(nameof(SelectedInstrument));
            }
        }

        TuningViewModel LastSelectedTuning { get; set; }

        public TuningViewModel SelectedTuning =>
            SelectedInstrument.SelectedTuning;

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

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

            if(Instruments.FirstOrDefault(x => x.Instrument.InstrumentType == InstrumentType.Guitar) is { } guitar_vm) {
                // if(guitar_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Open D") is not { } open_d) {
                //     guitar_vm.AddTuningCommand.Execute(
                //         new object[] {
                //             new[] { "D3","A3","D3","F#3","A3","D4" }.Select(x => Note.Parse(x)),
                //             0,
                //             "Open D"
                //         });
                // } else 
                if(guitar_vm.Tunings.FirstOrDefault(x => x.Tuning.Name == "Standard") is { } std) {
                    std.IsSelected = false;
                    std.IsSelected = true;
                }
                //open_d.Tuning.ClearCollections();
            }

            //InitInstrumentAsync().FireAndForgetSafeAsync();
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
            }
        }

        async Task InitInstrumentAsync() {
            if(!SelectedTuning.IsLoaded) {
                await SelectedTuning.InitCollectionsAsync();
            }

            OnPropertyChanged(nameof(SelectedInstrument));
            OnPropertyChanged(nameof(SelectedTuning));

            new ChordSvgBuilder().Test(SelectedTuning.Tuning,SelectedTuning.Tuning.Chords.SelectMany(x => x.Groups));
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
                    kvp.ToString(),
                    kvp.Key,
                    kvp.Value.Item2,
                    kvp.Value.Item1.Length,
                    kvp.Value.Item3);
                inst.Tunings.Add(tuning);
                instl.Add(inst);
            }

            return instl;
        }

        #endregion

        #region Commands

        #endregion

    }
}