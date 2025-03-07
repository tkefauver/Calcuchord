using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SkiaSharp;
using Svg.Skia;

namespace Calcuchord.Browser {
    public class Share_browser : Share_default,IShareHtml {
        public override void ShareHtml(string html,string title) {
            string fileName = title + ".html";

            string b64 = html.ToBase64String();
            JsInterop.ShareFile(b64,"text/html",fileName,title);
        }

        public override async Task ShareMidiAsync(IEnumerable<IEnumerable<int>> toneSets,bool isScale,string title) {
            string tempFile = "temp.mid";
            string fileName = title + ".mid";
            if(isScale) {
                await Builder.CreateMidiScaleAsync(toneSets,tempFile);
            } else {
                await Builder.CreateMidiChordAsync(toneSets,tempFile);
            }

            string b64 = File.ReadAllBytes(tempFile).ToBase64String();
            JsInterop.ShareFile(b64,"audio/midi",fileName,title);
            File.Delete(tempFile);
        }

        public override async Task SharePdfAsync(SKSvg svg,string title) {
            await Task.Delay(1);
            string tempFile = "temp.pdf";
            string fileName = title + ".pdf";
            svg.Picture.ToPdf(tempFile,ThemeViewModel.Instance.IsDark ? SKColors.Black : SKColors.White,1f,1f);

            string b64 = File.ReadAllBytes(tempFile).ToBase64String();
            JsInterop.ShareFile(b64,"application/pdf",fileName,title);
            File.Delete(tempFile);
        }
    }
}