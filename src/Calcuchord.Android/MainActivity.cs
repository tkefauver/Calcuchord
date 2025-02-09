using System.Diagnostics.CodeAnalysis;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
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
            return base.CustomizeAppBuilder(builder)
                .UseReactiveUI()
                .UseAndroidWebView()
                .WithInterFont()
                .LogToTrace()
                .AfterPlatformServicesSetup(
                    _ => {
                        PlatformWrapper.Init(new AndroidPlatformServices());
                    });
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            DoPermissionCheck();
        }

        [SuppressMessage("Interoperability","CA1416:Validate platform compatibility",Justification = "<Pending>")]
        public bool DoPermissionCheck() {
            // from https://stackoverflow.com/a/33162451/105028

            if(Build.VERSION.SdkInt >= BuildVersionCodes.M) {

                if(CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted) {
                    return true;
                }

                ActivityCompat.RequestPermissions(this,[Manifest.Permission.ReadExternalStorage],1);
                return false;
            }

            return true;
        }
    }
}