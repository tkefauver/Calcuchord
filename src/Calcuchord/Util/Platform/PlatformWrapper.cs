namespace Calcuchord {
    public static class PlatformWrapper {
        public static IWebViewHelper WebViewHelper { get; private set; }

        public static void Init(IWebViewHelper webViewHelper) {
            WebViewHelper = webViewHelper;
        }
    }
}