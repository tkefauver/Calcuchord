namespace Calcuchord.Browser {
    public class PlatformServices_browser : PlatformServies_default {
        IMidiPlayer _midiPlayer;

        public override IMidiPlayer MidiPlayer {
            get {
                if(_midiPlayer == null) {
                    _midiPlayer = new MidiPlayer_browser();
                }

                return _midiPlayer;
            }
        }

        public override IPrefsIo PrefsIo { get; } = new PrefsIo_browser();
        public override IPlatformInfo PlatformInfo { get; } = new PlatformInfo_browser();
    }
}