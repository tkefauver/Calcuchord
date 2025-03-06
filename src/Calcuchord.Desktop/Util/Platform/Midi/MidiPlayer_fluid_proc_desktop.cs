using System.Diagnostics;
using System.IO;

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
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using Process p = Process.Start(psi);

        }
    }

}