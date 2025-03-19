using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace Calcuchord.Desktop {

    public class MidiPlayer_fluid_proc_desktop : MidiFluidPlayerBase {

        protected override void PlayFile(string soundFontPath) {
            if(!File.Exists(soundFontPath)) {
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "fluidsynth",
                Arguments = $"-a alsa -g 1.0 {soundFontPath} {MidiFilePath}",
                UseShellExecute = true,
                CreateNoWindow = true,
            };
            using Process p = Process.Start(psi);

        }
    }

}