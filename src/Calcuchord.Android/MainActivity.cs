using System;
using System.Diagnostics.CodeAnalysis;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
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