// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Calcuchord.JsonChords {
    public class ChordsJsonRoot {
        public Main main { get; set; }
        public Tunings tunings { get; set; }
        public List<string> keys { get; set; }
        public List<string> suffixes { get; set; }
        public Chords chords { get; set; }
    }

    public class Chords {
        public List<MusicKey> C { get; set; }
        public List<MusicKey> Csharp { get; set; }
        public List<MusicKey> D { get; set; }
        public List<MusicKey> Db { get; set; }
        public List<MusicKey> Eb { get; set; }
        public List<MusicKey> E { get; set; }
        public List<MusicKey> F { get; set; }
        public List<MusicKey> Fsharp { get; set; }
        public List<MusicKey> G { get; set; }
        public List<MusicKey> Gb { get; set; }
        public List<MusicKey> Ab { get; set; }
        public List<MusicKey> A { get; set; }
        public List<MusicKey> Bb { get; set; }
        public List<MusicKey> B { get; set; }
    }

    public class Main {
        public int strings { get; set; }
        public int fretsOnChord { get; set; }
        public string name { get; set; }
        public int numberOfChords { get; set; }
    }

    public class Tunings {
        public List<string> standard { get; set; }
    }

    public class Position {
        public List<int> frets { get; set; }
        public List<int> fingers { get; set; }
        public int baseFret { get; set; }
        public List<int> barres { get; set; }
        public List<int> midi { get; set; }
        public bool? capo { get; set; }

        [JsonIgnore]
        public IEnumerable<int> real_frets {
            get {
                foreach(int fret in frets) {
                    if(fret <= 0 || baseFret <= 1) {
                        yield return fret;
                    } else {
                        yield return (baseFret - 1) + fret;
                    }


                }
            }
        }
    }

    public class MusicKey {
        public string key { get; set; }
        public string suffix { get; set; }
        public List<Position> positions { get; set; }
    }

    public class A : MusicKey {
    }

    public class Ab : MusicKey {
    }

    public class B : MusicKey {
    }

    public class Bb : MusicKey {
    }

    public class C : MusicKey {
    }

    public class Csharp : MusicKey {
    }

    public class D : MusicKey {
    }

    public class Db : MusicKey {
    }

    public class Eb : MusicKey {
    }

    public class E : MusicKey {
    }

    public class F : MusicKey {
    }

    public class Fsharp : MusicKey {
    }

    public class G : MusicKey {
    }

    public class Gb : MusicKey {
    }
}