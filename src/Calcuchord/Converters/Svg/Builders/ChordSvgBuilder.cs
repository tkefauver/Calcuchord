using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class ChordSvgBuilder : SvgBuilderBase {
        public void Test(IEnumerable<NoteGroup> ngl) {
            HtmlDocument doc = new();

            HtmlNode body = doc.CreateElement("body");
            doc.DocumentNode.AppendChild(body);

            void AddSvg(HtmlNode svg,NoteGroup ng) {
                HtmlNode title = doc.CreateElement("span");
                title.InnerHtml = ng.ToString();

                body.AppendChild(doc.CreateElement("br"));
                body.AppendChild(title);
                body.AppendChild(doc.CreateElement("br"));
                body.AppendChild(svg);
            }

            ngl.ForEach(x => AddSvg(Build(x),x));

            string result = doc.DocumentNode.OuterHtml;
            MpFileIo.WriteTextToFile("/home/tkefauver/Desktop/chords.html",result);
        }

        public override HtmlNode Build(NoteGroup ng) {
            HtmlDocument doc = new();

            string fg = "#000000"; //ColorPalette.Instance.P[PaletteColorType.Fg];
            double fret_line_w = 0.25;
            double fret_line_h = 0.25;
            double nut_line_h = 1;
            double str_line_w = 0.25;
            double str_line_h = 0.25;

            double fret_w = 10;
            double fret_h = 12;

            double dot_r = 4;
            double dot_stroke_w = 1;
            double bar_h = 9; // r*2 + stroke_w
            double bar_offset_y = (fret_h - bar_h) / 2d;

            int visible_fret_count = 4;
            double font_size = 4;
            string font_family = "Verdana";

            int str_count = ng.Parent.Parent.Parent.StringCount;
            int min_fret = 0;
            int max_fret = 0;
            int min_vis_fret = 0;
            if(ng.Notes.Where(x => x.FretNum >= 0) is { } real_frets &&
               real_frets.Any()) {
                min_fret = real_frets.Min(x => x.FretNum);
                max_fret = real_frets.Max(x => x.FretNum);
                if(real_frets.Where(x => x.FretNum > 0) is { } vis_frets &&
                   vis_frets.Any()) {
                    min_vis_fret = vis_frets.Min(x => x.FretNum);
                }
            }

            bool show_header_labels = ng.Notes.Any(x => x.FretNum <= 0);
            bool show_nut = max_fret - min_vis_fret <= visible_fret_count;
            bool show_fret_marker = !show_nut;
            double min_fret_x = show_fret_marker ? fret_w : 0;

            double pad = 0;
            int rows = visible_fret_count;
            int cols = str_count;
            if(show_header_labels) {
                rows++;
            }

            if(show_fret_marker) {
                cols++;
            }

            double tw = (fret_w * cols) + pad;
            double th = (fret_h * rows) + pad;
            double tox = 0;
            double toy = 0;
            HtmlNode svg = doc.CreateElement("svg");
            svg.Attributes.Add("xmlns","http://www.w3.org/2000/svg");
            svg.Attributes.Add("width",tw);
            svg.Attributes.Add("height",th);

            // bg
            HtmlNode bg_g = doc.CreateElement("g");
            bg_g.Attributes.Add("transform",$"translate({tox},{toy})");
            svg.AppendChild(bg_g);


            if(show_header_labels) {
                double cur_x = min_fret_x;
                double cur_y = fret_h; //-1;
                for(int i = 0; i < str_count; i++) {
                    if(ng.Notes.FirstOrDefault(x => x.StringNum == i) is { } str_fret &&
                       str_fret.FretNum <= 0) {
                        double text_x = cur_x - 2;
                        double text_y = cur_y;
                        string str_header_text = str_fret.FretNum < 0 ? "X" : "O";
                        AddText(doc,bg_g,str_header_text,font_size + 2,font_family,fg,text_x,text_y);
                    }

                    cur_x += fret_w;
                }
            }

            if(show_fret_marker) {
                double text_x = 0;
                double text_y = (fret_h / 2d) - (font_size / 2d);
                string marker_text = $"{min_vis_fret}fr";
                AddText(doc,bg_g,marker_text,font_size,font_family,fg,text_x,text_y);
            }

            double curx = min_fret_x;
            double cury = show_header_labels ? fret_h : 0;
            // +2 header, footer
            for(int vis_fret_num = 0; vis_fret_num < visible_fret_count; vis_fret_num++) {
                int fret_num = min_fret + vis_fret_num;
                curx = min_fret_x;
                for(int str_num = 0; str_num < str_count; str_num++) {
                    AddRect(doc,bg_g,"transparent",fg,curx,cury,fret_w,fret_h,fret_line_w);

                    if(fret_num == 0 && show_nut) {
                        AddRect(doc,bg_g,fg,"transparent",curx,cury,fret_w,nut_line_h,fret_line_w);
                    }

                    if(fret_num > 0 &&
                       ng.Notes.FirstOrDefault(x => x.StringNum == str_num && x.FretNum == fret_num) is { } fret_note) {
                        double cur_r = dot_r;
                        bool is_root = fret_note.IsRoot;
                        // TODO add user thing
                        bool is_user = false; //
                        int finger_num = fret_note.FingerNum;
                        string dot_bg = FingerBg[finger_num];
                        string dot_fg = FingerFg[finger_num];
                        var bar_notes = ng.Notes.Where(x => x.FretNum == fret_num && x.FingerNum == finger_num)
                            .OrderBy(x => x.StringNum).ToList();
                        if(bar_notes.Count > 1) {
                            bool is_head = bar_notes.First() == fret_note;
                            bool is_tail = bar_notes.Last() == fret_note;

                            double bar_y = cury + bar_offset_y;
                            AddRect(doc,bg_g,dot_bg,"transparent",curx,bar_y,fret_w,bar_h,sw: 0);
                        }

                        double cx = curx + (fret_w / 2d);
                        double cy = cury + (fret_h / 2d);

                        if(is_root) {
                            AddCircle(doc,bg_g,RootBg,cx,cy,dot_r);
                            dot_r -= dot_stroke_w;
                        }

                        if(is_user) {
                            AddCircle(doc,bg_g,UserBg,cx,cy,dot_r);
                            dot_r -= dot_stroke_w;
                        }

                        AddCircle(doc,bg_g,dot_bg,cx,cy,dot_r);
                        AddText(doc,bg_g,finger_num.ToString(),font_size,font_family,dot_fg,cx - 1,cy + 1);
                    }

                    curx += fret_w;
                }

                cury += fret_h;
            }

            return svg;
        }
    }
}