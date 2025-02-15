namespace Calcuchord.Browser {
    public class PrefsIo_browser : IPrefsIo {

        public string ReadPrefs() {
            return JsInterop.ReadPrefs();
        }

        public void WritePrefs(string prefsJson) {
            JsInterop.WritePrefs(prefsJson);
        }
    }

}