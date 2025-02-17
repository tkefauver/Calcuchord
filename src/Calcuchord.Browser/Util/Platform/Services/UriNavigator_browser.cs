namespace Calcuchord.Browser {
    public class UriNavigator_browser : IUriNavigator {

        public void NavigateTo(string uri) {
            JsInterop.OpenLink(uri);
        }
    }
}