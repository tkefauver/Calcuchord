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

        IEnumerable<MatchViewModelBase> Items { get; }


        //MatchViewModelBase[] Items { get; }
        public Dictionary<NoteType,Dictionary<string,IEnumerable<MatchViewModelBase>>> PatternLookup { get; } = [];

        public MusicPatternType PatternType { get; }
        public Tuning Tuning { get; }

        #endregion

        #region Constructors

        public MatchProvider(MusicPatternType patternType,Tuning tuning) {
            PatternType = patternType;
            Tuning = tuning;
            if(Tuning == null) {
                Items = [];
            } else {
                Items =
                    Tuning.Collections[PatternType]
                        .SelectMany(x => x.Patterns)
                        .Select(x => CreateMatchViewModel(x,0))
                        .ToArray();

                var coll = Tuning.Collections[PatternType];
                for(int i = 0; i < 12; i++) {
                    NoteType nt = (NoteType)i;
                    if(coll.Where(x => x.Key == nt) is { } all_key_groups) {
                        var key_suffix_lookup =
                            all_key_groups
                                .GroupBy(x => x.SuffixKey)
                                .ToDictionary(
                                    x => x.Key,
                                    x =>
                                        x.SelectMany(y => y.Patterns)
                                            .OrderBy(y => y.Position)
                                            .Select(y => CreateMatchViewModel(y,0))
                                );
                        PatternLookup.Add(nt,key_suffix_lookup);
                    }
                }
            }

        }

        #endregion

        #region Public Methods

        public IEnumerable<MatchViewModelBase> GetAll() {
            return Items; //.Select(x => CreateMatchViewModel(x,1));
        }

        public IEnumerable<MatchViewModelBase> GetBookmarks() {
            return
                Items
                    .Where(x => x.IsBookmarked);
            //.Select(x => CreateMatchViewModel(x,1));
        }

        public IEnumerable<MatchViewModelBase> GetMatches(IEnumerable<NoteViewModel> matchNotes) {
            var results =
                Items.Select(x => (x,GetScore(x.NotePattern,matchNotes)))
                    .Where(x => x.Item2 > 0)
                    .Select(x => x.Item1);
            //.Select(x => CreateMatchViewModel(x.Item1,x.Item2));
            return results;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        MatchViewModelBase CreateMatchViewModel(NotePattern notePattern,double score) {
            switch(PatternType) {
                default:
                case MusicPatternType.Chords:
                    return new ChordMatchViewModel(notePattern,score);
                case MusicPatternType.Scales:
                    return new ScaleMatchViewModel(notePattern,score);
                case MusicPatternType.Modes:
                    return new ModeMatchViewModel(notePattern,score);
            }
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