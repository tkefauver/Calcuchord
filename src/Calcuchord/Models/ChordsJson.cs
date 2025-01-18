// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

using System.Collections.Generic;

namespace Calcuchord;

public class Root {
    public Main main{ get; set; }
    public Tunings tunings{ get; set; }
    public List<string> keys{ get; set; }
    public List<string> suffixes{ get; set; }
    public Chords chords{ get; set; }
}

public class Main {
    public int strings{ get; set; }
    public int fretsOnChord{ get; set; }
    public string name{ get; set; }
    public int numberOfChords{ get; set; }
}

public class Tunings {
    public List<string> standard{ get; set; }
}

public class Position {
    public List<int> frets{ get; set; }
    public List<int> fingers{ get; set; }
    public int baseFret{ get; set; }
    public List<int> barres{ get; set; }
    public List<int> midi{ get; set; }
    public bool? capo{ get; set; }
}

public class Key {
    public string key{ get; set; }
    public string suffix{ get; set; }
    public List<Position> positions{ get; set; }
}

public class A : Key { }

public class Ab : Key { }

public class B : Key { }

public class Bb : Key { }

public class C : Key { }

public class Csharp : Key { }

public class D : Key { }

public class Eb : Key { }

public class E : Key { }

public class F : Key { }

public class Fsharp : Key { }

public class G : Key { }

public class Chords {
    public List<C> C{ get; set; }
    public List<Csharp> Csharp{ get; set; }
    public List<D> D{ get; set; }
    public List<Eb> Eb{ get; set; }
    public List<E> E{ get; set; }
    public List<F> F{ get; set; }
    public List<Fsharp> Fsharp{ get; set; }
    public List<G> G{ get; set; }
    public List<Ab> Ab{ get; set; }
    public List<A> A{ get; set; }
    public List<Bb> Bb{ get; set; }
    public List<B> B{ get; set; }
}