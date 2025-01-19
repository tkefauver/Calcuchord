using System;
using WebViewCore.Configurations;

namespace Calcuchord.Desktop {
    public class DesktopWebViewHelper : WebViewHelperBase {
        public override void InitEnv(WebViewCreationProperties config) {
            base.InitEnv(config);
            Environment.SetEnvironmentVariable(
                "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
                "--autoplay-policy=no-user-gesture-required");
        }
    }
}