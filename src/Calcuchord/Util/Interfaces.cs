using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Skia;

namespace Calcuchord {

    public interface IPlatformServices {
        IMidiPlayer MidiPlayer { get; }
        IMidiFileBuilder MidiFileBuilder { get; }
        IUriNavigator UriNavigator { get; }
        IStorageHelper StorageHelper { get; }
        IPrefsIo PrefsIo { get; }
        IPlatformInfo PlatformInfo { get; }
        ILog Logger { get; }
        IShareHtml ShareHtml { get; }
        IShareMidi ShareMidi { get; }
        ISharePdf SharePdf { get; }
    }

    public interface IMidiFileBuilder {
        Task CreateMidiScaleAsync(IEnumerable<IEnumerable<int>> toneSets,string filePath);
        Task CreateMidiChordAsync(IEnumerable<IEnumerable<int>> toneSets,string filePath);
    }

    public interface IShareHtml {
        Task ShareHtmlAsync(string html,string title);
    }

    public interface ISharePdf {
        Task SharePdfAsync(SKSvg svg,string title);
    }

    public interface IShareMidi {
        Task ShareMidiAsync(IEnumerable<IEnumerable<int>> toneSets,bool isScale,string title);
    }

    public interface ILog {
        void WriteLine(string message);
    }

    public interface IUriNavigator {
        void NavigateTo(string uri,object args);
    }

    public interface IPlatformInfo {
        bool IsMobile { get; }
        bool IsTablet { get; }
    }

    public interface IPrimaryModel {
        string Id { get; }
        void CreateId(string forceId);
    }

    public interface IStorageHelper {
        string StorageDir { get; }
        bool IsExternalWriteEnabled();
        event EventHandler ExternalWriteEnabled;
        void RequestExternalWritePermission();
    }

    public interface IPrefsIo {
        Task<string> ReadPrefsAsync();
        Task WritePrefsAsync(string prefsJson);
    }

    public interface IMidiPlayer {
        bool CanPlay { get; }
        void Init(object obj);
        void PlayChord(IEnumerable<IEnumerable<int>> tone_sets);
        void PlayScale(IEnumerable<IEnumerable<int>> tone_sets);

    }
}