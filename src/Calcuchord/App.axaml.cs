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
            if(PlatformWrapper.Services is { } ps &&
               ps.MidiPlayer is MidiPlayer_sugarwv mp_swv) {
                AvaloniaWebViewBuilder.Initialize(
                    config => {
                        mp_swv.Init(config);
                    });
            }
        }


        public override void OnFrameworkInitializationCompleted() {
            AssetMover.MoveAllAssets();

            Prefs.Init();
            ThemeViewModel.Instance.Init();

            _ = new MainViewModel();

            switch(ApplicationLifetime) {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = MainViewModel.Instance
                    };
                    break;
                case ISingleViewApplicationLifetime singleViewPlatform:
                    singleViewPlatform.MainView = new MainView
                    {
                        DataContext = MainViewModel.Instance
                    };
                    break;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}