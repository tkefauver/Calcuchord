using System;
using AvaloniaWebView;
using WebViewCore.Configurations;

namespace Calcuchord {
    public interface IWebViewHelper {
        void InitEnv(WebViewCreationProperties config);
        bool ConfigureWebView(WebView wv);
    }

    public interface IProgressIndicator {
        double PercentDone { get; }
        string ProgressLabel { get; }
        event EventHandler ProgressChanged;
    }
}