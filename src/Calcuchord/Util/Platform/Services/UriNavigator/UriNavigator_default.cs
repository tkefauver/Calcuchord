using System;
using System.Diagnostics;

namespace Calcuchord {
    public class UriNavigator_default : IUriNavigator {

        public virtual void NavigateTo(string url) {
            if(OperatingSystem.IsWindows()) {
                //Process.Start(new ProcessStartInfo("cmd",$"/c start {url}") {UseShellExecute = true});
                Process.Start(new ProcessStartInfo { FileName = url,UseShellExecute = true });
                return;
            }

            if(OperatingSystem.IsLinux()) {
                Process.Start("xdg-open",url);
                return;
            }

            if(OperatingSystem.IsMacOS()) {
                Process.Start("open",url);
            }
        }
    }
}