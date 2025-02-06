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
                    PlatformWrapper.Init(new BrowserPlatformServices());
                })
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() {


        return AppBuilder
            .Configure<App>();
    }
}