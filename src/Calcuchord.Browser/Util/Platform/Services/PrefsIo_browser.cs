namespace Calcuchord.Browser {
    public class PrefsIo_browser : IPrefsIo {

        public string ReadPrefs() {
            return GlobalJsInterop.ReadPrefs();
        }

        public void WritePrefs(string prefsJson) {
            GlobalJsInterop.WritePrefs(prefsJson);
        }
    }
}