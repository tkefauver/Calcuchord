using System;
using System.Diagnostics;
using System.IO;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public static class AssetMover {
        public static void MoveAllAssets() {
            try {
                if(PlatformWrapper.Services is not { } ps ||
                   ps.StorageHelper is not { } sh ||
                   sh.StorageDir is not { } storage_dir) {
                    return;
                }

                if(OperatingSystem.IsLinux()) {
                    string sound_dir = Path.Combine(storage_dir,"sound");
                    if(!Directory.Exists(sound_dir)) {
                        Directory.CreateDirectory(sound_dir);
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


                if(ps.MidiPlayer is MidiPlayer_sugarwv wvh &&
                   wvh.PlayerUrl is { } tone_target_url &&
                   tone_target_url.ToPathFromUri() is { } tone_target_path &&
                   !tone_target_path.IsFile()) {
                    string tone_text = MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/tone.html");
                    File.WriteAllText(tone_target_path,tone_text);
                    PlatformWrapper.Services.Logger.WriteLine($"tone.html was written to: {tone_target_path}");
                }
            } catch(Exception e) {
                e.Dump();
            }

        }
    }
}