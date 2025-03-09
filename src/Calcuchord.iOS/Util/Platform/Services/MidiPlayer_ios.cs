using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AudioToolbox;
using AVFoundation;
using Foundation;
using MonkeyPaste.Common;

namespace Calcuchord.iOS {
    public class MidiPlayer_ios : MidiSoundFontPlayerBase {
        public override void PlayChord(IEnumerable<IEnumerable<int>> notes) {
            if (PlatformWrapper.Services.MidiFileBuilder is not { } mfb)
            {
                return;
            }

            Task.Run(async () =>
            {
                await mfb.CreateMidiChordAsync(notes, MidiFilePath);
                await PlayMidiFileAsync(MidiFilePath);

            });
            
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> notes) {
            if (PlatformWrapper.Services.MidiFileBuilder is not { } mfb)
            {
                return;
            }

            Task.Run(async () =>
            {
                await mfb.CreateMidiScaleAsync(notes, MidiFilePath);
                await PlayMidiFileAsync(MidiFilePath);
            });
        }
        
        async Task PlayMidiFileAsync(string midiFilePath) {
            // Load the MIDI file into the MusicSequence
            NSUrl midiFileUrl = NSUrl.FromFilename(midiFilePath);
            NSUrl sf2Url = NSUrl.FromFilename(GetInstrumentSoundFontPath(null));
            var mp = new AVMidiPlayer(midiFileUrl,sf2Url,out NSError error);
            mp.PrepareToPlay();
            await mp.PlayAsync();
            await Task.Delay(10_000);
        }

    }
}