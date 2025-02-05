using Avalonia.Controls;
using Avalonia.Platform;

namespace Calcuchord.Browser {
    public class BrowserWebViewHelper : WebViewHelperBase {
        public override bool IsSupported => true;

        public override void InitEnv(object configObj) {
            MidiPlayer.BrowserMidiPlayer = new BrowserMidiPlayer();
        }

        public override bool ConfigureWebView(object wv) {
            // if(wv is not Grid grid) {
            //     return false;
            // }
            //
            // EmbedWebView embed = new EmbedWebView();
            // //grid.Children.Insert(0,embed);
            // Grid.SetRowSpan(embed,2);
            // embed.Width = 500;
            // embed.Height = 500;
            // grid.Children.Add(embed);
            return true;
        }

    }

    public class EmbedWebView : NativeControlHost {
        public static INativeDemoControl Implementation { get; set; }

        public bool IsSecond { get; set; }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            return Implementation?.CreateControl(IsSecond,parent,() => base.CreateNativeControlCore(parent)) ??
                   base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            base.DestroyNativeControlCore(control);
        }
    }
}