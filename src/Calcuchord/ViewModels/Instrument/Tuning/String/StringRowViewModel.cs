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

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        #endregion

        #region Model

        public InstrumentNote OpenNote { get; private set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public StringRowViewModel(TuningViewModel parent,InstrumentNote openNote) : base(parent) {
            OpenNote = openNote;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion
    }
}