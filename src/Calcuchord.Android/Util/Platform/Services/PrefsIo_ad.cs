using System.Diagnostics.CodeAnalysis;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using Debug = System.Diagnostics.Debug;

namespace Calcuchord.Android {
    public class PrefsIo_ad : PrefsIo_default {
        readonly Context _context;

        public PrefsIo_ad(Context context) {
            _context = context;
        }

        [SuppressMessage("Interoperability","CA1416:Validate platform compatibility",Justification = "<Pending>")]
        bool DoPermissionCheck() {
            return true;

            if(_context is not Activity mac) {
                return false;
            }
            // from https://stackoverflow.com/a/33162451/105028

            if(Build.VERSION.SdkInt >= BuildVersionCodes.M) {

                if(_context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted) {
                    return true;
                }

                ActivityCompat.RequestPermissions(mac,[Manifest.Permission.ReadExternalStorage],1);
                return false;
            }

            return true;
        }

        public override void WritePrefs(string prefsJson) {
            if(DoPermissionCheck()) {
                base.WritePrefs(prefsJson);
                return;
            }

            PlatformWrapper.Services.Logger.WriteLine($"Prefs Error, cannot write to '{PrefsFilePath}'");

        }
    }
}