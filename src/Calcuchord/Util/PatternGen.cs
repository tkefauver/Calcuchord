using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using MonkeyPaste.Common;

namespace Calcuchord {

    public class PatternGen : IProgressIndicator {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Interfaces

        #region IProgressIndicator Implementation

        public event EventHandler<double> ProgressChanged;

        int TotalChordProgressCount { get; set; }
        int TotalScaleProgressCount { get; set; }
        int TotalModeProgressCount { get; set; }

        int CurChordProgressCount { get; set; }
        int CurScaleProgressCount { get; set; }
        int CurModeProgressCount { get; set; }
        int TotalProgressCount => TotalChordProgressCount + TotalScaleProgressCount + TotalModeProgressCount;
        public int CurrentProgressCount => CurChordProgressCount + CurScaleProgressCount + CurModeProgressCount;
        public int CurItemCount { get; set; }

        #endregion

        #endregion

        #region Properties

        Dictionary<ChordSuffixType,int[]> Chords { get; } = new Dictionary<ChordSuffixType,int[]>
        {
            { ChordSuffixType.Num5,[0,7] },
            { ChordSuffixType.major,[0,4,3] },
            { ChordSuffixType.minor,[0,3,4] },
            { ChordSuffixType.dim,[0,3,3] },
            { ChordSuffixType.aug,[0,4,4] },
            { ChordSuffixType.sus2,[0,2,5] },
            { ChordSuffixType.sus4,[0,5,2] },
            { ChordSuffixType.maj7,[0,4,3,4] },
            { ChordSuffixType.m7,[0,3,4,3] },
            { ChordSuffixType.Num7,[0,4,3,3] },
            { ChordSuffixType.m7b5,[0,3,3,4] },
            { ChordSuffixType.dim7,[0,3,3,3] },
            { ChordSuffixType.Num9,[0,4,3,3,4] },
            { ChordSuffixType.maj9,[0,4,3,4,3] },
            { ChordSuffixType.m9,[0,3,4,3,4] },
            { ChordSuffixType.Num11,[0,4,3,3,4,3] },
            { ChordSuffixType.maj11,[0,4,3,4,3,3] },
            { ChordSuffixType.m11,[0,3,4,3,4,3] }
        };

        Dictionary<ScaleSuffixType,int[]> Scales { get; } = new Dictionary<ScaleSuffixType,int[]>
        {
            { ScaleSuffixType.Major,[0,2,2,1,2,2,2,1] },
            { ScaleSuffixType.NaturalMinor,[0,2,1,2,2,1,2,2] },
            { ScaleSuffixType.HarmonicMinor,[0,2,1,2,2,1,3,1] },
            { ScaleSuffixType.MelodicMinor,[0,2,1,2,2,2,2,2] },
            { ScaleSuffixType.MinorPentatonic,[0,3,2,2,3,2] },
            { ScaleSuffixType.Pentatonic,[0,2,2,3,2,3] },
            { ScaleSuffixType.Blues,[0,3,2,1,1,3] }
        };

        Dictionary<ModeSuffixType,int[]> Modes { get; } = new Dictionary<ModeSuffixType,int[]>
        {
            { ModeSuffixType.Dorian,[0,2,1,2,2,2,1,2] },
            { ModeSuffixType.Phrygian,[0,1,2,2,2,1,2,2] },
            { ModeSuffixType.Lydian,[0,2,2,2,1,2,2,1] },
            { ModeSuffixType.Mixolydian,[0,2,2,1,2,2,1,2] },
            { ModeSuffixType.Locrian,[0,1,2,2,1,2,2,2] },
            { ModeSuffixType.AhavaRaba,[0,1,3,1,2,1,2,2] }
        };

        Dictionary<MusicPatternType,Dictionary<string,int[]>> _patterns;

