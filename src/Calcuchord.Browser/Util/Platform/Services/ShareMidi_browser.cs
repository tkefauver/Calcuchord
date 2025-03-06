using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace Calcuchord.Browser {
    public class ShareMidi_browser : IShareMidi {
        MidiFileBuilder Builder { get; } = new MidiFileBuilder();

        public async Task ShareMidiAsync(IEnumerable<IEnumerable<int>> toneSets,bool isScale,string title) {
            string fileName = title + ".mid";
            if(isScale) {
                await Builder.CreateMidiScaleAsync(toneSets,fileName);
            } else {
                await Builder.CreateMidiChordAsync(toneSets,fileName);
            }

            string b64 = File.ReadAllBytes(fileName).ToBase64String();
            JsInterop.ShareFile(b64,"audio/midi",fileName,title);
        }
    }
}