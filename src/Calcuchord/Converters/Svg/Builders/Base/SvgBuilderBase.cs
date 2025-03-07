using System;
using System.Collections.Generic;
using System.IO;
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

        public static SvgOptionType[] DefaultSvgOptionType { get; } =
        [
            SvgOptionType.Fingers,
            SvgOptionType.Barres,
            SvgOptionType.Tuning,
            SvgOptionType.Roots,
            SvgOptionType.Matches,
            SvgOptionType.Frets,
            SvgOptionType.Colors,
            SvgOptionType.Shadows,
        ];

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        protected bool WithTitle { get; set; }

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
        protected double DotStrokeWidth => 0.33;

        protected double BarHeight => DotRadius * 2;

        protected double BodyFontSize => 4;
        protected double HeaderFontSize => 6;
        protected double TitleFontSize => 6;

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
                PaletteColorType.Finger4Bg,
            ];
            FingerBg = fbg.Select(x => ThemeViewModel.Instance.P[x]).ToArray();
            PaletteColorType[] ffg =
            [
                PaletteColorType.NutFg,
                PaletteColorType.Finger1Fg,
                PaletteColorType.Finger2Fg,
                PaletteColorType.Finger3Fg,
                PaletteColorType.Finger4Fg,
            ];
            FingerFg = ffg.Select(x => ThemeViewModel.Instance.P[x]).ToArray();

            // using Stream font_stream = AssetLoader.Open(
            //     new("avares://Calcuchord/Assets/Fonts/Garamond/EBGaramond-VariableFont_wght.ttf"));
            // SKSvgSettings settings = new SKSvgSettings();
            // settings.TypefaceProviders?.Add(new CustomTypefaceProvider(font_stream));
        }

        #endregion

        #region Public Methods

        public abstract HtmlNode Build(NotePattern ng,object args);

        public string GetBatchHtml(Tuning tuning,IEnumerable<NotePattern> ngl) {
            HtmlDocument doc = new HtmlDocument();

            HtmlNode head = doc.CreateElement("head");
            doc.DocumentNode.AppendChild(head);
            head.InnerHtml =
                """
                <link href="https://fonts.googleapis.com" rel="preconnect">
                <link crossorigin href="https://fonts.gstatic.com" rel="preconnect">
                <link href="https://fonts.googleapis.com/css2?family=EB+Garamond:ital,wght@0,400..800;1,400..800&family=Nunito:ital,wght@0,200..1000;1,200..1000&display=swap" rel="stylesheet">
                """;
            HtmlNode main_style = doc.CreateElement("style");
            main_style.InnerHtml =
                """
                body {
                   margin: 0;
                   padding: 0;
                   width: 100%;                                        
                   height: 100%;                   
                }
                .container {
                    min-width:100%;
                    min-height:100%;
                    padding: 0;
                    margin: 0;
                    display: grid;
                    grid-template-columns: auto auto auto;
                    gap: 10px;
                }

                p, span {
                    font-family: "EB Garamond", serif;
                    font-optical-sizing: auto;
                    font-weight: 1;
                    font-style: normal;
                }

                p.title {
                    margin: 0;
                    text-align: center;
                    font-size: 24px;
                }

                div.item {
                    display: block;
                    width: 90%;
                    text-align: center;
                    padding: 10px;
                    margin-bottom: 100px;
                }
                svg {
                    transform: scale(2) translate(0px,20px);
                }
                body {
                    background-color: white;
                    color: black;
                }

                body.dark {
                    background-color: black;
                    color: white;
                }

                .footer {
                    width: 100%;
                    font-size: 16px;
                    display: inline-block;
                    text-align: center;
                    margin-top: 30px;
                }

                .close-btn {
                    position: fixed;
                    font-size: 8px;
                    left: 0;
                    top: 0;
                    cursor: pointer;
                }
                """;
            head.AppendChild(main_style);

            HtmlNode body_elm = doc.CreateElement("body");
            doc.DocumentNode.AppendChild(body_elm);
            if(ThemeViewModel.Instance.IsDark) {
                body_elm.SetAttributeValue("class","dark");
            }

            HtmlNode svg_style = doc.CreateElement("style");
            svg_style.InnerHtml = MainViewModel.Instance.MatchSvgCss;
            body_elm.AppendChild(svg_style);

            HtmlNode cont_elm = doc.CreateElement("div");
            cont_elm.SetAttributeValue("class","container");
            body_elm.AppendChild(cont_elm);

            void AddSvg(HtmlNode svg,NotePattern ng) {
                HtmlNode item_elm = doc.CreateElement("div");
                item_elm.SetAttributeValue("class","item");
                cont_elm.AppendChild(item_elm);

                HtmlNode title_elm = doc.CreateElement("p");
                title_elm.SetAttributeValue("class","title");
                title_elm.InnerHtml = ng.ToString();
                item_elm.AppendChild(title_elm);

                item_elm.AppendChild(svg);
            }

            foreach(NotePattern ng in ngl) {
                AddSvg(Build(ng,null),ng);
            }

            HtmlNode footer_elm = doc.CreateElement("p");
            footer_elm.SetAttributeValue("class","footer");
            body_elm.AppendChild(footer_elm);
            HtmlNode footer_label_elm = doc.CreateElement("span");
            footer_label_elm.InnerHtml = "Created With: ";
            footer_elm.AppendChild(footer_label_elm);
            HtmlNode footer_link_elm = doc.CreateElement("a");
            footer_link_elm.SetAttributeValue("href","https://github.com/tkefauver/Calcuchord");
            footer_elm.AppendChild(footer_link_elm);
            HtmlNode footer_link_label_elm = doc.CreateElement("span");
            footer_link_label_elm.InnerHtml = "Calcuchord";
            footer_link_elm.AppendChild(footer_link_label_elm);

            return doc.DocumentNode.OuterHtml;
        }

        public string GetBatchSvg(Tuning tuning,IEnumerable<NotePattern> ngl,int colCount) {
            object args = "styled|titled";
            var sub_svg_elml = ngl.Select(x => Build(x,args)).ToArray();
            colCount = Math.Min(sub_svg_elml.Length,colCount);

            HtmlNode svg_elm = InitBuild(args);
            HtmlDocument doc = CurrentDoc;

            double item_w = sub_svg_elml.First().GetAttributeValue("width",0d);
            double item_h = sub_svg_elml.First().GetAttributeValue("height",0d);

            double tw = item_w * colCount;
            double th = item_h * Math.Ceiling(sub_svg_elml.Length / (double)colCount);

            double title_fs = 12;
            double title_h = title_fs * 1d;
            AddTitleText(svg_elm,tuning.FullName,string.Empty,string.Empty,title_fs,Fg,tw,oy: -title_fs);
            double title_pad = 15;
            double content_y = title_h + title_pad;
            th += title_h + title_pad;

            for(int i = 0; i < sub_svg_elml.Length; i++) {
                HtmlNode sub_svg_elm = sub_svg_elml[i];
                double w = sub_svg_elm.GetAttributeValue("width",0d);
                double h = sub_svg_elm.GetAttributeValue("height",0d);
                int r = i / colCount;
                int c = i % colCount;
                double x = c * w;
                double y = content_y + (r * h);

                HtmlNode wrapper_g = doc.CreateElement("g");
                wrapper_g.SetAttributeValue("transform",$"translate({x},{y}) scale(0.9)");

                HtmlNode cntr_g = sub_svg_elm.FirstChild.NextSibling;
                cntr_g.Remove();
                wrapper_g.AppendChild(cntr_g);
                svg_elm.AppendChild(wrapper_g);
            }

            double logo_fs = 3;
            double logo_h = logo_fs * 1.5;
            double logo_y = th;
            AddTitleText(
                svg_elm,$"Created with Calcuchord ©{DateTime.Now.Year}",string.Empty,string.Empty,logo_fs,Fg,tw,oy: th);
            th += logo_h;

            logo_y += logo_h;
            double ox = 0; //800d / tw;
            AddTitleText(
                svg_elm,"https://tkefauver.github.io/Calcuchord",string.Empty,string.Empty,logo_fs,Fg,tw,ox: ox,oy: th);
            th += logo_h + 4;

            svg_elm.Attributes.Add("width",tw);
            svg_elm.Attributes.Add("height",th);

            return svg_elm.OuterHtml;
        }

        public void BatchToBrowser(Tuning tuning,IEnumerable<NotePattern> ngl) {
            string result = GetBatchHtml(tuning,ngl);
            try {
                if(PlatformWrapper.Services.ShareHtml is { } share_service &&
                   ngl.FirstOrDefault() is { } first_item) {
                    string title =
                        $"{tuning.Parent.Name}_{tuning.Name}_{MainViewModel.Instance.SelectedDisplayMode}";
                    share_service.ShareHtml(result,title);
                    return;
                }

                string fp = Path.Combine(
                    Path.GetTempPath(),
                    Path.GetRandomFileName().SplitNoEmpty(".")[0] + ".html");
                File.WriteAllText(fp,result);
                PlatformWrapper.Services.UriNavigator.NavigateTo(
                    fp.ToFileSystemUriFromPath(),null);
            } catch(Exception ex) {
                ex.Dump();
            }
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

            bool is_user_note = stvm.SelectedNotes.Any(x => x.RowNum == note.RowNum && x.NoteNum == note.ColNum);

            return is_user_note;
        }

        protected HtmlNode InitBuild(object args) {
            CurrentDoc = new();
            HtmlNode svg = CurrentDoc.CreateElement("svg");
            svg.Attributes.Add("xmlns","http://www.w3.org/2000/svg");

            if(args.ToStringOrEmpty().Contains("styled")) {
                HtmlNode style_elm = CurrentDoc.CreateElement("style");
                style_elm.InnerHtml = MainViewModel.Instance.MatchSvgCss;
                svg.AppendChild(style_elm);
            }

            if(args.ToStringOrEmpty().Contains("titled")) {
                WithTitle = true;
            }

            return svg;
        }

        protected void FinishBuild(object args) {
            WithTitle = false;
        }

        protected HtmlNode CreateG(HtmlNode cntr,string classes = "") {
            HtmlNode g = CurrentDoc.CreateElement("g");
            g.Attributes.Add("class",classes);
            cntr.AppendChild(g);
            return g;
        }

        protected void AddMarkerShape(
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


        protected void AddTitleText(
            HtmlNode cntr,
            string text,
            string text2,
            string text3,
            double fs,
            string fill,
            double w,
            bool isBold = false,
            string classes = null,
            bool shadow = false,
            double ox = 0,
            double oy = 0) {
            // fs: 6 each char is ~3.6 w so fsr = fs* 3.6/6
            double fsr = fs * 0.6d;
            double tw = (text.Length - 1) * fsr;
            double tx = (w / 2d) - (tw / 2d);
            double ty = fs * 2;
            double fsr2 = fsr / 2d;
            double fs2 = fs / 2d;
            double ty2 = ty + (fs / 3d);
            double w2 = text2.Length * fsr2;
            double ty3 = ty - (fs / 3d);
            double w3 = text3.Length * fsr2;
            tx -= (w2 + w3) / 2d;
            double tx2 = tx + tw + fsr2;
            double tx3 = tx2 + fsr2;
            AddText(cntr,text,fs,fill,tx + ox,ty + oy,isBold,classes,shadow);
            if(!string.IsNullOrEmpty(text2)) {
                AddText(cntr,text2,fs2,fill,tx2 + ox,ty2 + oy,isBold,classes,shadow);
            }

            if(!string.IsNullOrEmpty(text3)) {
                AddText(cntr,text3,fs2,fill,tx3 + ox,ty3 + oy,isBold,classes,shadow);
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