using System;
using System.Collections.Generic;

namespace Calcuchord {

    public interface IPlatformServices {
        IStorageHelper StorageHelper { get; }
        IMidiPlayer MidiPlayer { get; }

    }

    public interface IPrimaryModel {
        string Id { get; }
        void CreateId(string forceId);
    }

    public interface IStorageHelper {
        string StorageDir { get; }
    }


    public interface IWebViewHelper {
        string ToneUrl { get; }
        void InitEnv(object config);
        bool ConfigureWebView(object wv);
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