using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AvaloniaWebView;

namespace Calcuchord {
    public partial class MidiPlayer {
        WebView _wv;

        void IMidiPlayer.Init(object obj) {
            if(obj is not WebView wv) {
                return;
            }

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

                _wv.NavigationCompleted += async (_,args) => {
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

        void IMidiPlayer.PlayChord(IEnumerable<Note> notes) {
            SetStopDt(notes.Count(),false);
            _wv.ExecuteScriptAsync($"playChord({string.Join(",",notes.Select(x => x.MidiTone))})");
        }

        void IMidiPlayer.PlayScale(IEnumerable<Note> notes) {
            SetStopDt(notes.Count(),true);
            _wv.ExecuteScriptAsync($"playScale({string.Join(",",notes.Select(x => x.MidiTone))})");
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

    }
}