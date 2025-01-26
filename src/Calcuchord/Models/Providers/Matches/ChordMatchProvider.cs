using System.Collections.Generic;

namespace Calcuchord {
    public class ChordMatchProvider : MatchProviderBase {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        protected override MusicPatternType PatternType => MusicPatternType.Chords;

        #endregion

        #region Events

        #endregion

        #region Constructors

        public ChordMatchProvider(Tuning tuning) : base(tuning) {
        }

        #endregion

        #region Public Methods

        public override IEnumerable<MatchViewModelBase> GetResults(IEnumerable<NoteViewModel> frets) {
            return null;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}