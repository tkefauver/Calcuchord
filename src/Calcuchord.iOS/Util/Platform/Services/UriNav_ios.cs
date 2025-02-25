namespace Calcuchord.iOS {
    public class UriNav_ios : IUriNavigator {

        public void NavigateTo(string uri) {
            Launcher.OpenAsync(new Uri(uri));
        }
    }
}