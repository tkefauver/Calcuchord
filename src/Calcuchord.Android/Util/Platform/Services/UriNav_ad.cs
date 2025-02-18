using System;
using Xamarin.Essentials;

namespace Calcuchord.Android {
    public class UriNav_ad : IUriNavigator {

        public void NavigateTo(string uri) {
            Launcher.OpenAsync(new Uri(uri));
        }
    }
}