        Dictionary<MusicPatternType,Dictionary<string,int[]>> PatternsLookup {
            get {
                if(_patterns == null) {
                    _patterns = new()
                    {
                        {
                            MusicPatternType.Chords,
                            Chords.ToDictionary(x => x.Key.ToString(),x => x.Value)
                        },
                        {
                            MusicPatternType.Scales,
                            Scales.ToDictionary(x => x.Key.ToString(),x => x.Value)
                        },
                        {
                            MusicPatternType.Modes,
                            Modes.ToDictionary(x => x.Key.ToString(),x => x.Value)
                        }
                    };
                }

                return _patterns;
            }
        }

        public MusicPatternType PatternType { get; private set; }

        bool IsKeyboard =>
            Tuning.Parent.InstrumentType == InstrumentType.Piano;

        InstrumentNote[] OpenNotes =>
            Tuning.OpenNotes.ToArray();

        int FretCount =>
            Tuning.Parent.ColCount;

        int PatternOpenFretSpan { get; }

        int StringCount =>
            OpenNotes.Length;

        Tuning Tuning { get; }

        CancellationToken Ct { get; set; }

        #endregion

        #region Events

        #endregion

        #region Constructors

        public PatternGen(Tuning tuning) {
            Tuning = tuning;
            PatternOpenFretSpan = GuessRealisticFretSpan(tuning);
        }

        #endregion

        #region Public Methods

        public async Task<Dictionary<MusicPatternType,IEnumerable<PatternKeyCollection>>> GenerateAsync(
            CancellationToken ct) {
            DispatcherTimer dt = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            dt.Tick += DtOnTick;

            void DtOnTick(object sender,EventArgs e) {
                UpdateProgress();
            }

            dt.Start();
            var result = new Dictionary<MusicPatternType,IEnumerable<PatternKeyCollection>>();
            var patterns = new[]
                { MusicPatternType.Modes,MusicPatternType.Scales,MusicPatternType.Chords };
            try {
                foreach(MusicPatternType pattern in patterns) {
                    PatternType = pattern;
                    result.Add(pattern,await GenerateAsync_internal(ct));
                }
            } catch(Exception) {
                dt.Tick -= DtOnTick;
                throw;
            }

            dt.Tick -= DtOnTick;
            return result;
        }

