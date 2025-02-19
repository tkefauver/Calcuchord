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

        protected IEnumerable<PatternKeyCollection> Items =>
            Tuning.Collections[PatternType];

        protected abstract MusicPatternType PatternType { get; }
        protected Tuning Tuning { get; }

        #endregion

        #region Events

        #endregion

        #region Constructors

        protected MatchProviderBase(Tuning tuning) {
            Tuning = tuning;
        }

        #endregion

        #region Public Methods

        public abstract IEnumerable<MatchViewModel> GetResults(IEnumerable<NoteViewModel> frets);

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}