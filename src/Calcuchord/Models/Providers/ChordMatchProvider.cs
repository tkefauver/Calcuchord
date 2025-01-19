using System.Collections.Generic;
using System.Linq;

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

        public ChordMatchProvider(InstrumentTuning tuning) : base(tuning) {
            if(!tuning.Chords.Any()) {

            }
        }

        #endregion

        #region Public Methods

        public override IEnumerable<MatchViewModelBase> GetResults(IEnumerable<FretViewModel> frets) {
            return null;
        }

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion
    }
}