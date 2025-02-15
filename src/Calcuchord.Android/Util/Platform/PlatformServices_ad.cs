using Android.Content;

namespace Calcuchord.Android {
    public class PlatformServices_ad : PlatformServies_default {

        public PlatformServices_ad(Context context) {
            PrefsIo = new PrefsIo_ad(context);
            PlatformInfo = new PlatformInfo_ad(context);
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

        public override IPlatformInfo PlatformInfo { get; }
    }
}