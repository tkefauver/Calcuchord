using System;
using System.Diagnostics;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class UriNavigator_default : IUriNavigator {

        public virtual void NavigateTo(string url,object args) {
            if(OperatingSystem.IsWindows()) {
                if(url.IsDirectory()) {
                    Process.Start("explorer",url);
                    return;
                }

                Process.Start(new ProcessStartInfo { FileName = url,UseShellExecute = true });
                return;
            }

            if(OperatingSystem.IsLinux()) {
                if(args is string fp) {
                    Process.Start(
                        "dbus-send",
                        $"--session --print-reply --dest=org.freedesktop.FileManager1 --type=method_call /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"file://{fp}\" string:\"\"");
                    return;
                } 
                if(args is string[] fpl) {
                    Process.Start(
                        "dbus-send",
                        $"dbus-send --session --print-reply --dest=org.freedesktop.FileManager1 --type=method_call /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:{string.Join(",",fpl.Select(x => $"\"{x.ToFileSystemUriFromPath()}\""))} string:\"\"");
                    return;
                } 
                
                Process.Start("xdg-open",url);
            }

            if(OperatingSystem.IsMacOS()) {
                if(args is string fp) {
                    Process.Start(
                        "open",
                        $"-R {fp}");
                    return;
                } 
                if(args is string[] fpl) {
                    Process.Start(
                        "open",
                        $"-R {fpl.Last()}");
                    return;
                }
                Process.Start("open",url);
            }
        }
    }
}