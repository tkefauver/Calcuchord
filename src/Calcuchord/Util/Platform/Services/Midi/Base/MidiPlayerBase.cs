using System;
using System.Collections.Generic;
using System.Linq;

namespace Calcuchord {
    public abstract class MidiPlayerBase : IMidiPlayer {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        public virtual bool CanPlay { get; protected set; } = true;

        #endregion

        #region Events

        public virtual void Init(object obj) {
            if(OperatingSystem.IsWindows()) {
                Environment.SetEnvironmentVariable(
                    "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
                    "--autoplay-policy=no-user-gesture-required");
            }

        }

        public abstract void PlayChord(IEnumerable<Note> notes);

        public abstract void PlayScale(IEnumerable<Note> notes);

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        protected virtual int GetMidiNote(Note note) {
            return note.MidiTone;
        }

        protected int[] GetMidiNotes(IEnumerable<Note> notes) {
            return notes.Where(x => !x.IsMute).Select(x => GetMidiNote(x)).ToArray();
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion


    }
}