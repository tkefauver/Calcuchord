using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.WebView.Desktop;

namespace Calcuchord.Desktop {
    internal sealed class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() {
            PlatformWrapper.Init(() => {
                PlatformWrapper.StorageHelper = new DesktopStorageHelper();
                PlatformWrapper.WebViewHelper = new DesktopWebViewHelper();
            });


            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .UseDesktopWebView()
                .WithInterFont()
                .LogToTrace()
                .AfterSetup(
                    _ => {
                        AssetMover.MoveAllAssets();
                    });
        }
    }
}