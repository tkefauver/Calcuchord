using System;
using System.IO;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace Calcuchord.Browser {
    public class Share_browser : Share_default {

        protected override void FinishShare(string filePath,string mimeType) {
            Task.Run(
                async () => {
                    try {
                        byte[] bytes = await File.ReadAllBytesAsync(filePath);
                        await JsInterop.ShareFileAsync(bytes.ToBase64String(),mimeType,Path.GetFileName(filePath));
                        File.Delete(filePath);
                    } catch(Exception ex) {
                        ex.Dump();
                    }
                });
        }

        protected override async Task<string> ShowSaveFilePickerAsync(string title,string[] extTypes) {
            await Task.Delay(1);
            string fileName = $"{title}.{extTypes[0]}";
            return Path.Combine(PlatformWrapper.Services.StorageHelper.ShareDir,fileName);
        }
    }
}