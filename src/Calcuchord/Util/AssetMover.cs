using System;
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

                if(!Directory.Exists(storage_dir)) {
                    Directory.CreateDirectory(storage_dir);
                }

                if(MidiPlayerBase.IS_NFLUID_PLAYER &&
                   Path.Combine(storage_dir,"sound") is { } sound_dir &&
                   !sound_dir.IsDirectory()) {
                    // NOTE below only happens if sound/ doesn't exist
                    Directory.CreateDirectory(sound_dir);

                    byte[] guitar_bytes =
                        MpAvFileIo.ReadBytesFromResource("avares://Calcuchord/Assets/Sounds/guitar.sf2");
                    string guitar_path = Path.Combine(sound_dir,"guitar.sf2");
                    File.WriteAllBytes(guitar_path,guitar_bytes);

                    byte[] piano_bytes =
                        MpAvFileIo.ReadBytesFromResource("avares://Calcuchord/Assets/Sounds/piano.sf2");
                    string piano_path = Path.Combine(sound_dir,"piano.sf2");
                    File.WriteAllBytes(piano_path,piano_bytes);
                }


                if(ps.MidiPlayer is MidiPlayer_sugarwv wvh &&
                   wvh.PlayerUrl is { } tone_target_url &&
                   tone_target_url.ToPathFromUri() is { } tone_target_path &&
                   !tone_target_path.IsFile()) {
                    // NOTE below only happens if tone.html doesn't exist

                    File.WriteAllText(
                        tone_target_path,
                        MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/tone.html"));

                    string js_dir = Path.Combine(storage_dir,"js");
                    Directory.CreateDirectory(js_dir);

                    string midi_dir = Path.Combine(js_dir,"midi");
                    Directory.CreateDirectory(midi_dir);

                    File.WriteAllText(
                        Path.Combine(midi_dir,"0240_Aspirin_sf2_file.js"),
                        MpAvFileIo.ReadTextFromResource(
                            "avares://Calcuchord/Assets/Text/js/midi/0240_Aspirin_sf2_file.js"));

                    File.WriteAllText(
                        Path.Combine(midi_dir,"player.js"),
                        MpAvFileIo.ReadTextFromResource("avares://Calcuchord/Assets/Text/js/midi/player.js"));

                    File.WriteAllText(
                        Path.Combine(midi_dir,"WebAudioFontPlayer.js"),
                        MpAvFileIo.ReadTextFromResource(
                            "avares://Calcuchord/Assets/Text/js/midi/WebAudioFontPlayer.js"));


                    PlatformWrapper.Services.Logger.WriteLine($"tone.html was written to: {tone_target_path}");
                }
            } catch(Exception e) {
                e.Dump();
            }

        }
    }
}