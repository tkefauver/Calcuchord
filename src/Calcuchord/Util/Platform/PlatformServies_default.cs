namespace Calcuchord {

    public abstract class PlatformServies_default : IPlatformServices {
        readonly PrefsIo_default _prefs = new PrefsIo_default();

        public virtual IStorageHelper StorageHelper => _prefs;

        public virtual IPrefsIo PrefsIo => _prefs;

        public abstract IMidiPlayer MidiPlayer { get; }
    }
}