        async Task<IEnumerable<PatternKeyCollection>> GenerateAsync_internal(CancellationToken ct) {
            Stopwatch sw = Stopwatch.StartNew();
            Ct = ct;

            try {
                IEnumerable<PatternKeyCollection> result = null;
                if(IsKeyboard) {
                    result = await GenKeyboardPatternAsync();
                } else {
                    result = await GenFretboardPatternAsync();
                }

                foreach(PatternKeyCollection ngc in result) {
                    ngc.Patterns.ForEach(x => x.CreateId(null));
                    ngc.SetParent(Tuning);
                }

                if(PatternType == MusicPatternType.Chords) {
                    PlatformWrapper.Services.Logger.WriteLine(
                        $"Chord Gen complete. Total time: {sw.ElapsedMilliseconds} ms. {result.SelectMany(x => x.Patterns).Count()} groups.");
                }


                return result;
            } catch(Exception ex) {
                ex.Dump();
            }

            return [];
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #region Keyboard

        async Task<IEnumerable<PatternKeyCollection>> GenKeyboardPatternAsync() {
            await Task.Delay(1,Ct);
            var patterns = PatternsLookup[PatternType];
            var ngcl = new List<PatternKeyCollection>();
            foreach(string suffix in patterns.Select(x => x.Key)) {
                for(int cur_key_val = 0; cur_key_val < 12; cur_key_val++) {
                    NoteType cur_key = (NoteType)cur_key_val;
                    var pattern = GenPattern(cur_key,suffix);
                    var all_pattern_inst_notes = GenNotes(pattern);
                    // only return 1 set of pattern for chords or 1 set + next root for scales/modes
                    // int result_len = pattern.Length + 1;
                    // int pattern_count = 1;
                    // if(PatternType == MusicPatternType.Chords) {
                    //     // 
                    //     int diff = all_pattern_inst_notes.Count() % pattern.Length;
                    //     
                    //     result_len = (all_pattern_inst_notes.Count() - diff) * pattern.Length;
                    //     pattern_count = (int)(result_len / pattern.Length);
                    // }
                    PatternKeyCollection ngc = new PatternKeyCollection(PatternType,cur_key,suffix);
                    var all_pattern_notes =
                        all_pattern_inst_notes
                            .Select(x => new PatternNote(0,x));
                    var cur_pattern = new List<PatternNote>();
                    foreach((PatternNote pn,int idx) in all_pattern_notes.WithIndex()) {
                        cur_pattern.Add(pn);
                        if(PatternType == MusicPatternType.Chords) {
                            if(cur_pattern.Count == pattern.Length) {
                                ngc.Patterns.Add(new NotePattern(ngc,ngc.Patterns.Count - 1,cur_pattern.ToList()));
                                cur_pattern.Clear();
                            }
                        } else {
                            if(cur_pattern.Count == pattern.Length + 1 || pn == all_pattern_notes.Last()) {
                                ngc.Patterns.Add(new NotePattern(ngc,0,cur_pattern));
                                break;
                            }
                        }
                    }


                    ngcl.Add(ngc);
                }
            }

            return ngcl;
        }

        #endregion

        #region Fretboard

        async Task<IEnumerable<PatternKeyCollection>> GenFretboardPatternAsync() {
            if(PatternType == MusicPatternType.Chords) {
                return await GetFretboardChordsAsync();
            }

            return await GetFretboardScalesAsync();
        }

        #region Scales/Modes

        async Task<IEnumerable<PatternKeyCollection>> GetFretboardScalesAsync() {
            await Task.Delay(1,Ct);
            var patterns = PatternsLookup[PatternType];
            var ngcl = new List<PatternKeyCollection>();
            CurItemCount = 0;

            foreach(string suffix in patterns.Select(x => x.Key)) {
                for(int cur_key_val = 0; cur_key_val < 12; cur_key_val++) {
                    NoteType cur_key = (NoteType)cur_key_val;
                    var pattern = GenPattern(cur_key,suffix);
                    var pattern_inst_notes = GenNotes(pattern);
                    var blocks = pattern_inst_notes
                        .GroupBy(x => Math.Floor((x.NoteNum + 0) / (double)PatternOpenFretSpan));
                    PatternKeyCollection ngc = new PatternKeyCollection(PatternType,cur_key,suffix);
                    ngc.Patterns.AddRange(
                        blocks.Select(
                            (x,idx) => new NotePattern(ngc,idx,AddScaleFingering(x))));
                    CurItemCount += ngc.Patterns.Count;
                    if(PatternType == MusicPatternType.Modes) {
                        CurModeProgressCount++;
                    } else {
                        CurScaleProgressCount++;
                    }

                    ngcl.Add(ngc);
                }

                if(OperatingSystem.IsBrowser()) {
                    await Task.Delay(1,Ct);
                }

                //UpdateProgress();
            }

            return ngcl;
        }

        #endregion

        #region Chords

        bool RejectExists(IEnumerable<InstrumentNote> notes,IEnumerable<IEnumerable<InstrumentNote>> existing) {
            return existing.Any(x => x.Difference(notes).None());
        }

        bool RejectNotEnoughNotes(IEnumerable<InstrumentNote> notes,NoteType[] pattern) {
            return notes.Count() < pattern.Length;
        }

        bool RejectNotAllNotes(IEnumerable<InstrumentNote> notes,NoteType[] pattern) {
            return pattern.Any(x => notes.All(y => y.Key != x));
        }

        bool RejectNotStartOnRoot(IEnumerable<InstrumentNote> notes,NoteType[] pattern) {
            if(notes.OrderBy(x => x.RowNum).ThenBy(x => x.NoteNum).FirstOrDefault() is { } root_note) {
                return root_note.Key != pattern[0];
            }

            return true;
        }

        bool RejectNotesOnSameString(IEnumerable<InstrumentNote> notes,NoteType[] pattern) {
            return notes.GroupBy(x => x.RowNum).Any(x => x.Count() > 1);
        }

        bool IsValidCombo(IEnumerable<InstrumentNote> notes,NoteType[] pattern) {
            Func<IEnumerable<InstrumentNote>,NoteType[],bool>[] reject_funcs =
            [
                RejectNotEnoughNotes,
                RejectNotAllNotes,
                RejectNotStartOnRoot,
                RejectNotesOnSameString
            ];
            foreach(var reject_func in reject_funcs) {
                if(reject_func.Invoke(notes,pattern)) {
                    return false;
                }
            }

            return true;
        }


        async Task<IEnumerable<PatternKeyCollection>> GetFretboardChordsAsync() {
            var patterns = PatternsLookup[MusicPatternType.Chords];
            int max_min_fret_num = FretCount - PatternOpenFretSpan - 1;
            InitProgress(patterns.Count * 12);
            CurItemCount = 0;
            Stopwatch tsw = Stopwatch.StartNew();
            IEnumerable<PatternKeyCollection> ngcl = ngcl = await Task.WhenAll(
                patterns.SelectMany(
                    x =>
                        Enumerable.Range(0,12)
                            .Select(
                                y =>
                                    GetChordGroupAsync(x.Key,(NoteType)y,0,max_min_fret_num,true))));

            PlatformWrapper.Services.Logger.WriteLine(
                $"{CurItemCount} total chords for '{Tuning}' in {tsw.ElapsedMilliseconds}ms");


            return ngcl;
        }

        void CreateBlockCombo_helper(InstrumentNote[][] lookup,int str_num,List<InstrumentNote> notes) {

        }

        List<List<InstrumentNote>> CreateBlockCombinations(IEnumerable<InstrumentNote> blockNotes) {
            var lookup = Enumerable.Range(0,StringCount).Select(x => blockNotes.Where(y => y.RowNum == x).ToArray())
                .ToArray();
            var combos = new List<List<InstrumentNote>>();

            void AddCombos(InstrumentNote note,List<List<InstrumentNote>> cur,int str_num) {
                if(note != null) {
                    cur.ForEach(x => x.Add(note));
                    cur.Add([note]);
                }

                for(int i = str_num + 1; i < StringCount; i++) {
                    for(int j = 0; j < lookup[i].Length + 1; j++) {
                        if(j >= lookup[i].Length) {
                            AddCombos(null,cur,i);
                        } else {
                            AddCombos(lookup[i][j],cur,i);
                        }
                    }
                }

            }

            for(int i = 0; i < StringCount; i++) {
                var str_notes = lookup[i];
                for(int j = 0; j < str_notes.Length; j++) {
                    AddCombos(str_notes[j],combos,j);
                }
            }

            return combos;
        }

        async Task<PatternKeyCollection> GetChordGroupAsync(string suffix,NoteType cur_key,int min_fret_num,
            int max_min_fret_num,bool is_parallel = false) {
            var pattern = GenPattern(cur_key,suffix);
            var pattern_inst_notes = GenNotes(pattern);
            var valid_patterns = new List<IEnumerable<InstrumentNote>>();
            PatternKeyCollection ngc = new PatternKeyCollection(PatternType,cur_key,suffix);

            for(; min_fret_num <= max_min_fret_num; min_fret_num++) {
                int max_fret_num = min_fret_num + PatternOpenFretSpan;
                if(min_fret_num > 0) {
                    // block of 4 on open, then 3
                    max_fret_num--;
                }

                var block_notes = pattern_inst_notes.Where(
                    x => x.NoteNum >= min_fret_num && x.NoteNum <= max_fret_num);

                //var combos = CreateBlockCombinations(block_notes).Select(x=>x.Distinct()).Where(x=>x.Count() >= pattern.Length).Distinct().ToList();
                var combos = block_notes.PowerSet().Where(x => x.Length >= pattern.Length);
                foreach(var combo in combos) {
                    if(!IsValidCombo(combo,pattern) ||
                       RejectExists(combo,valid_patterns) ||
                       AddChordFingerings(combo) is not { } fingered_pattern) {
                        continue;
                    }

                    valid_patterns.Add(combo);
                    ngc.Patterns.Add(
                        new NotePattern(ngc,0,fingered_pattern.OrderBy(x => x.RowNum).ThenBy(x => x.NoteNum)));
                    CurItemCount++;
                }

                if(is_parallel) {
                    await Task.Delay(5,Ct);
                }

            }

            // set pattern position by lowest note
            ngc.Patterns = ngc.Patterns.OrderBy(x => x.Notes.Min(y => y.NoteId)).ToList();
            ngc.Patterns.ForEach((x,idx) => x.Position = idx);
            CurChordProgressCount++;
            //UpdateProgress();
            return ngc;
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
                    .Where(x => x.NoteNum > 0)
                    .GroupBy(x => x.NoteNum)
                    .OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key,x => x.OrderBy(y => y.RowNum).Select(y => y));
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

                        int min_fret_str = cur_fret_notes.Min(x => x.RowNum);
                        int max_fret_str = cur_fret_notes.Max(x => x.RowNum);
                        bool do_bar = cur_fret_notes.Skip(1).Any() &&
                                      fingered_fret_note_lookup.Keys.Any(x => x > cur_fret_num);
                        // check if any lower frets in str range have notes (then can't bar)
                        bool can_bar = do_bar &&
                                       !notes
                                           .Any(
                                               x => x.NoteNum >= 0 &&
                                                    x.NoteNum < cur_fret_num &&
                                                    x.RowNum >= min_fret_str &&
                                                    x.RowNum <= max_fret_str);
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
            notes.Where(x => x.NoteNum == 0).ForEach(x => pnl.Add(new(0,x)));
            // add mutes
            Enumerable
                .Range(0,StringCount).Where(x => notes.All(y => y.RowNum != x))
                .ForEach(x => pnl.Add(new(-1,InstrumentNote.Mute(x))));

            return pnl;
        }

