using System;
using System.IO;

namespace Calcuchord {
    public class StorageHelper_default : IStorageHelper {

        public string ShareDir {
            get {
                string root_dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calcuchord");
                if(!Directory.Exists(root_dir)) {
                    Directory.CreateDirectory(root_dir);
                }

                string _shareDir = Path.Combine(root_dir,"Share");
                if(!Directory.Exists(_shareDir)) {
                    Directory.CreateDirectory(_shareDir);
                }

                return _shareDir;
            }
        }

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
                    if(!_storageDir.ToLower().Contains(dir_name)) {
                        _storageDir = Path.Combine(_storageDir,dir_name);
                    }
                }

                return _storageDir;
            }
        }

        public virtual bool IsExternalWriteEnabled() {
            return true;
        }

        public event EventHandler ExternalWriteEnabled;

        public virtual void RequestExternalWritePermission() {
            // only needed on android
        }

        protected void TriggerSaveEnabled() {
            ExternalWriteEnabled?.Invoke(this,EventArgs.Empty);
        }
    }
}