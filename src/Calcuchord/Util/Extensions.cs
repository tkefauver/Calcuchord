using HtmlAgilityPack;

namespace Calcuchord {
    public static class Extensions {
        public static void Add(this HtmlAttributeCollection hac,string key,double val) {
            hac.Add(key,val.ToString());
        }
    }
}