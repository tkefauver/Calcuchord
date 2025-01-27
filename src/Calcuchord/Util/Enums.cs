using System;

namespace Calcuchord {
    public enum NoteMarkerState {
        Off,
        On,
        Root
    }

    public enum MatchUpdateSource {
        FindClick,
        NoteToggle,
        RootToggle,
        FilterToggle,
        SvgToggle
    }


    [Flags]
    public enum SvgFlags {
        None = 0,
        Fingers = 1,
        Notes = 2,
        Roots = 4,
        Matches = 8,
        Colors = 16,
        Frets = 32,
        Tuning = 64,
        Bars = 128
    }

    public enum NoteType {
        C,
        Db,
        D,
        Eb,
        E,
        F,
        Gb,
        G,
        Ab,
        A,
        Bb,
        B
    }

    public enum InstrumentType {
        Guitar,
        Ukulele,
        Piano,
        Custom
    }

    public enum MusicPatternType {
        Chords,
        Scales,
        Modes
    }

    public enum DisplayModeType {
        Search,
        Bookmarks,
        Index
    }

    public enum OptionType {
        Pattern,
        DisplayMode,
        Key,
        Svg,
        ChordSuffix,
        ScaleSuffix,
        ModeSuffix
    }

    // _ = '/'
    // Num = ''
    // sharp = 'sharp'
    public enum ScaleSuffixType {
        Major,
        NaturalMinor,
        HarmonicMinor,
        MelodicMinor,
        MinorPentatonic,
        Pentatonic,
        Blues
    }

    public enum ModeSuffixType {
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        Locrian,

        AhavaRaba
        // missing:
        // Ionian,
        // Aeolian
    }

    public enum ChordSuffixType {
        major,
        minor,
        dim,
        dim7,
        sus,
        sus2,
        sus4,
        sus2sus4,
        Num7sus4,
        Num7_G,
        alt,
        aug,
        Num5,
        Num6,
        Num69,
        Num7,
        Num7b5,
        aug7,
        Num9,
        Num9b5,
        aug9,
        Num7b9,
        Num7sharp9,
        Num11,
        Num9sharp11,
        Num13,
        maj7,
        maj7b5,
        maj7sharp5,
        maj7sus2,
        maj9,
        maj11,
        maj13,
        m6,
        m69,
        m7,
        m7b5,
        m9,
        m11,
        mmaj7,
        mmaj7b5,
        mmaj9,
        mmaj11,
        add9,
        madd9,
        add11,
        _E,
        _F,
        _Fsharp,
        _G,
        _Gsharp,
        _A,
        _Bb,
        _B,
        _C,
        _Csharp,
        m_B,
        m_C,
        m_Csharp,
        _D,
        m_D,
        _Dsharp,
        m_Dsharp,
        m_E,
        m_F,
        m_Fsharp,
        m_G,
        m_Gsharp,
        m9_Bb,
        m9_B,
        m9_C,
        m9_Csharp,
        m9_D,
        m9_Eb,
        m9_E,
        m9_F,
        m9_Fsharp,
        m9_G,
        m9_Ab,
        m9_A,
        _Eb,
        _Ab,
        m_A,
        m_Bb,
        m_Eb,
        m_Ab,
        Num7b9sharp5,
        Num13b9,
        Num13b5b9,
        b13b9,
        b13sharp9,
        m9b5
    }
}