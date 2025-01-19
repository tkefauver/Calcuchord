using System;
using System.ComponentModel;

namespace Calcuchord {
    public class InstrumentTuningViewModel : ViewModelBase<InstrumentViewModel> {
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

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsSelected {
            get => Parent.SelectedTuning == this;
            set {
                if(value) {
                    Tuning.LastSelectedDt = DateTime.Now;
                    HasModelChanged = true;
                }

                OnPropertyChanged(nameof(IsSelected));
            }
        }

        #endregion

        #region Model

        public InstrumentTuning Tuning { get; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public InstrumentTuningViewModel(InstrumentViewModel parent,InstrumentTuning tuning) : base(parent) {
            Tuning = tuning;
            PropertyChanged += OnPropertyChanged;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        void OnPropertyChanged(object sender,PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):

                    break;
            }
        }

        #endregion

        #region Commands

        #endregion
    }
}