using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Calcuchord {
    public abstract class MidiFilePlayerBase : MidiPlayerBase {
        public string SoundFontDir =>
            Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"sound");

        protected string MidiFilePath =>
            Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"output.mid");

        protected string GetInstrumentSoundFontPath(Note note) {
            string sounds_dir = SoundFontDir;
            string fn = "guitar.sf2";
            if(note is PatternNote pn &&
               pn.Parent is { } ng &&
               ng.Parent is { } ngc &&
               ngc.Parent is { } tuning &&
               tuning.Parent is { } inst) {
                if(inst.InstrumentType == InstrumentType.Piano) {
                    fn = "piano.sf2";
                }
            }

            return Path.Combine(sounds_dir,fn);
        }

        public override void PlayChord(IEnumerable<IEnumerable<int>> tone_sets) {
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
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> tone_sets) {
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
        }

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

        protected abstract void PlayFile(string soundFontPath);
    }
}