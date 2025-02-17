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
                    PlatformWrapper.Init(new PlatformServices_browser());

                })
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() {

#if DEBUG
        // PlatformWrapper.Services.Logger.WriteLine("Waiting for debugger");
        // while(!Debugger.IsAttached) {
        //     Thread.Sleep(100);
        // }
        //
        // PlatformWrapper.Services.Logger.WriteLine("Debugger attached");
#endif

        return AppBuilder
            .Configure<App>();
    }
}