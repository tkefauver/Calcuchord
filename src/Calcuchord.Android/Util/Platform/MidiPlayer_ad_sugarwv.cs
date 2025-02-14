using System;
using System.Threading.Tasks;
using Android.Webkit;
using Avalonia.WebView.Android.Core;
using MonkeyPaste.Common;
using AvWebView = AvaloniaWebView.WebView;
using WkWebView = Android.Webkit.WebView;

namespace Calcuchord.Android {
    public class MidiPlayer_ad_sugarwv : MidiPlayer_sugarwv {

        WkWebView _wkwv;

        protected override void ConfigurePlatformWebView(AvWebView wv) {
            if(wv.PlatformWebView is not AndroidWebViewCore wvc) {
                return;
            }

            WkWebView wkwv = wvc.WebView;


            wkwv.Settings.AllowFileAccessFromFileURLs = true;
            wkwv.Settings.AllowUniversalAccessFromFileURLs = true;
            wkwv.Settings.AllowFileAccess = true;
            wkwv.Settings.AllowContentAccess = true;
            wkwv.Settings.JavaScriptEnabled = true;
            wkwv.Settings.JavaScriptCanOpenWindowsAutomatically = true;

            wkwv.Settings.DomStorageEnabled = true;
            wkwv.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
            wkwv.SetWebChromeClient(new WebChromeClient());

            _wkwv = wkwv;

            //ClearCache();
        }

        void ClearCache() {
            WebStorage.Instance.DeleteAllData();

            // Clear all the cookies
            CookieManager.Instance.RemoveAllCookies(null);
            CookieManager.Instance.Flush();

            _wkwv.ClearCache(true);
            _wkwv.ClearFormData();
            _wkwv.ClearHistory();
            _wkwv.ClearSslPreferences();
        }

        protected override async Task ExecuteScriptAsync(string script) {
            if(_wkwv is null) {
                await base.ExecuteScriptAsync(script);
            } else {
                try {
                    _wkwv.EvaluateJavascript(script,null);
                } catch(Exception e) {
                    e.Dump();
                }
            }

        }

    }
}