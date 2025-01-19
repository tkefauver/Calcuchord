using System;
using Android.Webkit;
using Avalonia.WebView.Android.Core;
using WebViewCore.Configurations;
using WebView = AvaloniaWebView.WebView;

namespace Calcuchord.Android {
    public class AndroidWebViewHelper : WebViewHelperBase {
        public override void InitEnv(WebViewCreationProperties config) {
            base.InitEnv(config);
            config.BrowserExecutableFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public override bool ConfigureWebView(WebView wv) {
            if(wv.PlatformWebView is not AndroidWebViewCore wvc ||
               wvc.WebView is not { } wkwv) {
                return false;
            }

            wkwv.Settings.AllowFileAccessFromFileURLs = true;
            wkwv.Settings.AllowUniversalAccessFromFileURLs = true;
            wkwv.Settings.AllowFileAccess = true;
            wkwv.Settings.AllowContentAccess = true;

            wkwv.Settings.DomStorageEnabled = true;
            wkwv.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
            wkwv.SetWebChromeClient(new WebChromeClient());
            return true;
        }
    }
}