using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace Calcuchord.Browser {
    public class MidiPlayer_browser : MidiPlayerBase {


        int GetTone(Note note) {
            return note.NoteId;
        }

        protected override int GetMidiNote(Note note) {
            return note.NoteId;
        }

        public override void PlayChord(IEnumerable<Note> notes) {
            EmbedMidiInterop.PlayChord(GetMidiNotes(notes));
        }

        public override void PlayScale(IEnumerable<Note> notes) {
            EmbedMidiInterop.PlayScale(GetMidiNotes(notes));
        }

        public override void StopPlayback() {
            EmbedMidiInterop.StopPlayback();
            TriggerStopped();
        }
    }

    public static partial class EmbedMidiInterop {
        [JSImport("globalThis.window.playChord")]
        public static partial void PlayChord(int[] notes);

        [JSImport("globalThis.window.playScale")]
        public static partial void PlayScale(int[] notes);

        [JSImport("globalThis.window.stopPlayback")]
        public static partial void StopPlayback();
    }
}