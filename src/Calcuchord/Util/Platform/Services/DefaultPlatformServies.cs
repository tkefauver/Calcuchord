namespace Calcuchord {

    public abstract class DefaultPlatformServies : IPlatformServices {

        IStorageHelper _storageHelper;

        public virtual IStorageHelper StorageHelper {
            get {
                if(_storageHelper == null) {
                    _storageHelper = new DefaultStorageHelper();
                }

                return _storageHelper;
            }
        }


        public abstract IMidiPlayer MidiPlayer { get; }
        public virtual IPrefsIo PrefsIo { get; }
    }
}