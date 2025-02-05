using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PropertyChanged;
#if SUGAR_WV
using AvaloniaWebView;
#endif

namespace Calcuchord {
    [DoNotNotify]
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void RegisterServices() {
            base.RegisterServices();
#if SUGAR_WV
            AvaloniaWebViewBuilder.Initialize(
                config => {
                    PlatformWrapper.Load();
                    PlatformWrapper.WebViewHelper.InitEnv(config);
                });
#endif
        }


        public override void OnFrameworkInitializationCompleted() {
#if BROWSER
            PlatformWrapper.Load();
            PlatformWrapper.WebViewHelper.InitEnv(null);
#elif LINUX
            PlatformWrapper.Load();
            AssetMover.MoveAllAssets();
#endif
            Prefs.Init();
            ThemeViewModel.Instance.Init();

            _ = new MainViewModel();

            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = MainViewModel.Instance
                };
            } else if(ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = MainViewModel.Instance
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}