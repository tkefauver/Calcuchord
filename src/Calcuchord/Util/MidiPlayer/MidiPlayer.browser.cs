using System.Collections.Generic;

namespace Calcuchord {
    public partial class MidiPlayer {
        public static IMidiPlayer BrowserMidiPlayer { get; set; }

        public void Init(object obj) {
            // handled by webview helper in after framework init
        }

        public void PlayChord(IEnumerable<Note> notes) {
            if(BrowserMidiPlayer == null) {
                return;
            }

            BrowserMidiPlayer.PlayChord(GetMidiNotes(notes));
        }

        public void PlayScale(IEnumerable<Note> notes) {
            if(BrowserMidiPlayer == null) {
                return;
            }

            BrowserMidiPlayer.PlayScale(GetMidiNotes(notes));
        }

        public void StopPlayback() {
            if(BrowserMidiPlayer == null) {
                return;
            }

            BrowserMidiPlayer.StopPlayback();
        }
    }
}