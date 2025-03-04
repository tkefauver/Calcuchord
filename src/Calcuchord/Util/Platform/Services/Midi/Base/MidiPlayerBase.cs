using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Calcuchord {

    public abstract class MidiPlayerBase : IMidiPlayer {
        
        public virtual bool CanPlay { get; protected set; } = true;
        public abstract void Init(object obj);
        public abstract void PlayChord(IEnumerable<IEnumerable<int>> tone_sets);
        public abstract void PlayScale(IEnumerable<IEnumerable<int>> tone_sets);
    }
}