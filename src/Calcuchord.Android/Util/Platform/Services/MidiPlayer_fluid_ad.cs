using Android.Media;

namespace Calcuchord.Android {
    public class MidiPlayer_fluid_ad : MidiFluidPlayerBase {
        protected override void PlayFile(string soundFontPath) {
            MediaPlayer mp = new MediaPlayer();
            mp.SetDataSource(MidiFilePath);
            mp.Prepare();
            mp.Start();
        }

        
    }
}