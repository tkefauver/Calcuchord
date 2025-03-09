using Android.Content;

namespace Calcuchord.Android {
    public class PlatformServices_ad : PlatformServies_default {
        Share_ad ShareObj { get; }

        public PlatformServices_ad(Context context) {
            PlatformInfo = new PlatformInfo_ad(context);
            UriNavigator = new UriNav_ad(context);
            ShareObj = new Share_ad(context);
        }

        public override IShareMidi ShareMidi => ShareObj;
        public override ISharePdf SharePdf => ShareObj;
        public override IShareHtml ShareHtml => ShareObj;
        public override IPlatformInfo PlatformInfo { get; }
        public override IUriNavigator UriNavigator { get; }
        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_fluid_ad();
    }
}