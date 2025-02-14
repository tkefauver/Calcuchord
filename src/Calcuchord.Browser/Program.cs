using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
                    string root_dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    JsInterop.ConsoleLog($"RootDir: '{root_dir}'");
                    string test_dir = Path.Combine(root_dir,"test");
                    if(Directory.Exists(test_dir)) {
                        JsInterop.ConsoleLog($"TestDir EXISTS '{test_dir}'");
                    } else {
                        Directory.CreateDirectory(test_dir);
                        JsInterop.ConsoleLog($"TestDir CREATED '{test_dir}'");
                    }


                    PlatformWrapper.Init(new BrowserPlatformServices());

                })
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() {

#if DEBUG
        Debug.WriteLine("Waiting for debugger");
        while(!Debugger.IsAttached) {
            Thread.Sleep(100);
        }

        Debug.WriteLine("Debugger attached");
#endif

        return AppBuilder
            .Configure<App>();
    }
}