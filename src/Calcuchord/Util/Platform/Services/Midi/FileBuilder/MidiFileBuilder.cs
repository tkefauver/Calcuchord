using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
#if !IOS
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
#endif

namespace Calcuchord {
    public class MidiFileBuilder {

        public async Task CreateMidiScaleAsync(IEnumerable<IEnumerable<int>> toneSets,string fp) {
#if IOS
            await Task.Delay(1);
#else
            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);

            int vel = 127;
            int sus = 25;

            foreach(var tone_set in toneSets) {
                foreach(int note in tone_set) {
                    trackChunk.Events.Add(
                        new NoteOnEvent((SevenBitNumber)note,(SevenBitNumber)vel));
                }

                foreach(int note in tone_set) {
                    trackChunk.Events.Add(
                        new NoteOffEvent((SevenBitNumber)note,(SevenBitNumber)0)
                        {
                            DeltaTime = sus,
                        });
                }
            }

            await Task.Run(
                () => {
                    if(File.Exists(fp)) {
                        File.Delete(fp);
                    }

                    midiFile.Write(fp);
                });
#endif

        }

        public async Task CreateMidiChordAsync(IEnumerable<IEnumerable<int>> toneSets,string fp) {
#if IOS
            await Task.Delay(1);
#else
            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);
            int delta = 0;
            int sus = 200;
            int vel = 127;

            foreach(var tone_set in toneSets) {
                foreach(int tone in tone_set) {
                    trackChunk.Events.Add(
                        new NoteOnEvent((SevenBitNumber)tone,(SevenBitNumber)vel)
                        {
                            DeltaTime = delta,
                        });
                }

                delta += 5;
            }

            foreach(var tone_set in toneSets) {
                foreach(int tone in tone_set) {
                    trackChunk.Events.Add(
                        new NoteOffEvent((SevenBitNumber)tone,(SevenBitNumber)0)
                        {
                            DeltaTime = sus,
                        });
                }
            }

            await Task.Run(
                () => {
                    if(File.Exists(fp)) {
                        File.Delete(fp);
                    }

                    midiFile.Write(fp);
                });
#endif
        }
    }
}