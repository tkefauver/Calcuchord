using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaWebView;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void RegisterServices() {
            base.RegisterServices();
            AvaloniaWebViewBuilder.Initialize(
                config => {
                    PlatformWrapper.Load();
                    PlatformWrapper.WebViewHelper.InitEnv(config);
                });
        }


        public override void OnFrameworkInitializationCompleted() {
            Prefs.Instance.Init();
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