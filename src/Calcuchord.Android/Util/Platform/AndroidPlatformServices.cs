using Android.Content;

namespace Calcuchord.Android {
    public class AndroidPlatformServices : PlatformServies_default {

        public AndroidPlatformServices(Context context) {
            PrefsIo = new PrefsIo_ad(context);
        }

        public override IPrefsIo PrefsIo { get; }

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