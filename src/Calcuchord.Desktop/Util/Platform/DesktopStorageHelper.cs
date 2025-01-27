using System;
using System.IO;
using MonkeyPaste.Common;

namespace Calcuchord.Desktop {
    public class DesktopStorageHelper : IStorageHelper {
        string _storageDir;

        public string StorageDir {
            get {
                if(_storageDir == null) {
                    string dir_name = "Calcuchord";
                    #if DEBUG
                    dir_name += "_DEBUG";
                    #endif
                    _storageDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if(!_storageDir.ToLower().Contains(dir_name)) {
                        _storageDir = Path.Combine(_storageDir,dir_name);
                    }

                    if(!Directory.Exists(_storageDir)) {
                        try {
                            Directory.CreateDirectory(_storageDir);
                        } catch(Exception e) {
                            e.Dump();
                        }
                    }
                }

                return _storageDir;
            }
        }
    }
}