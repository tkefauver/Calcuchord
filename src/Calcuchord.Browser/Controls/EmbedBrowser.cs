using System;
using System.Runtime.InteropServices.JavaScript;
using Avalonia.Browser;
using Avalonia.Platform;

namespace Calcuchord.Browser {
    public interface INativeDemoControl {
        /// <param name="isSecond">Used to specify which control should be displayed as a demo</param>
        /// <param name="parent"></param>
        /// <param name="createDefault"></param>
        IPlatformHandle CreateControl(bool isSecond,IPlatformHandle parent,Func<IPlatformHandle> createDefault);
    }

    public class EmbedBrowserHandle : INativeDemoControl {
        public IPlatformHandle CreateControl(bool isSecond,IPlatformHandle parent,Func<IPlatformHandle> createDefault) {
            JSObject iframe = EmbedInterop.CreateElement("iframe");

            iframe.SetProperty(
                "src",
                "https://raw.githubusercontent.com/tkefauver/Calcuchord/master/src/Calcuchord/Assets/Text/tone.html");

            return new JSObjectControlHandle(iframe);
        }
    }


    public static partial class EmbedInterop {
        [JSImport("globalThis.document.createElement")]
        public static partial JSObject CreateElement(string tagName);

        [JSImport("addAppButton","embed.js")]
        public static partial void AddAppButton(JSObject parentObject);
    }
}