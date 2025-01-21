namespace Calcuchord {
    public class FretViewModel : ViewModelBase<StringRowViewModel> {
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

        public InstrumentNote FretNote { get; private set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public FretViewModel(StringRowViewModel parent,InstrumentNote fretNote) : base(parent) {
            FretNote = fretNote;
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