        string GetSignature(IEnumerable<InstrumentNote> notes) {
            return string.Join(" ",notes.OrderBy(x => x.RowNum).Select(x => x.NoteNum.ToString()));
        }

        #endregion

        #endregion

        #region Helpers

        #region Progress

        void InitProgress(int totalCount) {
            TotalChordProgressCount = Chords.Count * 12;
            TotalScaleProgressCount = Scales.Count * 12;
            TotalModeProgressCount = Modes.Count * 12;

        }

        void UpdateProgress() {
            if(Ct.IsCancellationRequested) {
                throw new OperationCanceledException();
            }

            double percent = CurrentProgressCount / (double)TotalProgressCount;
            PlatformWrapper.Services.Logger.WriteLine(
                $"Gen Progress: [{(int)(percent * 100)}%] Count: {CurrentProgressCount}");

            ProgressChanged?.Invoke(this,percent);

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
            int min_fret = notes.Where(x => x.NoteNum > 0).Min(x => x.NoteNum);
            int max_fret = notes.Where(x => x.NoteNum > 0).Max(x => x.NoteNum);
            var pnl = new List<PatternNote>();
            foreach(InstrumentNote note in notes) {
                int finger = 0;
                if(note.NoteNum >= min_fret) {
                    finger = Math.Min(4,(note.NoteNum - min_fret) + 1);
                }

                pnl.Add(new(finger,note));
            }

            return pnl;
        }

        IEnumerable<InstrumentNote> GenNotes(NoteType[] pattern) {
            var innl = new List<InstrumentNote>();
            foreach(InstrumentNote open_note in OpenNotes) {
                InstrumentNote cur_note = open_note; //.Clone();
                while(cur_note.NoteNum <= FretCount) {
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

        #endregion

        #endregion

        #region Commands

        #endregion

    }
}