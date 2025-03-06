using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcuchord {
    public abstract class MidiFluidPlayerBase : MidiSoundFontPlayerBase {
        MidiFileBuilder Builder { get; } = new MidiFileBuilder();

        public override void PlayChord(IEnumerable<IEnumerable<int>> tone_sets) {
            Task.Run(
                async () => {
                    await Builder.CreateMidiChordAsync(tone_sets,MidiFilePath);
                    PlayFile(GetInstrumentSoundFontPath(null));

                });
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> tone_sets) {
            Task.Run(
                async () => {
                    await Builder.CreateMidiScaleAsync(tone_sets,MidiFilePath);
                    PlayFile(GetInstrumentSoundFontPath(null));

                });
        }

        protected abstract void PlayFile(string soundFontPath);
    }
}