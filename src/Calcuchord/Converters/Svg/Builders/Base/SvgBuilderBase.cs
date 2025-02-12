using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using MonkeyPaste.Common;

namespace Calcuchord {
    public abstract class SvgBuilderBase {

        #region Private Variables

        #endregion

        #region Constants

        #endregion

        #region Statics

        public static SvgOptionType DefaultSvgOptionType =>
            SvgOptionType.Fingers |
            SvgOptionType.Barres |
            SvgOptionType.Colors |
            SvgOptionType.Tuning |
            SvgOptionType.Roots |
            SvgOptionType.Matches |
            SvgOptionType.Frets |
            SvgOptionType.Shadows;

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        protected bool ForPrint { get; set; }

        protected HtmlDocument CurrentDoc { get; private set; }
        protected string DefaultFontFamily => "Mono";


        #region Colors

        protected string[] FingerFg { get; }
        protected string[] FingerBg { get; }

        protected string BarShadow =>
            "#000000";

        protected virtual string RootBg =>
            ThemeViewModel.Instance.P[PaletteColorType.RootFretBg];

        protected virtual string RootFg =>
            ThemeViewModel.Instance.P[PaletteColorType.RootFretFg];

        protected virtual string UserBg =>
            ThemeViewModel.Instance.P[PaletteColorType.UserFretBg];

        protected virtual string UserFg =>
            ThemeViewModel.Instance.P[PaletteColorType.UserFretFg];

        protected string Fg =>
            ThemeViewModel.Instance.P[PaletteColorType.Fg];

        protected string Bg =>
            ThemeViewModel.Instance.P[PaletteColorType.Bg];

        protected string Transparent => "transparent";

        #endregion

        #region Measurements

        protected double ShadowOpacity => 0.3;
        protected double FretLineFixedAxisSize => 0.25;
        protected double NutFixedAxisSize => 1;

        protected double FretLength => 10;
        protected double StringFixedAxisLength => 12;

        protected double DotRadius => 4;
        protected double DotStrokeWidth => 0.775;

        protected double BarHeight => DotRadius * 2;

        protected double BodyFontSize => 4;
        protected double HeaderFontSize => 6;

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        protected SvgBuilderBase() {
            PaletteColorType[] fbg =
            [
                PaletteColorType.NutBg,
                PaletteColorType.Finger1Bg,
                PaletteColorType.Finger2Bg,
                PaletteColorType.Finger3Bg,
                PaletteColorType.Finger4Bg
            ];
            FingerBg = fbg.Select(x => ThemeViewModel.Instance.P[x]).ToArray();
            PaletteColorType[] ffg =
            [
                PaletteColorType.NutFg,
                PaletteColorType.Finger1Fg,
                PaletteColorType.Finger2Fg,
                PaletteColorType.Finger3Fg,
                PaletteColorType.Finger4Fg
            ];
            FingerFg = ffg.Select(x => ThemeViewModel.Instance.P[x]).ToArray();

            // using Stream font_stream = AssetLoader.Open(
            //     new("avares://Calcuchord/Assets/Fonts/Garamond/EBGaramond-VariableFont_wght.ttf"));
            // SKSvgSettings settings = new SKSvgSettings();
            // settings.TypefaceProviders?.Add(new CustomTypefaceProvider(font_stream));
        }

        #endregion

        #region Public Methods

        public abstract HtmlNode Build(NoteGroup ng,object args);

