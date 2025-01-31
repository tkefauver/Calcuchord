using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public class PianoSvgBuilder : SvgBuilderBase {
        protected override string UserBg =>
            ThemeViewModel.Instance.P[PaletteColorType.Finger1Bg];

        protected override string UserFg =>
            ThemeViewModel.Instance.P[PaletteColorType.Finger1Fg];


        public override HtmlNode Build(NoteGroup ng) {
            HtmlNode svg = InitBuild();

            HtmlNode bg_g = CurrentDoc.CreateElement("g");
            svg.AppendChild(bg_g);

            HtmlNode wk_bg_g = CurrentDoc.CreateElement("g");
            bg_g.AppendChild(wk_bg_g);

            HtmlNode bk_bg_g = CurrentDoc.CreateElement("g");
            bg_g.AppendChild(bk_bg_g);

            if(ng.ToString() == "C Major #1") {
            }

            SvgFlags flags = Prefs.Instance.SelectedSvgFlags;

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
            int key_count = ng.Parent.Parent.FretCount;

            double cur_x = 0;
            double cur_y = 0;

            for(int i = 0; i < key_count; i++) {
                Note cur_note = root_note.Offset(i);
                bool is_black = cur_note.IsAltered;
                PatternNote pattern_note = pattern_notes.FirstOrDefault(x => x.NoteNum == i);
                bool is_root = pattern_note != null && pattern_note.IsRoot;
                bool is_user = is_root || IsUserNote(pattern_note);

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
                if(is_root && flags.HasFlag(SvgFlags.Roots)) {
                    ReduceDims();
                    AddRect(cntr,RootBg,RootBg,x,y,w,h,lw);
                }

                // user
                if(is_user && flags.HasFlag(SvgFlags.Matches)) {
                    ReduceDims();
                    AddRect(cntr,UserBg,UserBg,x,y,w,h,lw);
                }

                if(pattern_note != null) {
                    ReduceDims();
                    AddRect(cntr,pattern_bg,pattern_bg,x,y,w,h,lw);

                    if(flags.HasFlag(SvgFlags.Notes)) {
                        double txw = w;
                        double txh = txw;
                        double txx = x;
                        double txy = (y + h) - txh;
                        AddCenteredText(cntr,pattern_note.Name,fs,pattern_fg,txx,txy,txw,txh);
                    }
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