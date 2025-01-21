using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaWebView;

namespace Calcuchord {
    public class MidiPlayer {
        #region Private Variables

        readonly WebView _wv;

        #endregion

        #region Constants

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        int ChordDelayMs => 30;
        int ScaleDelayMs => 300;

        bool CanPlay { get; set; }

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

        public MidiPlayer(WebView wv) {
            _wv = wv;
            CanPlay = PlatformWrapper.WebViewHelper.ConfigureWebView(_wv);
        }

        #endregion

        #region Public Methods

        public void PlayChord(int[] notes) {
            SetStopDt(notes.Length,isScale: false);
            _wv.ExecuteScriptAsync($"playChord({string.Join(",",notes)})");
        }

        public void PlayScale(int[] notes) {
            SetStopDt(notes.Length,isScale: true);
            _wv.ExecuteScriptAsync($"playScale({string.Join(",",notes)})");
        }

        public void StopPlayback() {
            if(IsPlaying) {
                _wv.ExecuteScriptAsync("stopPlayback()");
            }

            if(NextStopDt != null) {
                NextStopDt = null;
                Stopped?.Invoke(this,EventArgs.Empty);
            }
        }

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
    }
}