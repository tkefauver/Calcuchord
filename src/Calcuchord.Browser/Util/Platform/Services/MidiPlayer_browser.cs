using System.Collections.Generic;

namespace Calcuchord.Browser {

    public class MidiPlayer_browser : MidiPlayerBase {


        int GetTone(Note note) {
            return note.NoteId;
        }

        protected override int GetMidiNote(Note note) {
            return note.NoteId;
        }

        public override void PlayChord(IEnumerable<Note> notes) {
            JsInterop.PlayChord(GetMidiNotes(notes));
        }

        public override void PlayScale(IEnumerable<Note> notes) {
            JsInterop.PlayScale(GetMidiNotes(notes));
        }
    }

}