using System.Collections.Generic;

namespace Calcuchord {
    public abstract class MatchProviderBase {
        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        protected abstract MusicPatternType PatternType { get; }

        #endregion

        #region Events

        #endregion

        #region Constructors

        public MatchProviderBase(InstrumentTuning tuning) {
        }

        #endregion

        #region Public Methods

        public abstract IEnumerable<MatchViewModelBase> GetResults(IEnumerable<FretViewModel> frets);

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion
    }
}