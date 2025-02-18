using System.Threading.Tasks;

namespace Calcuchord.Browser {
    public class PrefsIo_browser : IPrefsIo {

        public async Task<string> ReadPrefsAsync() {
            string result = await JsInterop.ReadPrefsAsync();
            return result;
        }

        public async Task WritePrefsAsync(string prefsJson) {
            await JsInterop.WritePrefsAsync(prefsJson);
        }
    }

}