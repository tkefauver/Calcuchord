namespace Calcuchord.Browser {
    public class PlatformServices_browser : PlatformServies_default {
        Share_browser ShareObj { get; } = new Share_browser();
        public override IShareMidi ShareMidi => ShareObj;
        public override ISharePdf SharePdf => ShareObj;
        public override IShareHtml ShareHtml => ShareObj;
        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_browser();
        public override IUriNavigator UriNavigator { get; } = new UriNavigator_browser();
        public override IPrefsIo PrefsIo { get; } = new PrefsIo_browser();
        public override IPlatformInfo PlatformInfo { get; } = new PlatformInfo_browser();
        public override ILog Logger { get; } = new Logger_browser();
    }
}