using System;
using WebViewCore;
using WebViewCore.Configurations;
using AvWebView = AvaloniaWebView.WebView;

namespace Calcuchord.Desktop {

    public class DesktopWebViewHelper : WebViewHelperBase {
        public override void InitEnv(WebViewCreationProperties config) {
            base.InitEnv(config);
            if(OperatingSystem.IsWindows()) {
                Environment.SetEnvironmentVariable(
                    "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
                    "--autoplay-policy=no-user-gesture-required");
            }
        }

        public override bool ConfigureWebView(AvWebView wv) {
            if(OperatingSystem.IsLinux()) {
                IPlatformWebView test = wv.PlatformWebView;
                // if(wv.PlatformWebView is not WebKitSharp. wvc ||
                //    wvc.WebView is not { } wkwv) {
                //     return false;
                // }
            }

            return true;
        }
    }
}