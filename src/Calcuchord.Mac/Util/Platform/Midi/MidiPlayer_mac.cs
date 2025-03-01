using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MonkeyPaste.Common;

// using MonoMac.AudioToolbox;
// using MonoMac.AVFoundation;
// using MonoMac.Foundation;

namespace Calcuchord.Mac {

    public class MidiPlayer_mac : MidiFilePlayerBase {
        public override void Init(object obj) {
            base.Init(obj);

        }

        void PlayNotes(IEnumerable<int> notes,double delayMs,double durSeconds) {
            try {
                MusicSequence sequence = new MusicSequence();
                MusicTrack track = sequence.CreateTrack();
                double cur_delay = 300d / 1000d;
                foreach(int note in notes) {
                    track.AddMidiNoteEvent(
                        cur_delay,
                        new MidiNoteMessage(
                            channel: 0,
                            note: (byte)note,
                            velocity: 127,
                            releaseVelocity: 0,
                            duration: (float)durSeconds));
                    cur_delay += delayMs / 1000d;
                }

                string filePath = MidiFilePath;

                if(File.Exists(filePath)) {
                    // yo
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
            mp.Init();
            mp.PrepareToPlay();
            await mp.PlayAsync();
        }

        public override bool CanPlay => true;

        public override void PlayChord(IEnumerable<Note> notes) {
            PlayNotes(GetMidiNotes(notes),30,4);
        }

        public override void PlayScale(IEnumerable<Note> notes) {
            PlayNotes(GetMidiNotes(notes),300,1);
        }
    }
}