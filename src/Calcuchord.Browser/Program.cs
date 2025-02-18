using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Calcuchord;
using Calcuchord.Browser;

internal sealed partial class Program {
    static Task Main(string[] args) {

#if DEBUG
        Console.WriteLine("waiting for debugger");
        //JsInterop.ConsoleLog("waiting for debugger");
        //PlatformWrapper.Services.Logger.WriteLine("Waiting for debugger");
        while(!Debugger.IsAttached) {
            Thread.Sleep(100);
        }

        Console.WriteLine("Debugger attached");
        //JsInterop.ConsoleLog("Debugger attached");

        //PlatformWrapper.Services.Logger.WriteLine("Debugger attached");
#endif
        return BuildAvaloniaApp()
            .WithInterFont()
            .AfterPlatformServicesSetup(
                _ => {
                    PlatformWrapper.Init(new PlatformServices_browser());

                })
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() {


        return AppBuilder
            .Configure<App>();
    }
}