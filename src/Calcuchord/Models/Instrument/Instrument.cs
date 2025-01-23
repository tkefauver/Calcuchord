using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using MonkeyPaste.Common;

namespace Calcuchord {
    [DataContract]
    public class Instrument {

        #region Properties

        #region Members

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsSelected { get; set; }

        [DataMember]
        public InstrumentType InstrumentType { get; set; }

        [DataMember]
        public double? NeckLengthInInches { get; set; }

        [DataMember]
        public int FretCount { get; set; }

        [DataMember]
        public int StringCount { get; set; }

        [DataMember]
        public ObservableCollection<Tuning> Tunings { get; set; } = [];

        #endregion

        #region Ignored

        #endregion

        #endregion

        #region Constructors

        public Instrument() {
            Id = Guid.NewGuid().ToString();
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