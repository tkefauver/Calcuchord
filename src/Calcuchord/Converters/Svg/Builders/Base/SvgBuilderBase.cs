using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public abstract class SvgBuilderBase {
        protected string[] FingerFg { get; }
        protected string[] FingerBg { get; }

        protected string RootBg =>
            ColorPalette.Instance.P[PaletteColorType.RootFretBg];

        protected string RootFg =>
            ColorPalette.Instance.P[PaletteColorType.RootFretFg];

        protected string UserBg =>
            ColorPalette.Instance.P[PaletteColorType.UserFretBg];

        protected string UserFg =>
            ColorPalette.Instance.P[PaletteColorType.UserFretFg];

        public abstract HtmlNode Build(NoteGroup ng);

        protected SvgBuilderBase() {
            PaletteColorType[] fbg = [
                PaletteColorType.Bg,
                PaletteColorType.Finger1Bg,
                PaletteColorType.Finger2Bg,
                PaletteColorType.Finger3Bg,
                PaletteColorType.Finger4Bg
            ];
            FingerBg = fbg.Select(x => ColorPalette.Instance.P[x]).ToArray();
            PaletteColorType[] ffg = [
                PaletteColorType.Fg,
                PaletteColorType.Finger1Fg,
                PaletteColorType.Finger2Fg,
                PaletteColorType.Finger3Fg,
                PaletteColorType.Finger4Fg
            ];
            FingerFg = ffg.Select(x => ColorPalette.Instance.P[x]).ToArray();
        }


        protected void AddText(HtmlDocument doc,HtmlNode cntr,string text,double fs,string ff,string fill,double x,
            double y) {
            HtmlNode text_elm = doc.CreateElement("text");
            text_elm.Attributes.Add("font-size",fs);
            text_elm.Attributes.Add("font-family",ff);
            text_elm.Attributes.Add("fill",fill);
            text_elm.Attributes.Add("x",x);
            text_elm.Attributes.Add("y",y);
            text_elm.InnerHtml = text;
            cntr.AppendChild(text_elm);
        }

        protected void AddCircle(HtmlDocument doc,HtmlNode cntr,string fill,double x,double y,double r) {
            HtmlNode circle = doc.CreateElement("circle");
            circle.Attributes.Add("fill",fill);
            circle.Attributes.Add("r",r);
            circle.Attributes.Add("cx",x);
            circle.Attributes.Add("cy",y);
            cntr.AppendChild(circle);
        }

        protected void AddRect(HtmlDocument doc,HtmlNode cntr,string fill,string stroke,double x,double y,double w,
            double h,double sw) {
            HtmlNode rect = doc.CreateElement("rect");
            rect.Attributes.Add("stroke",stroke);
            rect.Attributes.Add("fill",fill);
            rect.Attributes.Add("stroke-width",sw);
            rect.Attributes.Add("width",w);
            rect.Attributes.Add("height",h);
            rect.Attributes.Add("x",x);
            rect.Attributes.Add("y",y);
            cntr.AppendChild(rect);
        }
    }
}