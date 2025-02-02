using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Calcuchord {
    [JsonObject]
    public class Instrument {

        #region Statics

        public static Instrument CreateByType(InstrumentType it,bool readOnlyTuning = false,string name = null) {
            string tuning_str = string.Empty;
            int fret_count = 23;
            double? neck_len = null;
            switch(it) {
                case InstrumentType.Guitar:
                    tuning_str = "E2 A2 D3 G3 B3 E4";
                    break;
                case InstrumentType.Mandolin:
                    tuning_str = "G3 D4 A4 E5";
                    break;
                case InstrumentType.Ukulele:
                    tuning_str = "G4 C4 E4 A4";
                    fret_count = 15;
                    break;
                case InstrumentType.Banjo:
                    tuning_str = "G3 D2 G2 B2 D3";
                    fret_count = 22;
                    break;
                case InstrumentType.Bass:
                    tuning_str = "E1 A1 D2 G2";
                    break;
                case InstrumentType.Balalaika:
                    tuning_str = "E4 E4 A4";
                    break;
                case InstrumentType.Lute:
                    tuning_str = "G2 C3 F3 A3 D4 G4";
                    fret_count = 26;
                    break;
                case InstrumentType.Violin:
                    tuning_str = "G3 D4 A4 E5";
                    fret_count = 29;
                    break;
                case InstrumentType.Cello:
                    tuning_str = "C2 G2 D3 A3";
                    fret_count = 29;
                    break;
                case InstrumentType.Piano:
                    tuning_str = "C3";
                    fret_count = 24;
                    break;
            }

            Tuning tuning = new Tuning(name ?? "Standard",true,readOnlyTuning);
            tuning.CreateId();
            tuning.OpenNotes.AddRange(tuning_str.Split(" ").Select((x,idx) => new InstrumentNote(0,idx,Note.Parse(x))));
            Instrument inst = new Instrument(it.ToString(),it,fret_count,tuning.OpenNotes.Count,neck_len);
            tuning.SetParent(inst);
            inst.Tunings.Add(tuning);
            return inst;
        }

        #endregion

        #region Properties

        #region Members

        public string Name { get; set; } = string.Empty;
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