using System;
using System.Collections.Generic;
using AvaloniaWebView;
using WebViewCore.Configurations;

namespace Calcuchord {
    public interface IStorageHelper {
        string StorageDir { get; }
    }


    public interface IWebViewHelper {
        string ToneUrl { get; }
        void InitEnv(WebViewCreationProperties config);
        bool ConfigureWebView(WebView wv);
        bool IsSupported { get; }
    }

    public interface IProgressIndicator {
        double PercentDone { get; }
        string ProgressLabel { get; }
        event EventHandler ProgressChanged;
    }

    public interface IMidiPlayer {
        bool CanPlay { get; }
        event EventHandler Stopped;
        void Init(object obj);
        void PlayChord(IEnumerable<Note> notes);
        void PlayScale(IEnumerable<Note> notes);
        void StopPlayback();

    }
}