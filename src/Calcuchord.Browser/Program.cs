using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Calcuchord;
using Calcuchord.Browser;

internal sealed partial class Program {
    static Task Main(string[] args) {
        return BuildAvaloniaApp()
            .WithInterFont()
            .AfterPlatformServicesSetup(
                _ => {
                    EmbedWebView.Implementation = new EmbedBrowserHandle();
                    PlatformWrapper.Init(
                        () => {
                            PlatformWrapper.StorageHelper = new BrowserStorageHelper();
                            PlatformWrapper.WebViewHelper = new BrowserWebViewHelper();
                        });
                })
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() {


        return AppBuilder
            .Configure<App>()
            .AfterSetup(
                _ => {
                    AssetMover.MoveAllAssets();
                });
    }
}