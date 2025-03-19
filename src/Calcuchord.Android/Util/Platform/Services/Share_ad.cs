using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using AndroidX.Core.Content;
using File = Java.IO.File;

namespace Calcuchord.Android {
    public class Share_ad : Share_default {
        readonly Context _context;

        public Share_ad(Context context) {
            _context = context;
        }

        protected override void FinishShare(string filePath,string mimeType) {
            // from https://stackoverflow.com/a/68738678/105028
            //Intent intent = new Intent(Intent.ActionSend);
            // intent.PutExtra(Intent.ExtraStream,Uri.Parse(filePath.ToFileSystemUriFromPath()));
            // intent.SetType(mimeType);
            // _context.StartActivity(intent);
            //intent.SetDataAndType(Uri,"application/vnd.android.package-archive");

            Intent intent = new Intent(Intent.ActionSend);
            Uri uri = FileProvider.GetUriForFile(_context,_context.PackageName + ".fileprovider",new File(filePath));
            intent.SetType(mimeType);
            intent.PutExtra(Intent.ExtraStream,uri);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            _context.StartActivity(Intent.CreateChooser(intent,"Share Time!"));

            // Intent intent = new Intent(Intent.ActionView);
            // Uri Uri = FileProvider.GetUriForFile(_context,_context.PackageName + ".fileprovider",new File(filePath));
            // intent.SetDataAndType(Uri,mimeType);
            // intent.SetFlags(ActivityFlags.NewTask);

            // _context.StartActivity(intent);
        }

        protected override async Task<string> ShowSaveFilePickerAsync(string title,string[] extTypes, string[] mimeTypes, string fileTypeName) {
            await Task.Delay(1);
            string fileName = $"{title}.{extTypes[0]}";
            return Path.Combine(PlatformWrapper.Services.StorageHelper.ShareDir,fileName);
        }
    }
}