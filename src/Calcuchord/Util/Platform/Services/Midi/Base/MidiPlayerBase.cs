using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;

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

        // NOTE PlayDelay should match in all js players (tone,wafp)
        protected int PlayDelayMs => 300;
        protected int ChordDelayMs => 30;
        protected int ScaleDelayMs => 300;

        public virtual bool CanPlay { get; protected set; } = true;

        protected DateTime? NextStopDt { get; set; }

        protected virtual bool IsPlaying {
            get {
                if(NextStopDt is { } lsdt) {
                    return DateTime.Now < lsdt;
                }

                return false;
            }
        }

        #endregion

        #region Events

        public event EventHandler Stopped;

        public virtual void Init(object obj) {
            if(OperatingSystem.IsWindows()) {
                Environment.SetEnvironmentVariable(
                    "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
                    "--autoplay-policy=no-user-gesture-required");
            }

        }

        public abstract void PlayChord(IEnumerable<Note> notes);

        public abstract void PlayScale(IEnumerable<Note> notes);

        public abstract void StopPlayback();

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

        protected void SetStopDt(int noteCount,bool isScale) {
            TimeSpan delay =
                TimeSpan.FromMilliseconds(PlayDelayMs + (noteCount * (isScale ? ScaleDelayMs : ChordDelayMs)));
            NextStopDt = DateTime.Now + delay;
            Dispatcher.UIThread.Post(
                async () => {
                    await Task.Delay(delay);
                    StopPlayback();
                });
        }

        protected void TriggerStopped() {
            Stopped?.Invoke(this,EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion


    }
}