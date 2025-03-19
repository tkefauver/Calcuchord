using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;

namespace Calcuchord.Desktop {
    public class MidiPlayer_win_desktop : MidiFluidPlayerBase {
        
        protected override void PlayFile(string soundFontPath) {
            Task.Run(async() =>
            {
                var access = MidiAccessManager.Default;
                var output = await access.OpenOutputAsync(access.Outputs.Last().Id);
                var ms = new MemoryStream(File.ReadAllBytes(MidiFilePath));
                var music = MidiMusic.Read(ms);
                var player = new MidiPlayer(music, output);
                player.PlaybackCompletedToEnd += () => {
                    player.Dispose();
                    ms.Dispose();
                    output.Dispose();
                };
                player.Play();
            });
        }
    }

}