using AvaloniaWebView;
using WebViewCore.Configurations;

public interface IWebViewHelper {
    void InitEnv(WebViewCreationProperties config);
    bool ConfigureWebView(WebView wv);
}