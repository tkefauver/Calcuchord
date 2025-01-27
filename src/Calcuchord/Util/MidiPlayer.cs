using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaWebView;

namespace Calcuchord {

    public class MidiPlayer {

        #region Private Variables

        WebView _wv;

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

        public bool CanPlay { get; private set; }

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

        public void Init(WebView wv) {
            _wv = wv;
            if(PlatformWrapper.WebViewHelper is not { } wvh) {
                return;
            }

            _wv.Loaded += (sender,args) => {
                bool success = wvh.ConfigureWebView(_wv);
                if(!success ||
                   wvh.ToneUrl is not { } tone_url) {
                    Debug.WriteLine("Error loading Tone.html assets");
                    return;
                }

                _wv.NavigationCompleted += async (sender,args) => {
                    if(args.IsSuccess) {
                        CanPlay = true;
                        Debug.WriteLine("Tone.html successfully loaded");
                    } else {
                        Debug.WriteLine("Error loading Tone.html page");
                    }
                };
                _wv.Url = new(tone_url);
            };
        }

        public void PlayChord(int[] notes) {
            SetStopDt(notes.Length,false);
            _wv.ExecuteScriptAsync($"playChord({string.Join(",",notes)})");
        }

        public void PlayScale(int[] notes) {
            SetStopDt(notes.Length,true);
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