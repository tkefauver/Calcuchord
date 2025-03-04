using System.Collections.Generic;
#if !IOS
using System.IO;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
#endif

namespace Calcuchord {
    public abstract class MidiFluidPlayerBase : MidiSoundFontPlayerBase {
        public override void PlayChord(IEnumerable<IEnumerable<int>> tone_sets) {
#if !IOS
            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);
            int delta = 0;
            int vel = 127;


            foreach(var tone_set in tone_sets) {
                foreach(int tone in tone_set) {
                    trackChunk.Events.Add(
                        new NoteOnEvent((SevenBitNumber)tone,(SevenBitNumber)vel)
                        {
                            DeltaTime = delta,
                        });
                }

                delta += 5;
            }

            foreach(var tone_set in tone_sets) {
                foreach(int tone in tone_set) {
                    trackChunk.Events.Add(
                        new NoteOffEvent((SevenBitNumber)tone,(SevenBitNumber)0)
                        {
                            DeltaTime = 200,
                        });
                }
            }

            PreparePlayback(midiFile,GetInstrumentSoundFontPath(null));
#endif
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> tone_sets) {
            #if !IOS
            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);

            int delay = 25;
            int deltaTime = 0;

            foreach(var tone_set in tone_sets) {
                foreach(int note in tone_set) {
                    int vel = 127;

                    trackChunk.Events.Add(
                        new NoteOnEvent((SevenBitNumber)note,(SevenBitNumber)vel)
                        {
                            DeltaTime = deltaTime,
                        });
                    trackChunk.Events.Add(
                        new NoteOffEvent((SevenBitNumber)note,(SevenBitNumber)0)
                        {
                            DeltaTime = delay,
                        });
                    deltaTime = 0;
                }
            }


            PreparePlayback(midiFile,GetInstrumentSoundFontPath(null));
#endif
        }
#if !IOS

        void PreparePlayback(MidiFile midiFile,string soundFontPath) {
            Task.Run(
                () => {
                    if(File.Exists(MidiFilePath)) {
                        File.Delete(MidiFilePath);
                    }

                    midiFile.Write(MidiFilePath);

                    PlayFile(soundFontPath);
                });
        }
#endif

        protected abstract void PlayFile(string soundFontPath);
    }
}