using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

namespace Calcuchord.Browser {
    public class BrowserMidiPlayer : IMidiPlayer {

        public bool CanPlay => true;

        public event EventHandler Stopped;

        public void Init(object obj) {
        }

        int GetTone(Note note) {
            return note.NoteId;
        }

        public void PlayChord(IEnumerable<Note> notes) {
            EmbedMidiInterop.PlayChord(notes.Select(x => GetTone(x)).ToArray());
        }

        public void PlayScale(IEnumerable<Note> notes) {
            EmbedMidiInterop.PlayScale(notes.Select(x => GetTone(x)).ToArray());
        }

        public void StopPlayback() {
            EmbedMidiInterop.StopPlayback();
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