using Android.Content;
using Android.Net;

namespace Calcuchord.Android {
    public class UriNav_ad : IUriNavigator {
        readonly Context _context;

        public UriNav_ad(Context context) {
            _context = context;
        }

        public void NavigateTo(string uri,object args) {
            //Launcher.OpenAsync(new Uri(uri));

            // from https://stackoverflow.com/a/69905385/105028
            // Intent chooser = Intent.CreateChooser(intent, "Choose Your Browser");
            // if (intent.ResolveActivity(_context.PackageManager) != null) {
            //     _context.StartActivity(chooser);
            // }

            Intent intent = new Intent(Intent.ActionView,Uri.Parse(uri));
            intent.SetFlags(ActivityFlags.NewTask);
            if(intent.ResolveActivity(_context.PackageManager) != null) {
                _context.StartActivity(intent);
            }

        }
    }
}