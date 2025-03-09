using System;
using Foundation;
using UIKit;

namespace Calcuchord.iOS {
    public class UriNav_ios : IUriNavigator {

        public void NavigateTo(string uri, object args = null) {
            try
            {
                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(uri)!,new NSDictionary(),null);
            }
            catch (Exception ex)
            {
                ex.Dump();
            }
        }
    }
}