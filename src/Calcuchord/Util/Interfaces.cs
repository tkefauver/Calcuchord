using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcuchord {

    public interface IPlatformServices {
        IUriNavigator UriNavigator { get; }
        IStorageHelper StorageHelper { get; }
        IMidiPlayer MidiPlayer { get; }
        IPrefsIo PrefsIo { get; }
        IPlatformInfo PlatformInfo { get; }
        ILog Logger { get; }
    }

    public interface ILog {
        void WriteLine(string message);
    }

    public interface IUriNavigator {
        void NavigateTo(string uri);
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
    }

    public interface IPrefsIo {
        Task<string> ReadPrefsAsync();
        Task WritePrefsAsync(string prefsJson);
    }


    public interface IWebViewHelper {
        string ToneUrl { get; }
        void InitEnv(object config);
        bool ConfigureWebView(object wv);
        bool IsSupported { get; }
    }

    public interface IMidiPlayer {
        bool CanPlay { get; }
        void Init(object obj);
        void PlayChord(IEnumerable<Note> notes);
        void PlayScale(IEnumerable<Note> notes);

    }
}