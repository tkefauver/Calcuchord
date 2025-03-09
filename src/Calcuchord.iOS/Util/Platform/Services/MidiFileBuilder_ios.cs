using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AudioToolbox;
using Foundation;

namespace Calcuchord.iOS;

public class MidiFileBuilder_ios : MidiFileBuilder_default {
        public override async Task CreateMidiChordAsync(IEnumerable<IEnumerable<int>> toneSets, string fp) {
                await Task.Delay(1);
                CreateMidiFile(toneSets,30,1,fp);
        }

        public override async Task CreateMidiScaleAsync(IEnumerable<IEnumerable<int>> toneSets, string fp) {
                await Task.Delay(1);
                CreateMidiFile(toneSets,300,1,fp);
        }

        void CreateMidiFile(IEnumerable<IEnumerable<int>> note_sets, double delayMs, double durSeconds, string filePath) {
                try {
                        MusicSequence sequence = new MusicSequence();
                        MusicTrack track = sequence.CreateTrack();
                        double cur_delay = 300d / 1000d;
                        foreach (var note_set in note_sets)
                        {
                                foreach(int note in note_set) {
                                        track?.AddMidiNoteEvent(
                                                cur_delay,
                                                new MidiNoteMessage(
                                                        channel: 0,
                                                        note: (byte)note,
                                                        velocity: 127,
                                                        releaseVelocity: 1,
                                                        duration: (float)durSeconds));
                                }
                                cur_delay += delayMs / 1000d;
                        }
                        if(File.Exists(filePath)) {
                                File.Delete(filePath);
                        }

                        // Save the sequence as a MIDI file
                        NSUrl fileUrl = NSUrl.FromFilename(filePath);
                        sequence.CreateFile(fileUrl,MusicSequenceFileTypeID.Midi);
                } catch(Exception ex) {
                        ex.Dump();
                }
        }
}