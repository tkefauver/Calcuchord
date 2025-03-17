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

        public double GetScore(NotePattern pattern,InstrumentNote[] matchNotes,MatchScoreMethodType scoring) {
            double score = 0;
            foreach(InstrumentNote mn in matchNotes) {
                double max_score = pattern.Notes.Max(x => x.Distance(mn,scoring));
                if(scoring == MatchScoreMethodType.Exact && max_score == 0) {
                    return 0;
                }

                score += max_score;

                // if(pattern.Notes.Any(x => x.ColNum == mn.WorkingNoteNum && x.RowNum == mn.RowNum)) {
                //     // exact
                //     score += 1;
                //     continue;
                // }
                //
                // if(scoring == MatchScoreMethodType.Exact) {
                //     return 0;
                // }
                // // find all notes in pattern with a matching tone,
                // // then the closest one of those on the instrument
                // if(pattern.Notes
                //        .Where(x => !x.IsMute && x.Key == mn.InstrumentNote.Key)
                //        .OrderBy(x => x.Distance(mn.InstrumentNote))
                //        .FirstOrDefault() is { } closest_pattern_match) {
                //     double dist = closest_pattern_match.Distance(mn.InstrumentNote);
                //
                //     score += 1 / (dist + 1d);
                //     continue;
                // }
                //
                // return 0;

            }

            // if(score == 0) {
            //     return 0;
            // }
            //
            // double score1 = matchNotes.Length / score;
            int pattern_len = pattern.Notes.Count(x => !x.IsMute) +
                              Math.Min(pattern.Notes.Count(x => x.IsMute),matchNotes.Count(x => x.IsMute));
            // double score2 = pattern_len / score; 
            // return (score1 + score2) / 2d;

            return score / pattern_len; //Math.Max(1,Math.Max(pattern.Notes.Count,matchNotes.Length));
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        MatchViewModel CreateMatchViewModel(NotePattern notePattern,double score) {
            return new MatchViewModel(PatternType,notePattern,score);
        }

        #endregion

    }
}