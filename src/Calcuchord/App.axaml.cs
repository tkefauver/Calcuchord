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
            if(PlatformWrapper.Services is { } ps &&
               ps.MidiPlayer is MidiPlayer_sugarwv mp_swv) {
                AvaloniaWebViewBuilder.Initialize(
                    config => {
                        mp_swv.Init(config);
                    });
            }
#endif
        }


        public override void OnFrameworkInitializationCompleted() {
            AssetMover.MoveAllAssets();

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