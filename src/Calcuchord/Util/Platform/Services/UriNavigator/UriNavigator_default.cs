using System;
using System.Diagnostics;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class UriNavigator_default : IUriNavigator {

        public virtual void NavigateTo(string url) {
            if(OperatingSystem.IsWindows()) {
                if(url.IsDirectory()) {
                    Process.Start("explorer",url);
                    return;
                }

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