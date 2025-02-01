using System.Collections.Generic;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class Instrument {

        #region Properties

        #region Members

        public string Name { get; set; }

        public InstrumentType InstrumentType { get; set; }
        public double? NeckLengthInInches { get; set; }


        public int FretCount { get; set; }


        public int StringCount { get; set; }


        public List<Tuning> Tunings { get; set; } = [];

        #endregion

        #region Ignored

        #endregion

        #endregion

        #region Constructors

        public Instrument() {
        }

        public Instrument(string name,InstrumentType it,int fretCount,int stringCount,
            double? neckLengthInInches = null) : this() {
            Name = name;
            InstrumentType = it;
            FretCount = fretCount;
            StringCount = stringCount;
            NeckLengthInInches = neckLengthInInches;
        }

        #endregion

        #region Public Methods

        public void RefreshModelTree() {
            Tunings.ForEach(x => x.SetParent(this));
        }

        public override string ToString() {
            return InstrumentType.ToString();
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

    }
}