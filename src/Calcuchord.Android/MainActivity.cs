using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using Avalonia.WebView.Android;

namespace Calcuchord.Android {
    [Activity(
        Label = "Calcuchord",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App> {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            PlatformWrapper.Init(() => {
                PlatformWrapper.StorageHelper = new DefaultStorageHelper();
                PlatformWrapper.WebViewHelper = new AndroidWebViewHelper();
            });
            return base.CustomizeAppBuilder(builder)
                .UseReactiveUI()
                .UseAndroidWebView()
                .WithInterFont()
                .LogToTrace()
                .AfterSetup(
                    _ => {
                        AssetMover.MoveAllAssets();
                    });
        }
    }
}