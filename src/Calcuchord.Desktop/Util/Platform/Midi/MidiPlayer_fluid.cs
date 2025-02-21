using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MonkeyPaste.Common;
using NFluidsynth;
using NFluidSettings = NFluidsynth.Settings;

namespace Calcuchord.Desktop {

    public class MidiPlayer_fluid : MidiPlayerBase {

        string MidiFilePath =>
            Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,"output.mid");

        Player Player { get; set; }
        NFluidSettings Settings { get; set; }
        Synth Synth { get; set; }
        AudioDriver AudioDriver { get; set; }

        public override void Init(object obj) {
            base.Init(obj);
            try {
                Settings = new NFluidSettings();

                // Change this if you don't have pulseaudio or want to change to anything else.
                if(OperatingSystem.IsLinux()) {
                    Settings[ConfigurationKeys.AudioDriver].StringValue = "pulseaudio";
                }

                Settings[ConfigurationKeys.SynthAudioChannels].IntValue = 2;
                Synth = new Synth(Settings);

                Synth.LoadSoundFont(GetInstrumentSoundFontPath(null),true);
                for(int i = 0; i < 16; i++) {
                    Synth.SoundFontSelect(i,0);
                }

                AudioDriver = new AudioDriver(Synth.Settings,Synth);

                Player = new Player(Synth);
                Player.Add(MidiFilePath);
            } catch(Exception ex) {
                // TODO should notify user to install fluidsynth on play click prolly (or figure out how to bundle it?)
                ex.Dump();
                CanPlay = false;
            }

        }

        public override void PlayChord(IEnumerable<Note> notes) {
            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);
            int delta = 0;

            int[] tones = GetMidiNotes(notes);

            foreach(int tone in tones) {
                int vel = 127;
                trackChunk.Events.Add(
                    new NoteOnEvent((SevenBitNumber)tone,(SevenBitNumber)vel)
                    {
                        DeltaTime = delta
                    });
                delta += 5;
            }

            foreach(int tone in tones) {
                trackChunk.Events.Add(
                    new NoteOffEvent((SevenBitNumber)tone,(SevenBitNumber)0)
                    {
                        DeltaTime = 200
                    });
            }

            PlayFile(midiFile,GetInstrumentSoundFontPath(notes.FirstOrDefault()));
        }

        public override void PlayScale(IEnumerable<Note> notes) {
            MidiFile midiFile = new MidiFile();
            TrackChunk trackChunk = new TrackChunk();
            midiFile.Chunks.Add(trackChunk);

            int delay = 25;
            int deltaTime = 0;

            foreach(int note in notes.Select(x => x.MidiTone)) {
                int vel = 127;

                trackChunk.Events.Add(
                    new NoteOnEvent((SevenBitNumber)note,(SevenBitNumber)vel)
                    {
                        DeltaTime = deltaTime
                    });
                trackChunk.Events.Add(
                    new NoteOffEvent((SevenBitNumber)note,(SevenBitNumber)0)
                    {
                        DeltaTime = delay
                    });
                deltaTime = 0;
            }

            PlayFile(midiFile,GetInstrumentSoundFontPath(notes.FirstOrDefault()));

        }


        void PlayFile(MidiFile midiFile,string soundFontPath) {
            Task.Run(
                () => {
                    if(File.Exists(MidiFilePath)) {
                        File.Delete(MidiFilePath);
                    }

                    midiFile.Write(MidiFilePath);

                    Player.Play();
                    Player.Join();
                });
        }

        string GetInstrumentSoundFontPath(Note note) {
            if(PlatformWrapper.Services is not { } sv ||
               sv.StorageHelper is not { } sh) {
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