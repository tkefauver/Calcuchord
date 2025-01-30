using System;
using System.Diagnostics;
using System.IO;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public static class AssetMover {
        public static void MoveAllAssets() {
            if(PlatformWrapper.StorageHelper is not { } sh ||
               sh.StorageDir is not { } storage_dir) {
                return;
            }

            if(OperatingSystem.IsLinux()) {
                string sound_dir = Path.Combine(storage_dir,"sound");
                if(!sound_dir.IsDirectory()) {
                    MpFileIo.CreateDirectory(sound_dir);
                }

                string[] sound_resources =
                [
                    "avares://Calcuchord/Assets/Sounds/guitar.sf2",
                    "avares://Calcuchord/Assets/Sounds/piano.sf2"
                ];
                foreach(string sr in sound_resources) {
                    byte[] bytes = MpAvFileIo.ReadBytesFromResource("avares://Calcuchord/Assets/Sounds/guitar.sf2");
                    string output_path = Path.Combine(sound_dir,Path.GetFileName(sr.ToPathFromUri()));
                    File.WriteAllBytes(output_path,bytes);
                }

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