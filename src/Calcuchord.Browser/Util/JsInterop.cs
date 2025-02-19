using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace Calcuchord.Browser {
    public static partial class JsInterop {

        #region Midi

        [JSImport("globalThis.window.playChord")]
        public static partial void PlayChord(int[] notes);

        [JSImport("globalThis.window.playScale")]
        public static partial void PlayScale(int[] notes);

        #endregion

        #region log

        [JSImport("globalThis.console.log")]
        public static partial void ConsoleLog(string text);

        #endregion

        #region Prefs

        [JSImport("globalThis.window.readPrefsAsync")]
        public static partial Task<string> ReadPrefsAsync();


        [JSImport("globalThis.window.writePrefsAsync")]
        public static partial Task WritePrefsAsync(string prefsJson);

        #endregion

        #region Device

        [JSImport("globalThis.window.isMobile")]
        public static partial bool IsMobile();

        [JSImport("globalThis.window.isTablet")]
        public static partial bool IsTablet();

        #endregion

        #region Uri Nav

        [JSImport("globalThis.window.openLink")]
        public static partial void OpenLink(string url);

        #endregion

    }
}