using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

namespace Calcuchord.Android {
    [Activity(
        Label = AppInfo.Name,
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App> {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            return base.CustomizeAppBuilder(builder)
                .UseReactiveUI()
                .WithInterFont()
                .LogToTrace()
                .AfterPlatformServicesSetup(
                    _ => {
                        AppDomain.CurrentDomain.UnhandledException += (_,e) => {
                            PlatformWrapper.Services.Logger.WriteLine(
                                $"AppDomain.CurrentDomain.UnhandledException: {e.ExceptionObject}. IsTerminating: {e.IsTerminating}");
                        };

                        AndroidEnvironment.UnhandledExceptionRaiser += (_,e) => {
                            PlatformWrapper.Services.Logger.WriteLine(
                                $"AndroidEnvironment.UnhandledExceptionRaiser: {e.Exception}. IsTerminating: {e.Handled}");
                            e.Handled = true;
                        };

                        PlatformWrapper.Init(new PlatformServices_ad(this));
                    });
        }
    }
}