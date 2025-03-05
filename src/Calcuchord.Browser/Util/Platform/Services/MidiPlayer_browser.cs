using System.Collections.Generic;

namespace Calcuchord.Browser {

    public class MidiPlayer_browser : MidiWebPlayerBase {
        public override void Init(object obj) {
            JsInterop.InitMidi();
        }

        public override void PlayChord(IEnumerable<IEnumerable<int>> notes) {
            JsInterop.PlayChord(GetParam(notes));
        }

        public override void PlayScale(IEnumerable<IEnumerable<int>> notes) {
            JsInterop.PlayScale(GetParam(notes));
        }
    }

}