        public void Test(Tuning tuning,IEnumerable<NoteGroup> ngl) {
            HtmlDocument doc = new HtmlDocument();

            HtmlNode body = doc.CreateElement("body");
            doc.DocumentNode.AppendChild(body);
            body.Attributes.Add("style","zoom=\"400%\"");
            HtmlNode style = doc.CreateElement("style");
            style.InnerHtml = MainViewModel.Instance.MatchSvgCss;
            body.AppendChild(style);

            void AddSvg(HtmlNode svg,NoteGroup ng) {
                HtmlNode title = doc.CreateElement("span");
                title.InnerHtml = ng.ToString();

                body.AppendChild(title);
                body.AppendChild(doc.CreateElement("br"));
                body.AppendChild(svg);
                body.AppendChild(doc.CreateElement("br"));
            }

            foreach(NoteGroup ng in ngl) {
                AddSvg(Build(ng,null),ng);
            }

            string result = doc.DocumentNode.OuterHtml;
            string fn =
                $"{tuning.ToString().Replace("|","-").Replace(" ","-")}_{GetType().Name.Replace("SvgBuilder",string.Empty).ToLower()}_{ngl.FirstOrDefault().Parent.PatternType}.html";
            string fp = $"/home/tkefauver/Desktop/{fn}";
            MpFileIo.WriteTextToFile(fp,result);

            new Uri(fp.ToFileSystemUriFromPath()).OpenInBrowser();

            // Process.Start(
            //     new ProcessStartInfo
            //     {
            //         UseShellExecute = true,
            //         //WorkingDirectory = Path.GetDirectoryName(fp),
            //         FileName = "xdg-open",
            //         Arguments = fp
            //     });
        }

        #endregion

        #region Protected Methods

        protected bool IsUserNote(InstrumentNote note) {
            if(note is null ||
               MainViewModel.Instance is not { } mvm ||
               !mvm.IsSearchModeSelected ||
               mvm.SelectedTuning is not { } stvm) {
                return false;
            }

            bool is_user_note = stvm.SelectedNotes.Any(x => x.RowNum == note.RowNum && x.NoteNum == note.NoteNum);

            return is_user_note;
        }

        protected HtmlNode InitBuild(object args) {
            CurrentDoc = new();
            HtmlNode svg = CurrentDoc.CreateElement("svg");
            svg.Attributes.Add("xmlns","http://www.w3.org/2000/svg");

            if(args is string arg_str &&
               arg_str == "PDF") {
                ForPrint = true;
            } else {
                ForPrint = false;
            }

            if(args.ToStringOrEmpty() == "styled") {
                HtmlNode style_elm = CurrentDoc.CreateElement("style");
                style_elm.InnerHtml = MainViewModel.Instance.MatchSvgCss;
                svg.AppendChild(style_elm);
            }

            return svg;
        }

        protected HtmlNode CreateG(HtmlNode cntr,string classes = "") {
            HtmlNode g = CurrentDoc.CreateElement("g");
            g.Attributes.Add("class",classes);
            cntr.AppendChild(g);
            return g;
        }

        protected void AddShape(
            HtmlNode cntr,
            bool isBox,
            string fill,
            string stroke,
            double cx,
            double cy,
            double r,
            double sw,
            string classes = null,
            bool shadow = false,
            double fillOpacity = 1) {
            if(isBox) {
                AddDiamond(cntr,fill,stroke,cx - r,cy - r,r * 2d,r * 2d,sw,classes,shadow,fillOpacity);
            } else {
                AddCircle(cntr,fill,stroke,cx,cy,r,sw,classes,shadow,fillOpacity);
            }
        }


        protected void AddCenteredText(
            HtmlNode cntr,
            string text,
            double fs,
            string fill,
            double x,
            double y,
            double w,
            double h,
            bool isBold = false,
            string classes = null,
            bool shadow = false,
            double ox = 0,
            double oy = 0) {
            double tx = (x + (w / 2d)) - ((fs * text.Length) / (text.Length + 2));
            double ty = y + (h / 2d) + (fs / 2d);
            AddText(cntr,text,fs,fill,tx + ox,ty + oy,isBold,classes,shadow);
        }

