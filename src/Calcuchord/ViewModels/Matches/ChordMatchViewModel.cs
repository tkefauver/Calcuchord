namespace Calcuchord {
    public class ChordMatchViewModel : MatchViewModelBase {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        #endregion

        #region View Models

        #endregion

        #region Appearance

        #endregion

        #region Layout

        #endregion

        #region State

        public override MusicPatternType MatchPatternType =>
            MusicPatternType.Chords;

        #endregion

        #region Instrument

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public ChordMatchViewModel() {
        }

        public ChordMatchViewModel(NoteGroup noteGroup,double score) : base(noteGroup,score) {
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        protected override void PlayGroupMidi() {
            if(PlatformWrapper.Services is not { } ps ||
               ps.MidiPlayer is not { } mp) {
                return;
            }

            mp.PlayChord(NoteGroup.Notes.ToArray());
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}