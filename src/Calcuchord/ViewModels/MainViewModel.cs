using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Instance = this;
            if(Prefs.Instance.Instruments is not IEnumerable<Instrument> instl ||
               !instl.Any()) {
                instl = CreateDefaultInstruments();
                Prefs.Instance.Instruments.AddRange(instl);
            }

            instl.ForEach(x => Instruments.Add(new(this,x)));

            InitInstrumentAsync().FireAndForgetSafeAsync();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        async Task InitInstrumentAsync() {
            if(!SelectedTuning.IsLoaded) {
                await SelectedTuning.InitCollectionsAsync();
            }

            OnPropertyChanged(nameof(SelectedInstrument));
            OnPropertyChanged(nameof(SelectedTuning));
            new ChordSvgBuilder().Test(SelectedTuning.Tuning.Chords.SelectMany(x => x.Groups));
        }

        IEnumerable<Instrument> CreateDefaultInstruments() {
            var instl = new List<Instrument>();
            var std_inst_lookup = new Dictionary<InstrumentType,(string[],int)> {
                { InstrumentType.Guitar,(["E2","A2","D3","G3","B3","E4"],23) },
                { InstrumentType.Ukulele,(["G4","C4","E4","A4"],15) },
                { InstrumentType.Piano,(["C3"],24) }
            };
            foreach(var kvp in std_inst_lookup) {
                InstrumentTuning tuning = new(
                    "Standard",
                    isDefault: true,
                    isReadOnly: true);
                tuning.OpenNotes.AddRange(
                    kvp.Value.Item1.Select(
                        (x,idx) =>
                            new InstrumentNote(
                                fretNum: 0,
                                idx,
                                Note.Parse(x))));
                Instrument inst = new(
                    kvp.ToString(),
                    kvp.Key,
                    kvp.Value.Item2,
                    kvp.Value.Item1.Length);
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