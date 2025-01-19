using System.Drawing;
using AvaloniaWebView;
using WebViewCore.Configurations;

namespace Calcuchord {
    public class WebViewHelperBase : IWebViewHelper {
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
        }

        public virtual bool ConfigureWebView(WebView wv) {
            return true;
        }
    }
}