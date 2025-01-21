using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public abstract class SvgBuilderBase {
        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        public static SvgFlags DefaultSvgFlags =>
            SvgFlags.Fingers |
            SvgFlags.Colors |
            SvgFlags.Tuning |
            SvgFlags.Roots |
            SvgFlags.Matches |
            SvgFlags.Frets;

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        protected HtmlDocument CurrentDoc { get; private set; }


        protected string[] FingerFg { get; }
        protected string[] FingerBg { get; }

        protected string BarShadow =>
            "#00000050";

        protected string RootBg =>
            ColorPalette.Instance.P[PaletteColorType.RootFretBg];

        protected string RootFg =>
            ColorPalette.Instance.P[PaletteColorType.RootFretFg];

        protected string UserBg =>
            ColorPalette.Instance.P[PaletteColorType.UserFretBg];

        protected string UserFg =>
            ColorPalette.Instance.P[PaletteColorType.UserFretFg];

        protected string Fg =>
            ColorPalette.Instance.P[PaletteColorType.Fg];

        protected string Bg =>
            ColorPalette.Instance.P[PaletteColorType.Bg];

        protected string Transparent => "transparent";

        protected double FretLineFixedAxisSize => 0.25;
        protected double NutFixedAxisSize => 1;

        protected double FretWidth => 10;
        protected double FretHeight => 12;

        protected double DotRadius => 4;
        protected double DotStrokeWidth => 0.775;

        protected double BarHeight => 9;
        protected double BarOffsetY => 1.5;

        protected double BodyFontSize => 4;
        protected double HeaderFontSize => 6;
        protected string DefaultFontFamily => "Verdana";

        protected int VisibleFretCount => 4;

        #endregion

        #region Events

        #endregion

        #region Constructors

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

        #endregion

        #region Public Methods

        public abstract HtmlNode Build(NoteGroup ng);

        #endregion

        #region Protected Methods

        protected HtmlNode InitBuild() {
            CurrentDoc = new();
            HtmlNode svg = CurrentDoc.CreateElement("svg");
            svg.Attributes.Add("xmlns","http://www.w3.org/2000/svg");
            return svg;
        }

        protected void AddText(
            HtmlNode cntr,
            string text,
            double fs,
            string fill,
            double x,
            double y,
            bool isBold = false,
            string classes = null) {
            HtmlNode text_elm = CurrentDoc.CreateElement("text");
            text_elm.Attributes.Add("font-size",fs);
            text_elm.Attributes.Add("font-family",DefaultFontFamily);
            if(isBold) {
                text_elm.Attributes.Add("font-weight","bold");
            }

            if(classes != null) {
                text_elm.Attributes.Add("classes",classes);
            }

            text_elm.Attributes.Add("fill",fill);
            text_elm.Attributes.Add("x",x);
            text_elm.Attributes.Add("y",y);
            text_elm.InnerHtml = text;
            cntr.AppendChild(text_elm);
        }

        protected void AddCircle(
            HtmlNode cntr,
            string fill,
            string stroke,
            double x,
            double y,
            double r,
            double sw,
            string classes = null) {
            HtmlNode circle = CurrentDoc.CreateElement("circle");
            circle.Attributes.Add("fill",fill);
            circle.Attributes.Add("stroke",stroke);
            circle.Attributes.Add("stroke-width",sw);
            circle.Attributes.Add("r",r);
            circle.Attributes.Add("cx",x);
            circle.Attributes.Add("cy",y);
            if(classes != null) {
                circle.Attributes.Add("classes",classes);
            }

            cntr.AppendChild(circle);
        }

        protected void AddRect(
            HtmlNode cntr,
            string fill,
            string stroke,
            double x,
            double y,
            double w,
            double h,
            double sw,
            string classes = null) {
            HtmlNode rect = CurrentDoc.CreateElement("rect");
            rect.Attributes.Add("stroke",stroke);
            rect.Attributes.Add("fill",fill);
            rect.Attributes.Add("stroke-width",sw);
            rect.Attributes.Add("width",w);
            rect.Attributes.Add("height",h);
            rect.Attributes.Add("x",x);
            rect.Attributes.Add("y",y);
            if(classes != null) {
                rect.Attributes.Add("classes",classes);
            }

            cntr.AppendChild(rect);
        }

        protected void AddLine(
            HtmlNode cntr,
            string stroke,
            double x1,
            double y1,
            double x2,
            double y2,
            double sw,
            string classes = null) {
            HtmlNode line = CurrentDoc.CreateElement("line");
            line.Attributes.Add("stroke",stroke);
            line.Attributes.Add("stroke-width",sw);
            line.Attributes.Add("x1",x1);
            line.Attributes.Add("y1",y1);
            line.Attributes.Add("x2",x2);
            line.Attributes.Add("y2",y2);
            if(classes != null) {
                line.Attributes.Add("classes",classes);
            }

            cntr.AppendChild(line);
        }

        #endregion

        #region Private Methods

        #endregion
    }
}