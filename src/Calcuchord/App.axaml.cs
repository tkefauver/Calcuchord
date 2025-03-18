using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }


        public override void OnFrameworkInitializationCompleted() {
            AssetMover.MoveAllAssets();
            Prefs.InitAsync().FireAndForgetSafeAsync();
            ThemeViewModel.Instance.Init();
            _ = new MainViewModel();

            switch(ApplicationLifetime) {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = MainViewModel.Instance,
                    };
#if DEBUG
                    this.AttachDevTools();
#endif
                    break;
                case ISingleViewApplicationLifetime singleViewPlatform:

                    singleViewPlatform.MainView = new MainView
                    {
                        DataContext = MainViewModel.Instance,
                    };
                    break;
            }

            base.OnFrameworkInitializationCompleted();
        }

        void NativeMenuItem_OnClick(object sender,EventArgs e) {
            MainViewModel.Instance.ShowAboutCommand.Execute("NATIVEMENU");
        }
    }
}