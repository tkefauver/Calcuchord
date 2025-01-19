using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
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
                    PlatformWrapper.WebViewHelper.InitEnv(config);
                });
        }


        public override void OnFrameworkInitializationCompleted() {
            Prefs.Init();


            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainViewModel()
                };
            }
            else if(ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                singleViewPlatform.MainView = new MainView {
                    DataContext = new MainViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        void DisableAvaloniaDataAnnotationValidation() {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach(var plugin in dataValidationPluginsToRemove) {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}