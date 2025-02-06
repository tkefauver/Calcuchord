﻿using System;
using Avalonia;
using Avalonia.ReactiveUI;

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

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
#if SUGAR_WV
                .UseDesktopWebView()
#endif
                .With(
                    new X11PlatformOptions
                    {
                        // EnableMultiTouch = true,
                        UseDBusMenu = false,
                        EnableIme = false
                    })
                .WithInterFont()
                .LogToTrace()
                .AfterPlatformServicesSetup(
                    _ => {
                        PlatformWrapper.Init(new DesktopPlatformServices());
                    });
        }
    }
}