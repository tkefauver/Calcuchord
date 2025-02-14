using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using Avalonia.WebView.Android;
using Debug = System.Diagnostics.Debug;

namespace Calcuchord.Android {
    [Activity(
        Label = "Calcuchord",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App> {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            return base.CustomizeAppBuilder(builder)
                .UseReactiveUI()
                .UseAndroidWebView()
                .WithInterFont()
                .LogToTrace()
                .AfterPlatformServicesSetup(
                    _ => {
                        AppDomain.CurrentDomain.UnhandledException += (s,e) => {
                            Debug.WriteLine(
                                $"AppDomain.CurrentDomain.UnhandledException: {e.ExceptionObject}. IsTerminating: {e.IsTerminating}");
                        };

                        AndroidEnvironment.UnhandledExceptionRaiser += (s,e) => {
                            Debug.WriteLine(
                                $"AndroidEnvironment.UnhandledExceptionRaiser: {e.Exception}. IsTerminating: {e.Handled}");
                            e.Handled = true;
                        };

                        PlatformWrapper.Init(new AndroidPlatformServices(this));
                    });
        }
    }
}