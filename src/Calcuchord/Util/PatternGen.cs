using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class PatternGen : IProgressIndicator {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Interfaces

        #region IProgressIndicator Implementation

        public double PercentDone =>
            CurrentProgressCount / TotalProgressCount;

        public string ProgressLabel { get; private set; }
        public event EventHandler ProgressChanged;

        double TotalProgressCount { get; set; }
        double CurrentProgressCount { get; set; }

        #endregion

        #endregion

        #region Properties

        Dictionary<MusicPatternType,Dictionary<string,int[]>> _patterns;

        Dictionary<MusicPatternType,Dictionary<string,int[]>> PatternsLookup {
            get {
                if(_patterns == null) {
                    _patterns = new() {
                        {
                            MusicPatternType.Chords,
                            new() {
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
                            }
                        }, {
                            MusicPatternType.Scales,
                            new() {
                                { "Major",[0,2,2,1,2,2,2,1] },
                                { "Natural minor",[0,2,1,2,2,1,2,2] },
                                { "Harmonic minor",[0,2,1,2,2,1,3,1] },
                                { "Melodic minor",[0,2,1,2,2,2,2,2] },
                                { "Minor pentatonic",[0,3,2,2,3,2] },
                                { "Pentatonic",[0,2,2,3,2,3] },
                                { "Blues",[0,3,2,1,1,3] }
                            }
                        }, {
                            MusicPatternType.Modes,
                            new() {
                                { "Dorian mode",[0,2,1,2,2,2,1,2] },
                                { "Phrygian mode",[0,1,2,2,2,1,2,2] },
                                { "Lydian mode",[0,2,2,2,1,2,2,1] },
                                { "Mixolydian mode",[0,2,2,1,2,2,1,2] },
                                { "Locrian mode",[0,1,2,2,1,2,2,2] },
                                { "Ahava raba mode",[0,1,3,1,2,1,2,2] }
                            }
                        }
                    };
                }

                return _patterns;
            }
        }

        MusicPatternType PatternType { get; }

        bool IsKeyboard =>
            Tuning.Parent.InstrumentType == InstrumentType.Piano;

        InstrumentNote[] OpenNotes =>
            Tuning.OpenNotes.ToArray();

        int FretCount =>
            Tuning.FretCount;

        int PatternFretSpan { get; }

        int StringCount =>
            OpenNotes.Length;

        Tuning Tuning { get; }

        #endregion

        #region Events

        #endregion


        #region Constructors

        public PatternGen(MusicPatternType pattern,Tuning tuning) {
            Tuning = tuning;
            PatternType = pattern;
            PatternFretSpan = GuessRealisticFretSpan(tuning);
        }

        #endregion

        #region Public Methods

        public async Task<IEnumerable<NoteGroupCollection>> GenerateAsync() {
            IEnumerable<NoteGroupCollection> result = null;
            if(IsKeyboard) {
                result = await GenKeyboardPatternAsync();
            } else {
                result = await GenFretboardPatternAsync();
            }
            result.ForEach(x => x.SetParent(Tuning));
            return result;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #region Keyboard

        async Task<IEnumerable<NoteGroupCollection>> GenKeyboardPatternAsync() {
            await Task.Delay(1);
            return null;
        }

        #endregion

        #region Fretboard

        async Task<IEnumerable<NoteGroupCollection>> GenFretboardPatternAsync() {
            if(PatternType == MusicPatternType.Chords) {
                return await GetFretboardChordsAsync();
            }

            return await GetFretboardScalesAsync(PatternType == MusicPatternType.Modes);
        }

        #region Scales/Modes

        async Task<IEnumerable<NoteGroupCollection>> GetFretboardScalesAsync(bool isMode) {
            await Task.Delay(1);
            var patterns = PatternsLookup[isMode ? MusicPatternType.Modes : MusicPatternType.Scales];
            var ngcl = new List<NoteGroupCollection>();
            foreach(string suffix in patterns.Select(x => x.Key)) {
                for(int cur_key_val = 0; cur_key_val < 12; cur_key_val++) {
                    NoteType cur_key = (NoteType)cur_key_val;
                    var pattern = GenPattern(cur_key,suffix);
                    var pattern_inst_notes = GenNotes(pattern);
                    var blocks = pattern_inst_notes
                        .GroupBy(x => Math.Floor((x.FretNum + 0) / (double)PatternFretSpan));
                    NoteGroupCollection ngc = new(PatternType,cur_key,suffix);
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

        #endregion

        #region Chords

        bool RejectNotAllNotes(InstrumentNote[] notes,NoteType[] pattern,
            IEnumerable<IEnumerable<InstrumentNote>> existing) {
            return pattern.Any(x => notes.All(y => y.Key != x));
        }

        bool RejectNotStartOnRoot(InstrumentNote[] notes,NoteType[] pattern,
            IEnumerable<IEnumerable<InstrumentNote>> existing) {
            return notes.OrderBy(x => x.StringNum).ThenBy(x => x.FretNum).FirstOrDefault().Key != pattern[0];
        }

        bool RejectNotesOnSameString(InstrumentNote[] notes,NoteType[] pattern,
            IEnumerable<IEnumerable<InstrumentNote>> existing) {
            return notes.GroupBy(x => x.StringNum).Any(x => x.Count() > 1);
        }

        bool RejectExists(InstrumentNote[] notes,NoteType[] pattern,IEnumerable<IEnumerable<InstrumentNote>> existing) {
            return existing.Any(x => !x.Where(y => y.FretNum > 0).Difference(notes.Where(z => z.FretNum > 0)).Any());
        }

        bool IsValidCombo(InstrumentNote[] notes,NoteType[] pattern,IEnumerable<IEnumerable<InstrumentNote>> existing) {
            Func<InstrumentNote[],NoteType[],IEnumerable<IEnumerable<InstrumentNote>>,bool>[] reject_funcs = [
                RejectNotAllNotes,
                RejectNotStartOnRoot,
                RejectNotesOnSameString,
                RejectExists
            ]; //10366
            foreach(var reject_func in reject_funcs) {
                if(reject_func.Invoke(notes,pattern,existing)) {
                    return false;
                }
            }
            return true;
        }

        IEnumerable<PatternNote> AddChordFingerings(IEnumerable<InstrumentNote> notes) {
            /*
             1. Go from lowest to highest fret and lowest
             to highest string using fingers from lowest to highest.

             2. If multiple notes at cur fret and higher frets have
             notes, barre current finger at current fret&

             3. If no notes on a fret after a fret w note(s)
             then skip that finger (like a power chord)&

             4. Fill in missing strings with mutes
             */
            var pnl = new List<PatternNote>();
            var fingered_fret_note_lookup =
                notes
                    .Where(x => x.FretNum > 0)
                    .GroupBy(x => x.FretNum)
                    .OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key,x => x.OrderBy(y => y.StringNum).Select(y => y));
            if(fingered_fret_note_lookup.Count != 0) {
                int min_fingered_fret = fingered_fret_note_lookup.Keys.Min();
                int max_fingered_fret = fingered_fret_note_lookup.Keys.Max();
                int cur_finger = 1;
                for(int cur_fret_num = min_fingered_fret;
                    cur_fret_num <= max_fingered_fret;
                    cur_fret_num++) {
                    bool incr_finger = true;
                    if(fingered_fret_note_lookup.TryGetValue(cur_fret_num,out var cur_fret_notes)) {
                        // fret has notes

                        int min_fret_str = cur_fret_notes.Min(x => x.StringNum);
                        int max_fret_str = cur_fret_notes.Max(x => x.StringNum);
                        bool do_bar = cur_fret_notes.Skip(1).Any() &&
                                      fingered_fret_note_lookup.Keys.Any(x => x > cur_fret_num);
                        // check if any lower frets in str range have notes (then can't bar)
                        bool can_bar = do_bar &&
                                       !notes
                                           .Any(
                                               x => x.FretNum >= 0 && x.FretNum < cur_fret_num &&
                                                    x.StringNum >= min_fret_str &&
                                                    x.StringNum <= max_fret_str);
                        foreach(InstrumentNote cur_fret_note in cur_fret_notes) {
                            if(cur_finger > 4) {
                                // reject
                                return null;
                            }
                            pnl.Add(new(cur_finger,cur_fret_note));
                            if(!can_bar) {
                                cur_finger++;
                                incr_finger = false;
                            }
                        }
                    }
                    if(incr_finger) {
                        cur_finger++;
                    }
                }
            }
            // add opens
            notes.Where(x => x.FretNum == 0).ForEach(x => pnl.Add(new(0,x)));
            // add mutes
            Enumerable
                .Range(0,StringCount).Where(x => notes.All(y => y.StringNum != x))
                .ForEach(x => pnl.Add(new(-1,InstrumentNote.Mute(x))));

            return pnl;
        }

        async Task<IEnumerable<NoteGroupCollection>> GetFretboardChordsAsync() {
            await Task.Delay(1);
            var patterns = PatternsLookup[MusicPatternType.Chords];
            var ngcl = new List<NoteGroupCollection>();
            int max_min_fret_num = FretCount - PatternFretSpan - 1;
            InitProgress(patterns.Count * 12);
            int cur_progress = 0;
            int cur_chord_count = 0;
            Stopwatch tsw = Stopwatch.StartNew();

            foreach(string suffix in patterns.Select(x => x.Key)) {
                for(int cur_key_val = 0; cur_key_val < 12; cur_key_val++) {
                    Stopwatch sw = Stopwatch.StartNew();
                    NoteType cur_key = (NoteType)cur_key_val;
                    var pattern = GenPattern(cur_key,suffix);
                    var pattern_inst_notes = GenNotes(pattern);
                    var valid_patterns = new List<IEnumerable<InstrumentNote>>();
                    for(int min_fret_num = 0; min_fret_num <= max_min_fret_num; min_fret_num++) {
                        int max_fret_num = min_fret_num + PatternFretSpan;
                        if(min_fret_num > 0) {
                            max_fret_num--;
                        }
                        var block_notes = pattern_inst_notes.Where(
                            x => x.FretNum >= min_fret_num && x.FretNum <= max_fret_num);
                        var combos = block_notes.Combinations().Where(x => x.Length >= pattern.Length);
                        foreach(var combo in combos) {
                            if(!IsValidCombo(combo,pattern,valid_patterns)) {
                                continue;
                            }
                            valid_patterns.Add(combo);
                        }
                    }
                    NoteGroupCollection ngc = new(PatternType,cur_key,suffix);
                    foreach((var vp,int idx) in valid_patterns.OrderBy(x => x.Min(y => y.FretNum))
                                .ThenBy(x => x.Min(y => y.StringNum)).WithIndex()) {
                        if(AddChordFingerings(vp) is not { } fingerings) {
                            continue;
                        }
                        ngc.Groups.Add(new(ngc,idx,fingerings.OrderBy(x => x.StringNum).ThenBy(x => x.FretNum)));
                        cur_chord_count++;
                    }
                    ngcl.Add(ngc);
                    UpdateProgress(++cur_progress,$"{cur_chord_count} chords found...{sw.ElapsedMilliseconds}ms");
                }
            }

            Debug.WriteLine($"{cur_chord_count} total chords for '{Tuning}' in {tsw.ElapsedMilliseconds}ms");


            return ngcl;
        }

        #endregion

        #endregion

        #region Helpers

        #region Progress

        void InitProgress(int totalCount) {
            TotalProgressCount = totalCount;
            UpdateProgress(0,"Initializing...");
        }

        void UpdateProgress(int curCount,string label) {
            CurrentProgressCount = curCount;
            ProgressLabel = label;
            Debug.WriteLine($"{ProgressLabel} [{(int)(PercentDone * 100)}%]");
            ProgressChanged?.Invoke(this,EventArgs.Empty);
        }

        #endregion

        int GuessRealisticFretSpan(Tuning tuning) {
            // from https://www.statcrunch.com/reports/view?reportid=5152&tab=preview
            // The spread of the male hands was 7 in-11 in,
            // which is a lager average than the girl's span of 6.5 in-9.5 in.
            // The center for the females is shorter than the males.
            // The male's center is between 8.5 inches and 9.5 inches and the
            // female center is somewhere around 7.5-8 inches.

            // male avg: 9
            // female avg: 8
            // avg: 8.5
            // avg finger span (half): 4.25 
            return 4;
        }

        bool IsOrderedTuning() {
            for(int i = 1; i < OpenNotes.Length; i++) {
                if(OpenNotes[i].NoteId < OpenNotes[i - 1].NoteId) {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<PatternNote> AddScaleFingering(IEnumerable<InstrumentNote> notes) {
            int min_fret = notes.Where(x => x.FretNum > 0).Min(x => x.FretNum);
            int max_fret = notes.Where(x => x.FretNum > 0).Max(x => x.FretNum);
            var pnl = new List<PatternNote>();
            foreach(InstrumentNote note in notes) {
                int finger = 0;
                if(note.FretNum >= min_fret) {
                    finger = Math.Min(4,(max_fret - note.FretNum) + 1);
                }

                pnl.Add(new(finger,note));
            }

            return pnl;
        }

        IEnumerable<InstrumentNote> GenNotes(NoteType[] pattern) {
            var innl = new List<InstrumentNote>();
            foreach(InstrumentNote open_note in OpenNotes) {
                InstrumentNote cur_note = open_note; //.Clone();
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
            int[] offsets = PatternsLookup[PatternType][suffix];
            var ntl = new List<NoteType>();
            int note_val = (int)key;
            for(int i = 0; i < offsets.Length; i++) {
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