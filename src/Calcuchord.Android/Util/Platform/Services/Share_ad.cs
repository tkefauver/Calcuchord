using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;

namespace Calcuchord.Android {
    public class Share_ad : Share_default {
        readonly Context _context;

        public Share_ad(Context context) {
            _context = context;
        }

        protected override void FinishShare(string filePath,string mimeType) {
            // from https://stackoverflow.com/a/68738678/105028
            Intent shareToneIntent = new Intent(Intent.ActionSend);
            shareToneIntent.PutExtra(Intent.ExtraStream,Uri.Parse(filePath));
            shareToneIntent.SetType(mimeType);
            _context.StartActivity(shareToneIntent);
        }

        protected override async Task<string> ShowSaveFilePickerAsync(string title,string[] extTypes) {
            await Task.Delay(1);
            string fileName = $"{title}.{extTypes[0]}";
            return Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,fileName);
        }
    }
}