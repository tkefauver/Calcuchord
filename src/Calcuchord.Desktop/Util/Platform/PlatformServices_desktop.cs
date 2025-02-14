using System;

namespace Calcuchord.Desktop {
    public class PrefsIo_desktop : IPrefsIo {

        public string ReadPrefs() {
            return null;
        }

        public void WritePrefs(string prefsJson) {
        }
    }

    public class PlatformServices_desktop : PlatformServies_default {
        IMidiPlayer _midiPlayer;

        public override IMidiPlayer MidiPlayer {
            get {
                if(_midiPlayer == null) {
                    if(OperatingSystem.IsLinux()) {
                        _midiPlayer = new MidiPlayer_linux();
                    } else {
                        _midiPlayer = new MidiPlayer_sugarwv();
                    }
                }

                return _midiPlayer;
            }
        }
    }
}