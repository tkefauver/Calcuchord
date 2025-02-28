using System;
using Foundation;
//using Microsoft.Maui.ApplicationModel;
using MonkeyPaste.Common;
using UIKit;
//using Xamarin.Essentials;

namespace Calcuchord.iOS {
    public class UriNav_ios : IUriNavigator {

        public void NavigateTo(string uri) {
            //Browser.OpenAsync(new Uri(uri)).FireAndForgetSafeAsync();
            //Launcher.OpenAsync(new Uri(uri)).FireAndForgetSafeAsync();
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