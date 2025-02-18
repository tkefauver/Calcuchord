using Android.Content;

namespace Calcuchord.Android {
    public class PlatformServices_ad : PlatformServies_default {

        public PlatformServices_ad(Context context) {
            PrefsIo = new PrefsIo_ad(context);
            PlatformInfo = new PlatformInfo_ad(context);
        }

        public override IPrefsIo PrefsIo { get; }
        public override IPlatformInfo PlatformInfo { get; }

        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_ad_sugarwv();
        public override IUriNavigator UriNavigator { get; } = new UriNav_ad();
    }
}