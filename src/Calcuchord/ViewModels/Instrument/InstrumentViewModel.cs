using System.Collections.ObjectModel;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class InstrumentViewModel : ViewModelBase<MainViewModel> {
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

        public ObservableCollection<InstrumentTuningViewModel> Tunings { get; } = [];

        public InstrumentTuningViewModel SelectedTuning {
            get => Tunings.OrderByDescending(x => x.Tuning.LastSelectedDt).FirstOrDefault();
            set {
                Tunings.ForEach(x => x.IsSelected = value == x);
                OnPropertyChanged(nameof(SelectedTuning));
            }
        }

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsSelected { get; set; }

        #endregion

        #region Model

        public Instrument Instrument { get; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public InstrumentViewModel(MainViewModel parent,Instrument it) : base(parent) {
            Instrument = it;
            Tunings.AddRange(Instrument.Tunings.Select(x => new InstrumentTuningViewModel(this,x)));
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion
    }
}