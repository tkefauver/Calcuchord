namespace Calcuchord.Browser {
    public class PlatformServices_browser : PlatformServies_default {
        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_browser();
        public override IUriNavigator UriNavigator { get; } = new UriNavigator_browser();
        public override IPrefsIo PrefsIo { get; } = new PrefsIo_browser();
        public override IPlatformInfo PlatformInfo { get; } = new PlatformInfo_browser();
        public override ILog Logger { get; } = new Logger_browser();
    }
}