using System.Diagnostics;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public static class AssetMover {
        public static void MoveAllAssets() {
            if(PlatformWrapper.StorageHelper is not { } sh ||
               sh.StorageDir is not { } storage_dir) {
                return;
            }

            if(PlatformWrapper.WebViewHelper is { } wvh &&
               wvh.ToneUrl is { } tone_target_url &&
               tone_target_url.ToPathFromUri() is { } tone_target_path &&
               !tone_target_path.IsFile()) {
                string tone_text = MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/tone.html");
                MpFileIo.WriteTextToFile(tone_target_path,tone_text);
                Debug.WriteLine($"tone.html was written to: {tone_target_path}");
            }
        }
    }
}