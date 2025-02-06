using Android.Webkit;
using Avalonia.WebView.Android.Core;
using AvWebView = AvaloniaWebView.WebView;
using WkWebView = Android.Webkit.WebView;

namespace Calcuchord.Android {
    public class MidiPlayer_ad_sugarwv : MidiPlayer_sugarwv {

        protected override void ConfigurePlatformWebView(AvWebView wv) {
            if(wv.PlatformWebView is not AndroidWebViewCore wvc) {
                return;
            }

            WkWebView wkwv = wvc.WebView;

            wkwv.Settings.AllowFileAccessFromFileURLs = true;
            wkwv.Settings.AllowUniversalAccessFromFileURLs = true;
            wkwv.Settings.AllowFileAccess = true;
            wkwv.Settings.AllowContentAccess = true;

            wkwv.Settings.DomStorageEnabled = true;
            wkwv.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
            wkwv.SetWebChromeClient(new WebChromeClient());
        }
    }
}