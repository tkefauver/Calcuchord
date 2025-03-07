namespace Calcuchord.Browser {
    public class UriNavigator_browser : IUriNavigator {

        public void NavigateTo(string uri,object args) {
            JsInterop.OpenLink(uri);
        }
    }
}