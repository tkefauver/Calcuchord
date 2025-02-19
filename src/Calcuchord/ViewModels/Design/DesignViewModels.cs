using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class DesignMainViewModel : MainViewModel {
    }

    public class DesignChordMatchViewModel : MatchViewModel {
        public DesignChordMatchViewModel() {
            PatternType = MusicPatternType.Chords;

            var instl = new[]
                { Instrument.CreateByType(InstrumentType.Guitar) }; //MainViewModel.CreateDefaultInstruments();

            PatternKeyCollection ngc = new PatternKeyCollection(MusicPatternType.Chords,NoteType.C,"Major");
            ngc.SetParent(
                instl.FirstOrDefault(x => x.InstrumentType == InstrumentType.Guitar)?.Tunings.FirstOrDefault());

            NotePattern = new NotePattern(
                ngc,0,new List<PatternNote>
                {
                    PatternNote.Create(0,InstrumentNote.Mute(0)),
                    PatternNote.Create(3,InstrumentNote.Create(3,1,NoteType.C,2)),
                    PatternNote.Create(2,InstrumentNote.Create(2,2,NoteType.E,3)),
                    PatternNote.Create(0,InstrumentNote.Create(0,3,NoteType.G,3)),
                    PatternNote.Create(1,InstrumentNote.Create(1,4,NoteType.C,4)),
                    PatternNote.Create(0,InstrumentNote.Create(0,5,NoteType.E,4))
                });
            NotePattern.SetParent(ngc);
            instl.ForEach(x => x.RefreshModelTree());
        }
    }

    public class DesignInstrumentViewModel : InstrumentViewModel {
        public DesignInstrumentViewModel() {
            Instrument = new()
            {
                InstrumentType = InstrumentType.Guitar
            };
            Instrument = Instrument.CreateByType(InstrumentType.Banjo);
            SelectedInstrumentTypeIndex = 1;
        }
    }
}