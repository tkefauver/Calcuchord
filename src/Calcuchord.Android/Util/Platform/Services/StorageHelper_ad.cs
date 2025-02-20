using System.Diagnostics.CodeAnalysis;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

namespace Calcuchord.Android {
    public class StorageHelper_ad : StorageHelper_default {
        readonly Context _context;

        public StorageHelper_ad(Context context) {
            _context = context;
        }

        [SuppressMessage("Interoperability","CA1416:Validate platform compatibility",Justification = "<Pending>")]
        public override bool IsExternalWriteEnabled() {
            if(_context is not Activity mac) {
                return false;
            }
            // from https://stackoverflow.com/a/33162451/105028

            if(Build.VERSION.SdkInt >= BuildVersionCodes.M) {

                if(_context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted) {
                    return true;
                }

                return false;
            }

            return true;
        }

        public override void RequestExternalWritePermission() {
            if(_context is not Activity mac) {
                return;
            }

            ActivityCompat.RequestPermissions(mac,[Manifest.Permission.ReadExternalStorage],1);
            if(IsExternalWriteEnabled()) {
                TriggerSaveEnabled();
            }
        }
    }
}