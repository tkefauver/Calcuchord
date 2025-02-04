using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class DesignMainViewModel : MainViewModel {
    }

    public class DesignChordMatchViewModel : ChordMatchViewModel {
        public DesignChordMatchViewModel() {
            var instl = MainViewModel.CreateDefaultInstruments();

            NoteGroupCollection ngc = new NoteGroupCollection(MusicPatternType.Chords,NoteType.C,"Major");
            ngc.SetParent(
                instl.FirstOrDefault(x => x.InstrumentType == InstrumentType.Guitar)?.Tunings.FirstOrDefault());

            NoteGroup = new(
                ngc,0,new List<PatternNote>
                {
                    new PatternNote(0,InstrumentNote.Mute(0)),
                    new PatternNote(3,new(3,1,NoteType.C,2)),
                    new PatternNote(2,new(2,2,NoteType.E,3)),
                    new PatternNote(0,new(0,3,NoteType.G,3)),
                    new PatternNote(1,new(1,4,NoteType.C,4)),
                    new PatternNote(0,new(0,5,NoteType.E,4))
                });
            NoteGroup.SetParent(ngc);
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