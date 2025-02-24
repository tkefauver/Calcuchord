namespace Calcuchord.Browser {
    public class ShareHtml_browser : IShareHtml {

        public void ShareHtml(string html,string title) {
            JsInterop.ShareHtml(html,title);
        }
    }
}