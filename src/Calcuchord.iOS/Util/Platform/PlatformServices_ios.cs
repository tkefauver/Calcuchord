namespace Calcuchord.iOS {
    public class PlatformServices_ios : PlatformServies_default {
        Share_ios ShareObj { get; }
        public PlatformServices_ios(AppDelegate appDelegate) {
            ShareObj = new Share_ios(appDelegate);
        }
        public override IShareMidi ShareMidi => ShareObj;
        public override ISharePdf SharePdf => ShareObj;
        public override IShareHtml ShareHtml => ShareObj;
        //public override IMidiFileBuilder MidiFileBuilder { get; } = new MidiFileBuilder_ios();
        public override IPlatformInfo PlatformInfo { get; } = new PlatformInfo_ios();
        public override IMidiPlayer MidiPlayer { get; } = new MidiPlayer_ios();
        public override IUriNavigator UriNavigator { get; } = new UriNav_ios();
    }
}