        protected void AddText(
            HtmlNode cntr,
            string text,
            double fs,
            string fill,
            double x,
            double y,
            bool isBold = false,
            string classes = null,
            bool shadow = false) {
            if(shadow) {
                string shadow_fill = fill == "#FFFFFF" ? "#000000" : "#FFFFFF";
                double offset = 0.25; //fs / 16d;
                AddText(cntr,text,fs,shadow_fill,x + offset,y + offset,isBold,classes + " shadow-elm");
            }

            HtmlNode text_elm = CurrentDoc.CreateElement("text");
            text_elm.Attributes.Add("font-size",fs);
            text_elm.Attributes.Add("font-family",DefaultFontFamily);
            if(isBold) {
                text_elm.Attributes.Add("font-weight","bold");
            }

            if(classes != null) {
                text_elm.Attributes.Add("class",classes);
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
            string classes = null,
            bool shadow = false,
            double fillOpacity = 1) {
            if(shadow) {
                string shadow_fill = fill == Fg ? Bg : "#000000";
                double offset = 0.25; //fs / 16d;
                AddCircle(cntr,shadow_fill,stroke,x + offset,y + offset,r,sw,classes + " shadow-elm");
            }

            HtmlNode circle = CurrentDoc.CreateElement("circle");
            circle.Attributes.Add("fill-opacity",fillOpacity);
            circle.Attributes.Add("fill",fill);
            circle.Attributes.Add("stroke",stroke);
            circle.Attributes.Add("stroke-width",sw);
            circle.Attributes.Add("r",r);
            circle.Attributes.Add("cx",x);
            circle.Attributes.Add("cy",y);
            if(classes != null) {
                circle.Attributes.Add("class",classes);
            }

            cntr.AppendChild(circle);
        }

        protected void AddDiamond(
            HtmlNode cntr,
            string fill,
            string stroke,
            double x,
            double y,
            double w,
            double h,
            double sw,
            string classes = null,
            bool shadow = false,
            double fillOpacity = 1) {
            if(shadow) {
                string shadow_fill = fill == Fg ? Bg : "#000000";
                double offset = 0.25; //fs / 16d;
                AddDiamond(cntr,shadow_fill,stroke,x + offset,y + offset,w,h,sw,classes + " shadow-elm");
            }
            //<polygon points="0 40,40 80,80 40,40 0" style=" fill: blue; stroke:black;"/>

            HtmlNode poly = CurrentDoc.CreateElement("polygon");
            double scale = 1.2;
            double xdiff = (w * scale) - w;
            double ydiff = (h * scale) - h;
            x -= xdiff / 2d;
            y -= ydiff / 2d;
            // w += xdiff;
            // h += ydiff;
            w *= scale;
            h *= scale;

            double hw = w * 0.5;
            double hh = h * 0.5;
            // L
            double x1 = x;
            double y1 = y + hh;

            // T
            double x2 = x + hw;
            double y2 = y;

            // R
            double x3 = x + w;
            double y3 = y + hh;

            // B
            double x4 = x + hw;
            double y4 = y + h;
            poly.Attributes.Add("points",$"{x1} {y1},{x2} {y2},{x3} {y3},{x4} {y4}");
            poly.Attributes.Add("stroke",stroke);
            poly.Attributes.Add("fill",fill);
            poly.Attributes.Add("fill-opacity",fillOpacity);
            poly.Attributes.Add("stroke-width",sw);
            if(classes != null) {
                poly.Attributes.Add("class",classes);
            }

            cntr.AppendChild(poly);
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
            string classes = null,
            bool shadow = false,
            double fillOpacity = 1) {
            if(shadow) {
                string shadow_fill = fill == Fg ? Bg : "#000000";
                double offset = 0.25; //fs / 16d;
                AddRect(cntr,shadow_fill,stroke,x + offset,y + offset,w,h,sw,classes + " shadow-elm");
            }

            HtmlNode rect = CurrentDoc.CreateElement("rect");
            rect.Attributes.Add("stroke",stroke);
            rect.Attributes.Add("fill",fill);
            rect.Attributes.Add("fill-opacity",fillOpacity);
            rect.Attributes.Add("stroke-width",sw);
            rect.Attributes.Add("width",w);
            rect.Attributes.Add("height",h);
            rect.Attributes.Add("x",x);
            rect.Attributes.Add("y",y);
            if(classes != null) {
                rect.Attributes.Add("class",classes);
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
                line.Attributes.Add("class",classes);
            }

            cntr.AppendChild(line);
        }

        #endregion

        #region Private Methods

        #endregion

    }
}