using System;
using NFluidsynth;
using NFluidSettings = NFluidsynth.Settings;

namespace Calcuchord.Desktop {

    public class MidiPlayer_fluid_desktop : MidiFilePlayerBase {

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

        protected override void PlayFile(string soundFontPath) {
            Player.Play();
            Player.Join();
        }


    }

}