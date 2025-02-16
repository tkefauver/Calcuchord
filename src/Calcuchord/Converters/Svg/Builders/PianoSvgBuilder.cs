using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public class PianoSvgBuilder : SvgBuilderBase {
        // protected override string UserBg =>
        //     ThemeViewModel.Instance.P[PaletteColorType.Finger1Bg];
        //
        // protected override string UserFg =>
        //     ThemeViewModel.Instance.P[PaletteColorType.Finger1Fg];


        public override HtmlNode Build(NotePattern ng,object args) {
            HtmlNode svg = InitBuild(args);

            HtmlNode bg_g = CurrentDoc.CreateElement("g");
            bg_g.SetAttributeValue("transform","scale(0.9) translate(10,0)");
            svg.AppendChild(bg_g);

            HtmlNode wk_bg_g = CurrentDoc.CreateElement("g");
            bg_g.AppendChild(wk_bg_g);

            HtmlNode bk_bg_g = CurrentDoc.CreateElement("g");
            bg_g.AppendChild(bk_bg_g);
            double lw = FretLineFixedAxisSize * 2;
            double bw = lw * 1;
            double wkw = StringFixedAxisLength;
            double bkw = wkw * KeyboardView.WHITE_TO_BLACK_WIDTH_RATIO;
            double wkh = wkw * KeyboardView.WHITE_WIDTH_TO_HEIGHT_RATIO;
            double bkh = wkh * KeyboardView.WHITE_TO_BLACK_HEIGHT_RATIO;

            string bkf = Fg;
            string wkf = Bg;
            string stroke = bkf;
            string pattern_bg = ThemeViewModel.Instance.P[PaletteColorType.UserFretBg];
            string pattern_fg = ThemeViewModel.Instance.P[PaletteColorType.UserFretFg];

            var pattern_notes = ng.Notes.OrderBy(x => x.NoteNum);
            InstrumentNote root_note = ng.Parent.Parent.OpenNotes.FirstOrDefault();
            int key_count = ng.Parent.Parent.WorkingColCount;

            double cur_x = 0;
            double cur_y = 0;

            for(int i = 0; i < key_count; i++) {
                Note cur_note = root_note.Offset(i);
                bool is_black = cur_note.IsAltered;
                PatternNote pattern_note = pattern_notes.FirstOrDefault(x => x.NoteNum == i);
                bool is_root = pattern_note != null && pattern_note.IsRoot;
                bool is_user = IsUserNote(pattern_note);

                double w = is_black ? bkw : wkw;
                double h = is_black ? bkh : wkh;
                double x = is_black ? cur_x - (bkw / 2d) : cur_x;
                double y = cur_y;
                double fs = is_black ? BodyFontSize - (bw * 2) : HeaderFontSize;

                void ReduceDims() {
                    x += bw;
                    y += bw;
                    w -= bw * 2;
                    h -= bw * 2;
                }

                string fill = is_black ? bkf : wkf;
                HtmlNode cntr = is_black ? bk_bg_g : wk_bg_g;

                // outline
                AddRect(cntr,Transparent,stroke,x,y,w,h,lw);
                // bg
                AddRect(cntr,fill,stroke,x,y,w,h,lw);

                // root
                if(is_root) {
                    //ReduceDims();
                    //AddRect(cntr,RootBg,RootBg,x,y,w,h,lw);
                    //marker_bg = Fg;
                }


                // user
                // if(is_user && flags.HasFlag(SvgOptionType.Matches)) {
                //     ReduceDims();
                //     AddRect(cntr,UserBg,UserBg,x,y,w,h,lw);
                // }

                if(pattern_note != null) {
                    ReduceDims();
                    AddRect(cntr,UserBg,UserBg,x,y,w,h,lw);

                    string marker_bg = Transparent;
                    string marker_fg = Bg;

                    double pad = is_black ? 0.5 : 1;
                    double mw = w - (pad * 2);
                    double mh = mw;
                    double cx = x + (w / 2d);
                    double cy = (y + h) - pad - (mh / 2d);
                    double r = mw / 2d;
                    if(is_root) {
                        AddMarkerShape(cntr,true,RootBg,Transparent,cx,cy,r,0,"root-box");
                        r -= DotStrokeWidth;
                    }

                    if(is_user) {
                        AddMarkerShape(cntr,is_root,pattern_bg,Transparent,cx,cy,r,0,"user-fill");
                        if(is_root) {
                            double ir = r - DotStrokeWidth;
                            AddMarkerShape(cntr,true,RootBg,Transparent,cx,cy,ir,0,"root-box");

                        }
                    }

                    AddCenteredText(
                        cntr,pattern_note.Name,fs,pattern_fg,cx - r,cy - r,r * 2d,r * 2d,classes: "notes-text",
                        oy: is_black ? 0 : -0.5);
                }


                if(!is_black) {
                    cur_x += wkw;
                }
            }

            double tw = cur_x;
            double th = wkh;

            svg.Attributes.Add("width",tw);
            svg.Attributes.Add("height",th);

            return svg;
        }
    }
}