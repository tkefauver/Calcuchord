using System.Runtime.InteropServices.JavaScript;

namespace Calcuchord.Browser {
    public static partial class JsInterop {
        [JSImport("globalThis.window.playChord")]
        public static partial void PlayChord(int[] notes);

        [JSImport("globalThis.window.playScale")]
        public static partial void PlayScale(int[] notes);

        [JSImport("globalThis.window.stopPlayback")]
        public static partial void StopPlayback();

        [JSImport("globalThis.console.log")]
        public static partial void ConsoleLog(string text);

        [JSImport("globalThis.window.readPrefs")]
        public static partial string ReadPrefs();


        [JSImport("globalThis.window.writePrefs")]
        public static partial void WritePrefs(string prefsJson);

        [JSImport("globalThis.window.isMobile")]
        public static partial bool IsMobile();

        [JSImport("globalThis.window.isTablet")]
        public static partial bool IsTablet();

    }
}