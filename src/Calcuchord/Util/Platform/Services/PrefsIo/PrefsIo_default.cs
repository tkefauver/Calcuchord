using System;
using System.IO;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace Calcuchord {

    public class PrefsIo_default : IPrefsIo,IStorageHelper {
        string _storageDir;

        public string StorageDir {
            get {
                if(OperatingSystem.IsBrowser()) {
                    return string.Empty;
                }

                if(_storageDir == null) {
                    string dir_name = "Calcuchord";
#if DEBUG
                    dir_name += "_DEBUG";
#endif
                    _storageDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#if DEBUG && LINUX
                    //_storageDir = "/home/tkefauver/Desktop";
#endif
                    if(!_storageDir.ToLower().Contains(dir_name)) {
                        _storageDir = Path.Combine(_storageDir,dir_name);
                    }
                }

                return _storageDir;
            }
        }

        string _prefsFilePath;

        protected string PrefsFilePath {
            get {
                if(_prefsFilePath == null &&
                   PlatformWrapper.Services is { } ps &&
                   ps.StorageHelper is { } sh &&
                   sh.StorageDir is { } sd) {
                    string fn = "appstate.json";
                    _prefsFilePath = Path.Combine(sd,fn);
                }

                return _prefsFilePath;
            }
        }

        public virtual async Task<string> ReadPrefsAsync() {
            if(!File.Exists(PrefsFilePath)) {
                return string.Empty;
            }

            try {
                return await File.ReadAllTextAsync(PrefsFilePath);
            } catch(Exception e) {
                e.Dump();

            }

            return string.Empty;
        }

        public virtual async Task WritePrefsAsync(string prefsJson) {
            try {
                if(PlatformWrapper.Services is not { } ps ||
                   ps.StorageHelper is not { } sh ||
                   sh.StorageDir is not { } storage_dir) {
                    return;
                }

                if(!Directory.Exists(storage_dir)) {
                    Directory.CreateDirectory(storage_dir);

                }

                await File.WriteAllTextAsync(PrefsFilePath,prefsJson);
            } catch(Exception e) {
                e.Dump();
            }
        }
    }
}