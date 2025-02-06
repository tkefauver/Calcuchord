namespace Calcuchord.Android {
    public class AndroidPlatformServices : DefaultPlatformServies {

        IMidiPlayer _midiPlayer;

        public override IMidiPlayer MidiPlayer {
            get {
                if(_midiPlayer == null) {
                    _midiPlayer = new MidiPlayer_ad_sugarwv();
                }

                return _midiPlayer;
            }
        }
    }
}