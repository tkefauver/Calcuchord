using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Calcuchord;

internal sealed partial class Program {
    static Task Main(string[] args) {
        return BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() {

        PlatformWrapper.Init(() => {
            PlatformWrapper.StorageHelper = new BrowserStorageHelper();
            PlatformWrapper.WebViewHelper = new BrowserWebViewHelper();
        });
        return AppBuilder.Configure<App>();
    }
}