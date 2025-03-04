using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AudioToolbox;
using AVFoundation;
using Foundation;
using MonkeyPaste.Common;

// using MonoMac.AudioToolbox;
// using MonoMac.AVFoundation;
// using MonoMac.Foundation;

namespace Calcuchord.Mac {

    public class MidiPlayer_mac : MidiSoundFontPlayerBase {        

        void PlayNotes(IEnumerable<IEnumerable<int>> note_sets,double delayMs,double durSeconds) {
            try {
                MusicSequence sequence = new MusicSequence();
                MusicTrack track = sequence.CreateTrack();
                double cur_delay = 300d / 1000d;
                foreach (var note_set in note_sets)
                {
                    foreach(int note in note_set) {
                        track.AddMidiNoteEvent(
                            cur_delay,
                            new MidiNoteMessage(
                                channel: 0,
                                note: (byte)note,
                                velocity: 127,
                                releaseVelocity: 0,
                                duration: (float)durSeconds));
                    }
                    cur_delay += delayMs / 1000d;
                }
                

                string filePath = Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"output.mid");

                if(File.Exists(filePath)) {
                    File.Delete(filePath);
                }

                // Save the sequence as a MIDI file
                NSUrl fileUrl = NSUrl.FromFilename(filePath);
                sequence.CreateFile(fileUrl,MusicSequenceFileTypeID.Midi);
                PlayMidiFileAsync(filePath).FireAndForgetSafeAsync();
            } catch(Exception ex) {
                ex.Dump();
            }

        }
        async Task PlayMidiFileAsync(string midiFilePath) {
            // Load the MIDI file into the MusicSequence
            NSUrl midiFileUrl = NSUrl.FromFilename(midiFilePath);
            NSUrl sf2Url = NSUrl.FromFilename(GetInstrumentSoundFontPath(null));
            var mp = new AVMidiPlayer(midiFileUrl,sf2Url,out NSError error);
            mp.PrepareToPlay();
            await mp.PlayAsync();
        }

        public override bool CanPlay => true;

        public override void Init(object obj) {
            
        }

        public override void PlayChord(IEnumerable<IEnumerable<int>> notes) {
            PlayNotes(notes,30,4);
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> notes) {
            PlayNotes(notes,300,1);
        }
    }
}