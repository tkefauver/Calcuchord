using Avalonia;
using Avalonia.iOS;
using Foundation;

namespace Calcuchord.iOS {
    [Register("AppDelegate")]
    #pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public partial class AppDelegate : AvaloniaAppDelegate<App>
    #pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            return base.CustomizeAppBuilder(builder)
                .WithInterFont()
                .With(new iOSPlatformOptions() {})
                .AfterPlatformServicesSetup(
                    _ => {
                        PlatformWrapper.Init(new PlatformServices_ios(this));
                    });
        }
    }
}