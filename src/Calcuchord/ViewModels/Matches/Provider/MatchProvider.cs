using System;
using System.Collections.Generic;
using System.Linq;

namespace Calcuchord {
    public class MatchProvider {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        public Dictionary<NoteType,Dictionary<string,IEnumerable<MatchViewModel>>> PatternLookup { get; } = [];

        public MusicPatternType PatternType { get; }
        public Tuning Tuning { get; }

        #endregion

        #region Constructors

        public MatchProvider(MusicPatternType patternType,Tuning tuning) {
            PatternType = patternType;
            Tuning = tuning;
            if(Tuning == null) {
                PatternLookup.Clear();
                return;
            }

            var coll = Tuning.Collections[PatternType];
            for(int i = 0; i < 12; i++) {
                NoteType nt = (NoteType)i;
                if(coll.Where(x => x.Key == nt) is { } all_key_groups) {
                    try {
                        var key_suffix_lookup =
                            all_key_groups
                                .Where(x => !string.IsNullOrEmpty(x.SuffixKey))
                                .GroupBy(x => x.SuffixKey)
                                .ToDictionary(
                                    x => x.Key,
                                    x =>
                                        x.SelectMany(y => y.Patterns)
                                            .OrderBy(y => y.Position)
                                            .Select(y => CreateMatchViewModel(y,0))
                                );
                        PatternLookup.Add(nt,key_suffix_lookup);
                    } catch(Exception e) {
                        e.Dump();
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        MatchViewModel CreateMatchViewModel(NotePattern notePattern,double score) {
            return new MatchViewModel(PatternType,notePattern,score);
        }

        public double GetScore(NotePattern pattern,IEnumerable<NoteViewModel> matchNotes) {
            double score = 0;
            foreach(NoteViewModel mn in matchNotes) {
                if(pattern.Notes.Any(x => x.ColNum == mn.WorkingNoteNum && x.RowNum == mn.RowNum)) {
                    score += 1;
                    continue;
                }

                return 0;

            }

            return score;
        }

        #endregion

    }
}