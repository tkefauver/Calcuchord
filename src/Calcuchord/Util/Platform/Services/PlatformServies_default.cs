namespace Calcuchord {
    public abstract class PlatformServies_default : IPlatformServices {

        public virtual ILog Logger { get; } = new Logger_default();
        public virtual IStorageHelper StorageHelper { get; } = new PrefsIo_default();
        public virtual IPlatformInfo PlatformInfo { get; } = new PlatformInfo_default();
        public virtual IUriNavigator UriNavigator { get; } = new UriNavigator_default();

        public virtual IPrefsIo PrefsIo { get; } = new PrefsIo_default();

        public abstract IMidiPlayer MidiPlayer { get; }
    }

}