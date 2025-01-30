using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Calcuchord {
    public partial class MidiPlayer : IMidiPlayer {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        static MidiPlayer _instance;
        public static MidiPlayer Instance => _instance ??= new();

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        int ChordDelayMs => 30;
        int ScaleDelayMs => 300;

        public bool CanPlay { get; protected set; } = true;

        DateTime? NextStopDt { get; set; }

        bool IsPlaying {
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

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        void SetStopDt(int noteCount,bool isScale) {
            TimeSpan delay = TimeSpan.FromMilliseconds(noteCount * (isScale ? ScaleDelayMs : ChordDelayMs));
            NextStopDt = DateTime.Now + delay;
            Dispatcher.UIThread.Post(
                async () => {
                    await Task.Delay(delay);
                    StopPlayback();
                });
        }

        #endregion

        #region Commands

        #endregion


    }
}