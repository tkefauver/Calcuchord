using System.Collections.Generic;

namespace Calcuchord.Browser {

    public class MidiPlayer_browser : MidiWebPlayerBase {


        int GetTone(Note note) {
            return note.NoteId;
        }

        protected override int GetMidiNote(Note note) {
            return note.NoteId;
        }

        public override void PlayChord(IEnumerable<IEnumerable<int>> notes) {
            JsInterop.PlayChord(GetParam(notes));
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> notes) {
            JsInterop.PlayScale(GetParam(notes));
        }
    }

}