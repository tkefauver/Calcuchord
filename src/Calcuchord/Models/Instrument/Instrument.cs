using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Calcuchord {
    [JsonObject]
    public class Instrument {

        #region Constants

        public const string STANDARD_GUITAR_TUNING_ID = "4c0cc5d9-da2e-4b99-b18d-1f444b54af2e";
        public const string STANDARD_UKULELE_TUNING_ID = "a66e4b0b-857b-44f3-ae2e-a39634019c41";
        public const string STANDARD_PIANO_TUNING_ID = "380b83b7-6882-4ccb-9c5b-9407a715d00f";

        #endregion

        #region Statics

        public static Instrument CreateByType(
            InstrumentType it,
            bool readOnlyTuning = false,
            string name = null,
            int capoNum = 0,
            double? neckLen = null,
            string id = null,
            bool chordsFromFile = false,
            bool isInstrumentSelected = false,
            bool isDefTuningSelected = true) {
            string tuning_str = string.Empty;
            int fret_count = 23;
            switch(it) {
                default:
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
                    //tuning_str = "G3 D2 G2 B2 D3";
                    tuning_str = "D2 G2 B2 D3";
                    fret_count = 22;
                    break;
                case InstrumentType.Bass:
                    tuning_str = "E1 A1 D2 G2";
                    break;
                case InstrumentType.Balalaika:
                    tuning_str = "E4 E4 A4";
                    fret_count = 16;
                    break;
                case InstrumentType.Lute:
                    tuning_str = "G2 C3 F3 A3 D4 G4";
                    fret_count = 26;
                    break;
                case InstrumentType.Violin:
                    tuning_str = "G3 D4 A4 E5";
                    fret_count = 29;
                    break;
                case InstrumentType.Viola:
                    tuning_str = "C3 G3 D4 A4";
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

            var open_notes = tuning_str.Split(" ").Select(
                (x,idx) => new InstrumentNote(
                    0,
                    idx,
                    Note.Parse(x))).ToArray();

            Instrument inst = new Instrument(
                name ?? it.ToString(),
                it,
                fret_count,
                open_notes.Length,
                neckLen);
            inst.IsSelected = isInstrumentSelected;

            Tuning tuning = new Tuning(
                "Standard",
                readOnlyTuning,
                capoNum);
            tuning.IsChordsFromFile = chordsFromFile;
            tuning.IsSelected = isDefTuningSelected;
            tuning.OpenNotes.AddRange(open_notes);
            tuning.SetParent(inst);
            inst.Tunings.Add(tuning);

            return inst;
        }

        #endregion

        #region Properties

        #region Members

        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public InstrumentType InstrumentType { get; set; }

        [JsonProperty]
        public bool IsSelected { get; set; }

        [JsonProperty]
        public double? NeckLengthInInches { get; set; }

        [JsonProperty]
        public int ColCount { get; set; }

        [JsonProperty]
        public int RowCount { get; set; }

        [JsonProperty]
        public List<Tuning> Tunings { get; set; } = [];

        #endregion

        #region Ignored

        #endregion

        #endregion

        #region Constructors

        public Instrument() {
        }

        public Instrument(string name,InstrumentType it,int colCount,int rowCount,
            double? neckLengthInInches = null) : this() {
            Name = name;
            InstrumentType = it;
            ColCount = colCount;
            RowCount = rowCount;
            NeckLengthInInches = neckLengthInInches;
        }

        #endregion

        #region Public Methods

        public Instrument Clone() {
            Instrument clone = new Instrument(Name,InstrumentType,ColCount,RowCount,NeckLengthInInches);
            clone.Tunings.AddRange(Tunings.Select(tuning => tuning.Clone()));
            clone.RefreshModelTree();
            return clone;
        }

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