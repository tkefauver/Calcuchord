using System;
using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class PatternGen {
        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Properties

        Dictionary<string,int[]> Chords { get; } = new() {
            { "5 chord",[0,7] },
            { "Major chord",[0,4,3] },
            { "Minor chord",[0,3,4] },
            { "Diminished chord",[0,3,3] },
            { "Augmented chord",[0,4,4] },
            { "Sus2 chord",[0,2,5] },
            { "Sus4 chord",[0,5,2] },
            { "Maj7 chord",[0,4,3,4] },
            { "min7 chord",[0,3,4,3] },
            { "7 chord",[0,4,3,3] },
            { "min7b5 chord",[0,3,3,4] },
            { "dim7 chord",[0,3,3,3] },
            { "9 chord",[0,4,3,3,4] },
            { "Maj9 chord",[0,4,3,4,3] },
            { "m9 chord",[0,3,4,3,4] },
            { "11 chord",[0,4,3,3,4,3] },
            { "Maj11 chord",[0,4,3,4,3,3] },
            { "min11 chord",[0,3,4,3,4,3] }
        };

        Dictionary<string,int[]> Modes { get; } = new() {
            { "Dorian mode",[0,2,1,2,2,2,1,2] },
            { "Phrygian mode",[0,1,2,2,2,1,2,2] },
            { "Lydian mode",[0,2,2,2,1,2,2,1] },
            { "Mixolydian mode",[0,2,2,1,2,2,1,2] },
            { "Locrian mode",[0,1,2,2,1,2,2,2] },
            { "Ahava raba mode",[0,1,3,1,2,1,2,2] }
        };

        Dictionary<string,int[]> Scales { get; } = new() {
            { "Major",[0,2,2,1,2,2,2,1] },
            { "Natural minor",[0,2,1,2,2,1,2,2] },
            { "Harmonic minor",[0,2,1,2,2,1,3,1] },
            { "Melodic minor",[0,2,1,2,2,2,2,2] },
            { "Minor pentatonic",[0,3,2,2,3,2] },
            { "Pentatonic",[0,2,2,3,2,3] },
            { "Blues",[0,3,2,1,1,3] }
        };

        Dictionary<MusicPatternType,Dictionary<string,int[]>> _patterns;

        Dictionary<MusicPatternType,Dictionary<string,int[]>> Patterns {
            get {
                if(_patterns == null) {
                    _patterns = new() {
                        { MusicPatternType.Chords,Chords },
                        { MusicPatternType.Scales,Scales },
                        { MusicPatternType.Modes,Modes }
                    };
                }

                return _patterns;
            }
        }

        MusicPatternType PatternType { get; }
        bool IsKeyboard { get; }
        InstrumentNote[] OpenNotes { get; }
        int FretCount { get; }
        int VertScaleFretSpan => 4;

        #endregion


        #region Constructors

        public PatternGen(MusicPatternType pattern,InstrumentTuning tuning) {
            PatternType = pattern;
            IsKeyboard = tuning.Parent.InstrumentType == InstrumentType.Piano;
            OpenNotes = tuning.OpenNotes.ToArray();
            FretCount = tuning.Parent.FretCount;
        }

        #endregion

        #region Public Methods

        public IEnumerable<NoteGroupCollection> Generate() {
            if(IsKeyboard) {
                return GenKeyboardPattern();
            }

            return GenFretboardPattern();
        }

        #endregion

        #region Protected Variables

        #endregion

        #region Private Methods

        #region Keyboard

        IEnumerable<NoteGroupCollection> GenKeyboardPattern() {
            return null;
        }

        #endregion

        #region Fretboard

        IEnumerable<NoteGroupCollection> GenFretboardPattern() {
            if(PatternType == MusicPatternType.Chords) {
                return GetFretboardChords();
            }

            return GetFretboardScales(PatternType == MusicPatternType.Modes);
        }

        IEnumerable<NoteGroupCollection> GetFretboardScales(bool isMode) {
            var patterns = isMode ? Modes : Scales;
            var ngcl = new List<NoteGroupCollection>();
            foreach(var suffix in patterns.Select(x => x.Key)) {
                for(var cur_key_val = 0; cur_key_val < 12; cur_key_val++) {
                    var cur_key = (NoteType)cur_key_val;
                    var pattern = GenPattern(cur_key,suffix);
                    var pattern_inst_notes = GenNotes(pattern);
                    var blocks = pattern_inst_notes
                        .GroupBy(x => Math.Floor((x.FretNum + 0) / (double)VertScaleFretSpan));
                    var ngc = new NoteGroupCollection(PatternType,cur_key,suffix);
                    ngc.Groups.AddRange(blocks.Select((x,idx) => new NoteGroup(ngc,idx,AddScaleFingering(x))));
                    // foreach(var ng in ngc.Groups) {
                    //     Debug.WriteLine($"{cur_key} {suffix} #{ng.Position}");
                    //     foreach(var (str_ng,idx) in ng.Notes.GroupBy(x => x.StringNum).WithIndex()) {
                    //         var sb = new StringBuilder();
                    //         sb.Append($"{idx}: ");
                    //         foreach(var pn in str_ng) {
                    //             sb.Append($"{pn.FretNum} ");
                    //         }
                    //
                    //         Debug.WriteLine(sb.ToString());
                    //     }
                    // }

                    ngcl.Add(ngc);
                }
            }


            return ngcl;
        }

        IEnumerable<NoteGroupCollection> GetFretboardChords() {
            return null;
        }

        #endregion

        #region Helpers

        bool IsOrderedTuning() {
            for(var i = 1; i < OpenNotes.Length; i++) {
                if(OpenNotes[i].NoteId < OpenNotes[i - 1].NoteId) {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<PatternNote> AddScaleFingering(IEnumerable<InstrumentNote> notes) {
            var min_fret = notes.Where(x => x.FretNum > 0).Min(x => x.FretNum);
            var max_fret = notes.Where(x => x.FretNum > 0).Max(x => x.FretNum);
            var pnl = new List<PatternNote>();
            foreach(var note in notes) {
                var finger = 0;
                if(note.FretNum >= min_fret) {
                    finger = Math.Min(val1: 4,(max_fret - note.FretNum) + 1);
                }

                pnl.Add(new(finger,note));
            }

            return pnl;
        }

        IEnumerable<InstrumentNote> GenNotes(NoteType[] pattern) {
            var innl = new List<InstrumentNote>();
            foreach(var open_note in OpenNotes) {
                var cur_note = open_note; //.Clone();
                while(cur_note.FretNum <= FretCount) {
                    if(pattern.Contains(cur_note.Key)) {
                        innl.Add(cur_note);
                    }

                    cur_note = cur_note.Next;
                }
            }

            return innl;
        }

        NoteType[] GenPattern(NoteType key,string suffix) {
            var offsets = Patterns[PatternType][suffix];
            var ntl = new List<NoteType>();
            var note_val = (int)key;
            for(var i = 0; i < offsets.Length; i++) {
                note_val += offsets[i % offsets.Length];
                if(note_val >= 12) {
                    note_val = note_val - 12;
                }

                ntl.Add((NoteType)note_val);
            }

            return ntl.ToArray();
        }

//[0,2,2,1,2,2,2,1] 

        #endregion

        #endregion

        #region Commands

        #endregion
    }
}