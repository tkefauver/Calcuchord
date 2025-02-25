namespace Calcuchord.iOS {
    public class PlatformServices_ios : PlatformServies_default {
        public override IPlatformInfo PlatformInfo { get; } = new PlatformInfo_ios();
        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_ios();
        public override IUriNavigator UriNavigator { get; } = new UriNav_ios();
    }
}