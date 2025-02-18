using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using Avalonia.WebView.Android;
using Xamarin.Essentials;

namespace Calcuchord.Android {
    [Activity(
        Label = "Calcuchord",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App> {
        public override void OnCreate(Bundle savedInstanceState,PersistableBundle persistentState) {
            base.OnCreate(savedInstanceState,persistentState);
            Platform.Init(this,savedInstanceState);
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            return base.CustomizeAppBuilder(builder)
                .UseReactiveUI()
                .UseAndroidWebView()
                .WithInterFont()
                .LogToTrace()
                .AfterPlatformServicesSetup(
                    _ => {
                        AppDomain.CurrentDomain.UnhandledException += (s,e) => {
                            PlatformWrapper.Services.Logger.WriteLine(
                                $"AppDomain.CurrentDomain.UnhandledException: {e.ExceptionObject}. IsTerminating: {e.IsTerminating}");
                        };

                        AndroidEnvironment.UnhandledExceptionRaiser += (s,e) => {
                            PlatformWrapper.Services.Logger.WriteLine(
                                $"AndroidEnvironment.UnhandledExceptionRaiser: {e.Exception}. IsTerminating: {e.Handled}");
                            e.Handled = true;
                        };

                        PlatformWrapper.Init(new PlatformServices_ad(this));
                    });
        }
    }
}