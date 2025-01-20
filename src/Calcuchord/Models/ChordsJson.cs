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
        public List<Key> C { get; set; }
        public List<Key> Csharp { get; set; }
        public List<Key> D { get; set; }
        public List<Key> Db { get; set; }
        public List<Key> Eb { get; set; }
        public List<Key> E { get; set; }
        public List<Key> F { get; set; }
        public List<Key> Fsharp { get; set; }
        public List<Key> G { get; set; }
        public List<Key> Gb { get; set; }
        public List<Key> Ab { get; set; }
        public List<Key> A { get; set; }
        public List<Key> Bb { get; set; }
        public List<Key> B { get; set; }
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
                foreach(var fret in frets) {
                    if(fret <= 0 || baseFret <= 1) {
                        yield return fret;
                    }
                    else {
                        yield return (baseFret - 1) + fret;
                    }


                }
            }
        }
    }

    public class Key {
        public string key { get; set; }
        public string suffix { get; set; }
        public List<Position> positions { get; set; }
    }

    public class A : Key {
    }

    public class Ab : Key {
    }

    public class B : Key {
    }

    public class Bb : Key {
    }

    public class C : Key {
    }

    public class Csharp : Key {
    }

    public class D : Key {
    }

    public class Db : Key {
    }

    public class Eb : Key {
    }

    public class E : Key {
    }

    public class F : Key {
    }

    public class Fsharp : Key {
    }

    public class G : Key {
    }

    public class Gb : Key {
    }
}