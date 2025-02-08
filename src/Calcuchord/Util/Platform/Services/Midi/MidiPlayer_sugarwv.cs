using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaWebView;
using MonkeyPaste.Common;
using WebViewCore.Configurations;

namespace Calcuchord {

    public class MidiPlayer_sugarwv : MidiPlayerBase {
        WebView _wv;

        public override bool CanPlay =>
            !string.IsNullOrEmpty(PlayerUrl) &&
            File.Exists(PlayerUrl.ToPathFromUri());

        public string PlayerUrl {
            get {
                if(PlatformWrapper.Services is { } ps &&
                   ps.StorageHelper is { } sh &&
                   sh.StorageDir is { } storage_dir) {
                    return Path.Combine(storage_dir,"tone.html").ToFileSystemUriFromPath();
                }

                return null;
            }
        }

        public override void Init(object obj) {
            if(obj is WebViewCreationProperties config) {
                InitEnv(config);
            } else if(obj is Grid cntr_grid) {
                InitWebView(cntr_grid);
            }
        }

        protected virtual async Task ExecuteScriptAsync(string script) {
            if(_wv is null) {
                return;
            }

            try {
                await _wv.ExecuteScriptAsync(script);
            } catch(Exception ex) {
                ex.Dump();
            }

        }

        protected virtual void ConfigurePlatformWebView(WebView wv) {
            // for android and ios
        }

        void InitWebView(Grid cntr_grid) {
            WebView wv = new WebView
            {
                Width = 0,
                Height = 0,
                IsVisible = false
            };
            if(OperatingSystem.IsAndroid()) {
                // NOTE android won't load webview w/o being visible
                wv.Width = 1;
                wv.Height = 1;
                wv.IsVisible = true;
            }

            cntr_grid.Children.Insert(0,wv);

            _wv = wv;

            _wv.NavigationStarting += (sender,arg) => {
                Debug.WriteLine($"Navigating to {arg.Url}");
            };

            _wv.NavigationCompleted += (_,webViewUrlLoadedEventArg) => {
                if(webViewUrlLoadedEventArg.IsSuccess) {
                    CanPlay = true;
                    Debug.WriteLine("Tone.html successfully loaded");
                } else {
                    Debug.WriteLine("Error loading Tone.html page");
                }
            };
            _wv.Loaded += (_,_) => {
                ConfigurePlatformWebView(_wv);

                if(PlayerUrl is null) {
                    Debug.WriteLine("Error loading Tone.html assets");
                    return;
                }


                _wv.Url = new Uri(PlayerUrl);
            };
        }

        void InitEnv(WebViewCreationProperties config) {
            config.AreDevToolEnabled =
#if DEBUG
                true;
#else
                false;
#endif


            config.AreDefaultContextMenusEnabled = false;
            config.IsStatusBarEnabled = false;
            config.DefaultWebViewBackgroundColor = Color.FromArgb(Color.Transparent.ToArgb());

            if(!string.IsNullOrEmpty(PlayerUrl) &&
               PlayerUrl.ToPathFromUri() is { } tone_path &&
               tone_path.IsFile()) {
                config.BrowserExecutableFolder = Path.GetDirectoryName(tone_path);
                Debug.WriteLine($"Browser executable folder: {config.BrowserExecutableFolder}");
            }
        }

        public override void PlayChord(IEnumerable<Note> notes) {
            SetStopDt(notes.Count(),false);
            ExecuteScriptAsync($"playChord([{string.Join(",",GetMidiNotes(notes))}])")
                .FireAndForgetSafeAsync();
        }

        public override void PlayScale(IEnumerable<Note> notes) {
            SetStopDt(notes.Count(),true);
            ExecuteScriptAsync($"playScale([{string.Join(",",GetMidiNotes(notes))}])")
                .FireAndForgetSafeAsync();
        }

        public override void StopPlayback() {
            if(IsPlaying) {
                ExecuteScriptAsync("stopPlayback()").FireAndForgetSafeAsync();
            }

            if(NextStopDt != null) {
                NextStopDt = null;
                TriggerStopped();
            }
        }

    }
}