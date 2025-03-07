namespace Calcuchord {
    public abstract class PlatformServies_default : IPlatformServices {
        Share_default ShareDefault { get; } = new Share_default();

        public virtual ISharePdf SharePdf => ShareDefault;
        public virtual IShareMidi ShareMidi => ShareDefault;
        public virtual IShareHtml ShareHtml => ShareDefault;
        public virtual ILog Logger { get; } = new Logger_default();
        public virtual IPlatformInfo PlatformInfo { get; } = new PlatformInfo_default();
        public virtual IUriNavigator UriNavigator { get; } = new UriNavigator_default();
        public virtual IStorageHelper StorageHelper { get; } = new StorageHelper_default();
        public virtual IPrefsIo PrefsIo { get; } = new PrefsIo_default();
        public abstract IMidiPlayer MidiPlayer { get; }
    }

}