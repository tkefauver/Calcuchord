using System;

namespace Calcuchord.Desktop {
    public class DesktopPlatformServices : DefaultPlatformServies {
        IMidiPlayer _midiPlayer;

        public override IMidiPlayer MidiPlayer {
            get {
                if(_midiPlayer == null) {
                    if(OperatingSystem.IsLinux()) {
#if LINUX
                        _midiPlayer = new LinuxMidiPlayer();
#endif
                    } else {
                        _midiPlayer = new MidiPlayer_sugarwv();
                    }
                }

                return _midiPlayer;
            }
        }
    }
}