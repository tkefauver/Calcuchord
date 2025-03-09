using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcuchord {
    public abstract class MidiFluidPlayerBase : MidiSoundFontPlayerBase {

        public override void PlayChord(IEnumerable<IEnumerable<int>> tone_sets) {
            if(PlatformWrapper.Services.MidiFileBuilder is not { } mfb) {
                return;
            }

            Task.Run(
                async () => {
                    await mfb.CreateMidiChordAsync(tone_sets,MidiFilePath);
                    PlayFile(GetInstrumentSoundFontPath(null));

                });
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> tone_sets) {
            if(PlatformWrapper.Services.MidiFileBuilder is not { } mfb) {
                return;
            }

            Task.Run(
                async () => {
                    await mfb.CreateMidiScaleAsync(tone_sets,MidiFilePath);
                    PlayFile(GetInstrumentSoundFontPath(null));

                });
        }

        protected abstract void PlayFile(string soundFontPath);
    }
}