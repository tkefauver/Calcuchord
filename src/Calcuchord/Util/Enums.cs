using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Calcuchord {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchSortType {
        Key,
        Position,
        Suffix
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaletteColorType {
        None = 0,
        HttTransparent,
        Bg,
        Fg,
        RootFretBg,
        RootFretFg,
        UserFretBg,
        UserFretFg,
        UnknownFretBg,
        UnknownFretFg,
        MutedFretBg,
        MutedFretFg,
        Finger1Fg,
        Finger1Bg,
        Finger2Fg,
        Finger2Bg,
        Finger3Fg,
        Finger3Bg,
        Finger4Fg,
        Finger4Bg,
        NutBg,
        NutFg,
        PianoWhiteKeyBg,
        PianoWhiteKeyBg2,
        PianoWhiteKeyBg3,
        PianoWhiteKeyFg,
        PianoBlackKeyBg,
        PianoBlackKeyBg2,
        PianoBlackKeyBg3,
        PianoBlackKeyFg,
        PianoMatch,
        DisabledAccentFg
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NoteMarkerState {
        Off,
        On,
        Root
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchUpdateSource {
        FindClick,
        NoteToggle,
        RootToggle,
        FilterToggle,
        InstrumentInit,
        BookmarkToggle,
        SortToggle
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum SvgOptionType {
        Fingers,
        Notes,
        Roots,
        Matches,
        Colors,
        Frets,
        Tuning,
        Barres,
        Shadows
    }

    [JsonConverter(typeof(StringEnumConverter))]
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

    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstrumentType {
        Guitar,
        Ukulele,
        Piano,
        Bass,
        Mandolin,
        Banjo,
        Violin,
        Viola,
        Cello,
        Lute,
        Balalaika,
        Other
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MusicPatternType {
        Chords,
        Scales,
        Modes
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DisplayModeType {
        Search,
        Bookmarks,
        Index
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OptionType {
        Pattern,
        DisplayMode,
        Key,

        ChordSuffix,
        ScaleSuffix,
        ModeSuffix,
        ChordSvg,
        ScaleSvg,
        ModeSvg,
        ChordSort,
        ScaleSort,
        ModeSort
    }

    // _ = '/'
    // Num = ''
    // sharp = 'sharp'
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ScaleSuffixType {
        Major,
        NaturalMinor,
        HarmonicMinor,
        MelodicMinor,
        MinorPentatonic,
        Pentatonic,
        Blues
    }

    [JsonConverter(typeof(StringEnumConverter))]
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

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChordSuffixType {
        major,
        maj7,
        maj7b5,
        maj7sharp5,
        maj7sus2,
        maj9,
        maj11,
        maj13,
        mmaj7,
        mmaj7b5,
        mmaj9,
        mmaj11,
        minor,
        m6,
        m69,
        m7,
        m7b5,
        m9,
        m11,
        dim,
        dim7,
        sus,
        sus2,
        sus4,
        sus2sus4,
        aug,
        aug7,
        aug9,
        alt,
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
        _D,
        _Dsharp,
        _Eb,
        _Ab,
        m_B,
        m_C,
        m_Csharp,
        m_D,
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
        m9b5,
        m_A,
        m_Bb,
        m_Eb,
        m_Ab,
        b13b9,
        b13sharp9,
        Num7sus4,
        Num7_G,
        Num5,
        Num6,
        Num69,
        Num7,
        Num7b5,
        Num9,
        Num9b5,
        Num7b9,
        Num7sharp9,
        Num11,
        Num9sharp11,
        Num13,
        Num7b9sharp5,
        Num13b9,
        Num13b5b9
    }
}