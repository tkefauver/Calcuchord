using System;

namespace Calcuchord.Desktop {
    public class DesktopPlatformServices : DefaultPlatformServies {
        IMidiPlayer _midiPlayer;

        public override IMidiPlayer MidiPlayer {
            get {
                if(_midiPlayer == null) {
                    if(OperatingSystem.IsLinux()) {
                        _midiPlayer = new LinuxMidiPlayer();
                    } else {
                        _midiPlayer = new MidiPlayer_sugarwv();
                    }
                }

                return _midiPlayer;
            }
        }
    }
}