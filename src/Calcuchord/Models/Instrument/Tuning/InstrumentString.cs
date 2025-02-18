using System.Collections.Generic;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class InstrumentString {

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

        [JsonProperty]
        public InstrumentNote BaseNote { get; set; }

        #endregion

        #region Ignored

        [JsonIgnore]
        public int BaseNoteNum =>
            BaseNote.ColNum;

        [JsonIgnore]
        public Tuning Parent { get; private set; }

        [JsonIgnore]
        public List<InstrumentNote> Notes { get; private set; }

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public InstrumentString(Tuning parent,InstrumentNote baseNote) {
            Parent = parent;
            BaseNote = baseNote;

        }

        #endregion

        #region Public Methods

        public void SetParent(Tuning tuning) {
            Parent = tuning;
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