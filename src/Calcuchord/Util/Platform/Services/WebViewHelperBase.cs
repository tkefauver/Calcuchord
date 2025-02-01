using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using AvaloniaWebView;
using MonkeyPaste.Common;
using WebViewCore.Configurations;

namespace Calcuchord {
    public class WebViewHelperBase : IWebViewHelper {
        public virtual bool IsSupported => !OperatingSystem.IsLinux();

        public virtual string ToneUrl {
            get {
                if(PlatformWrapper.StorageHelper is { } sh &&
                   sh.StorageDir is { } storage_dir) {
                    return Path.Combine(storage_dir,"tone.html").ToFileSystemUriFromPath();
                }

                return null;
            }
        }

        public virtual void InitEnv(WebViewCreationProperties config) {
            config.AreDevToolEnabled =
#if DEBUG
                true;
#else
                false;
#endif

            config.AreDefaultContextMenusEnabled = false;
            config.IsStatusBarEnabled = false;
            config.DefaultWebViewBackgroundColor = Color.FromArgb(Color.Transparent.ToArgb());

            if(!string.IsNullOrEmpty(ToneUrl) &&
               ToneUrl.ToPathFromUri() is { } tone_path &&
               tone_path.IsFile()) {
                config.BrowserExecutableFolder = Path.GetDirectoryName(tone_path);
                Debug.WriteLine($"Browser executable folder: {config.BrowserExecutableFolder}");
            }
        }

        public virtual bool ConfigureWebView(WebView wv) {
            return true;
        }
    }
}