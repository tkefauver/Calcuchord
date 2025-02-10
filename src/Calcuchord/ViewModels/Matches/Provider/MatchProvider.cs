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

        IEnumerable<NoteGroup> Items =>
            Tuning == null ? [] : Tuning.Collections[PatternType].SelectMany(x => x.Groups);

        MusicPatternType PatternType { get; }
        Tuning Tuning { get; }

        #endregion

        #region Constructors

        public MatchProvider(MusicPatternType patternType,Tuning tuning) {
            PatternType = patternType;
            Tuning = tuning;
        }

        #endregion

        #region Public Methods

        public IEnumerable<MatchViewModelBase> GetAll() {
            return Items.Select(x => CreateMatchViewModel(x,1));
        }

        public IEnumerable<MatchViewModelBase> GetBookmarks() {
            return
                Items
                    .Where(x => x.IsBookmarked)
                    .Select(x => CreateMatchViewModel(x,1));
        }

        public IEnumerable<MatchViewModelBase> GetMatches(IEnumerable<NoteViewModel> matchNotes) {
            var results =
                Items.Select(x => (x,GetScore(x,matchNotes)))
                    .Where(x => x.Item2 > 0)
                    .Select(x => CreateMatchViewModel(x.Item1,x.Item2));
            return results;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        MatchViewModelBase CreateMatchViewModel(NoteGroup noteGroup,double score) {
            switch(PatternType) {
                default:
                case MusicPatternType.Chords:
                    return new ChordMatchViewModel(noteGroup,score);
                case MusicPatternType.Scales:
                    return new ScaleMatchViewModel(noteGroup,score);
                case MusicPatternType.Modes:
                    return new ModeMatchViewModel(noteGroup,score);
            }
        }

        double GetScore(NoteGroup group,IEnumerable<NoteViewModel> matchNotes) {
            double score = 0;
            foreach(NoteViewModel mn in matchNotes) {
                if(group.Notes.Any(x => x.NoteNum == mn.WorkingNoteNum && x.RowNum == mn.RowNum)) {
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