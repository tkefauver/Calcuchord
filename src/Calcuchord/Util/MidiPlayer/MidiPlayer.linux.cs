using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Calcuchord {

    public partial class MidiPlayer {


        public void Init(object obj) {
            // TODO confirm fluidsynth and 
        }

        public void PlayChord(IEnumerable<Note> notes) {
            SetStopDt(notes.Count(),false);
            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);
            // trackChunk.Events.Add(new ProgramChangeEvent((SevenBitNumber)25)
            // {
            //     DeltaTime = 0
            // });
            int delta = 0;
            var tones = notes.Select(x => x.MidiTone);
            foreach(int tone in tones) {
                int vel = 127;
                trackChunk.Events.Add(new NoteOnEvent((SevenBitNumber)tone,(SevenBitNumber)vel)
                {
                    DeltaTime = delta
                });
                delta += 5;
            }

            foreach(int tone in tones) {
                trackChunk.Events.Add(new NoteOffEvent((SevenBitNumber)tone,(SevenBitNumber)0)
                {
                    DeltaTime = 200
                });
            }

            PlayFile(midiFile,GetInstrumentSoundFontPath(notes.FirstOrDefault()));
        }

        public void PlayScale(IEnumerable<Note> notes) {
            SetStopDt(notes.Count(),true);

            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);

            trackChunk.Events.Add(new ProgramChangeEvent((SevenBitNumber)25)
            {
                DeltaTime = 0
            });

            int delay = 25;
            int deltaTime = 0;
            foreach(int note in notes.Select(x => x.MidiTone)) {
                int vel = 127;

                trackChunk.Events.Add(new NoteOnEvent((SevenBitNumber)note,(SevenBitNumber)vel)
                {
                    DeltaTime = deltaTime
                });
                trackChunk.Events.Add(new NoteOffEvent((SevenBitNumber)note,(SevenBitNumber)0)
                {
                    DeltaTime = delay
                });
                deltaTime = 0;
            }

            PlayFile(midiFile,GetInstrumentSoundFontPath(notes.FirstOrDefault()));

        }

        public void StopPlayback() {
            if(NextStopDt != null) {
                NextStopDt = null;
                Stopped?.Invoke(this,EventArgs.Empty);
            }
        }

        void PlayFile(MidiFile midiFile,string soundFontPath) {
            string midiPath = "output.mid";
            if(File.Exists(midiPath)) {
                File.Delete(midiPath);
            }

            midiFile.Write(midiPath);

            if(File.Exists(soundFontPath)) {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "fluidsynth",
                    Arguments = $"-a alsa -g 1.0 {soundFontPath} {midiPath}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process p = Process.Start(psi);
            }
        }

        string GetInstrumentSoundFontPath(Note note) {
            if(PlatformWrapper.StorageHelper is not { } sh) {
                return string.Empty;
            }

            string sounds_dir = Path.Combine(sh.StorageDir,"sound");
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
    }

}