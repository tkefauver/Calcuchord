using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class StringRowViewModel : ViewModelBase<TuningViewModel> {

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

        public ObservableCollection<FretViewModel> Frets { get; } = [];

        public FretViewModel SelectedFret {
            get => SelectedFrets.FirstOrDefault();
            set {
                Frets.ForEach(x => x.IsSelected = x == value);
                OnPropertyChanged(nameof(SelectedFret));
                OnPropertyChanged(nameof(SelectedFrets));
            }
        }

        public IEnumerable<FretViewModel> SelectedFrets {
            get => Frets.Where(x => x.IsSelected);
            set {
                Frets.ForEach(x => x.IsSelected = value == null ? false : value.Contains(x));
                OnPropertyChanged(nameof(SelectedFret));
                OnPropertyChanged(nameof(SelectedFrets));
            }
        }

        FretViewModel OpenFret =>
            Frets.FirstOrDefault(x => x.FretNum == 0);

        FretViewModel DefaultFret =>
            OpenFret;

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsDefaultSelection =>
            SelectedFrets.All(x => x == DefaultFret) &&
            DefaultFret.IsDefaultState;

        public int StringNum =>
            OpenNote == null ? -1 : OpenNote.StringNum;

        public int FretCount =>
            Parent.Tuning.FretCount;

        #endregion

        #region Model

        public InstrumentNote OpenNote { get; set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public StringRowViewModel() {
        }

        public StringRowViewModel(TuningViewModel parent,InstrumentNote openNote) : base(parent) {
            Init(openNote);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        public void Init(InstrumentNote openNote) {
            OpenNote = openNote;

            int min_fret_num = Parent.Parent.IsKeyboard ? 0 : -1;
            Enumerable.Range(min_fret_num,Parent.LogicalFretCount)
                .ForEach(
                    x => Frets.Add(
                        new(
                            this,
                            new(x,StringNum,OpenNote == null ? null : OpenNote.Offset(x)))));
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}