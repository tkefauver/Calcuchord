using System;
using System.IO;
using System.Threading.Tasks;

namespace Calcuchord {

    public class PrefsIo_default : IPrefsIo {


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
                await File.WriteAllTextAsync(PrefsFilePath,prefsJson);
            } catch(Exception e) {
                e.Dump();
            }
        }
    }
}