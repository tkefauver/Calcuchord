using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public class ChordSvgBuilder : SvgBuilderBase {


        public override HtmlNode Build(NotePattern ng,object args) {
            HtmlNode svg = InitBuild(args);

            HtmlNode cntr_g = CurrentDoc.CreateElement("g");
            svg.AppendChild(cntr_g);

            HtmlNode fret_grid_g = CurrentDoc.CreateElement("g");
            cntr_g.AppendChild(fret_grid_g);

            HtmlNode bg_g = CurrentDoc.CreateElement("g");
            cntr_g.AppendChild(bg_g);

            int vfc = 4;
            double lw = FretLineFixedAxisSize;
            double fw = FretLength;
            double fh = StringFixedAxisLength;
            double nh = NutFixedAxisSize;

            var notes = ng.Notes.ToList();

            int str_count = ng.Parent.Parent.Parent.RowCount;
            int min_fret = 0;
            int max_fret = 0;
            int min_vis_fret = 0;
            // get min/max visual frets
            if(notes.Where(x => x.ColNum >= 0) is { } real_frets &&
               real_frets.Any()) {
                min_fret = real_frets.Min(x => x.ColNum);
                max_fret = real_frets.Max(x => x.ColNum);
                if(real_frets.Where(x => x.ColNum > 0) is { } vis_frets &&
                   vis_frets.Any()) {
                    min_vis_fret = vis_frets.Min(x => x.ColNum);
                }
            }

            // find barres
            int? shadow_barre_fret_num = null;
            Dictionary<int,(int min_str,int max_str)> barred_fret_lookup = [];
            if(notes.Where(x => x.ColNum > 0 && x.FingerNum > 0)
                   .GroupBy(x => x.FingerNum)
                   .Where(x => x.Count() > 1) is { } barre_groups &&
               barre_groups.Any()) {
                var barre_frets = barre_groups.SelectMany(x => x);
                if(notes.All(x => x.ColNum != 0)) {
                    // only show shadow when no open notes
                    // and min barre fret doesn't have other fingers
                    int min_barre_fret_num = barre_frets.Min(x => x.ColNum);
                    if(min_barre_fret_num == min_vis_fret &&
                       notes
                           .Where(x => x.FingerNum > 0 && x.ColNum == min_barre_fret_num)
                           .Select(x => x.FingerNum).Distinct().Count() ==
                       1) {
                        shadow_barre_fret_num = min_barre_fret_num;
                    }
                }

                var barre_fret_nums = barre_frets.Select(x => x.ColNum).Distinct();
                foreach(int barre_fret_num in barre_fret_nums) {
                    int min_str = barre_frets.Where(x => x.ColNum == barre_fret_num).Min(x => x.RowNum);
                    int max_str = barre_frets.Where(x => x.ColNum == barre_fret_num).Max(x => x.RowNum);
                    barred_fret_lookup.Add(barre_fret_num,(min_str,max_str));
                }
            }

            bool show_header_labels = notes.Any(x => x.ColNum <= 0);
            bool show_nut = max_fret < vfc;
            bool show_fret_marker = !show_nut;
            if(show_nut) {
                min_fret = 0;
                min_vis_fret = 1;
            }

            double min_fret_x = fw + 2; //show_fret_marker ? fw : 0;

            // +2 header, footer
            int rows = vfc + 2;

            // +1 fret label
            int cols = str_count + 1;

            double header_h = fh + 0.25;
            double curx = min_fret_x;
            double cury = header_h;
            if(ForPrint) {
                rows++;
                cury += fh * 2;
            }

            double tw = fw * cols;
            double th = fh * rows;

            if(ForPrint) {
                AddCenteredText(bg_g,ng.FullName,6,Bg,0,0,tw,fh * 2,classes: "match-title");
            }

            // fret labels
            {
                HtmlNode fret_g = CreateG(bg_g,"fret-labels");

                double fs = BodyFontSize;
                double offset_x = fs * 1.25d;
                double offset_y = fs / 3d;
                double marker_x = fw - offset_x;
                double marker_y = cury + offset_y + (fh / 2d);

                for(int i = 0; i < vfc; i++) {
                    // fret num label
                    int fret_num = min_vis_fret + i;
                    string label_text = fret_num.ToString();
                    double cur_x = marker_x;
                    if(label_text.Length > 1) {
                        cur_x -= offset_x / 2d;
                    }

                    AddText(fret_g,label_text,fs,Fg,cur_x,marker_y);
                    marker_y += fh;
                }
            }

            if(show_fret_marker) {
                double fs = BodyFontSize;
                if(min_vis_fret > 9) {
                    fs -= 1; //0.75;
                }

                double marker_x = 0;
                double marker_y = (cury + (fh / 2d) + (fs / 2d)) - 0.75;
                string marker_text = $"{min_vis_fret}fr";

                // open label
                HtmlNode marker_g = CreateG(bg_g,"fret-marker");
                AddText(marker_g,marker_text,fs,Fg,marker_x,marker_y);
            }

            if(show_header_labels) {
                double header_x = min_fret_x;
                double header_y = fh + (ForPrint ? fh * 2 : 0); //-1;
                for(int i = 0; i < str_count; i++) {
                    if(notes.FirstOrDefault(x => x.RowNum == i) is { } str_fret &&
                       str_fret.ColNum <= 0) {
                        bool is_mute = str_fret.ColNum < 0;
                        if(is_mute) {
                            double mute_size = 3;
                            double margin_y = 1;
                            double x1 = header_x - (mute_size / 2d);
                            double y1 = header_y - mute_size - margin_y;
                            double x2 = header_x + (mute_size / 2d);
                            double y2 = header_y - margin_y;
                            // draw \
                            AddLine(bg_g,Fg,x1,y1,x2,y2,lw * 2d);
                            // draw /
                            AddLine(bg_g,Fg,x2,y1,x1,y2,lw * 2d);
                        } else {
                            // open 'o'
                            double open_r = 2;
                            double x = header_x;
                            double y = header_y - (fh / 4d);
                            AddCircle(bg_g,Transparent,Fg,x,y,open_r,lw,str_fret.IsRoot ? "root-open" : string.Empty);
                        }
                    }

                    header_x += fw;
                }
            }

            // tuning
            {
                var open_notes = ng.Parent.Parent.OpenNotes;
                double fs = BodyFontSize;
                double offset_x = fs / 2;
                double cur_x = min_fret_x - offset_x;
                double cur_y = th - (fh / 2d);
                HtmlNode tuning_g = CreateG(bg_g,"string-tuning");

                for(int i = 0; i < str_count; i++) {
                    string label_text = open_notes[i].FullName;
                    AddText(tuning_g,label_text,fs,Fg,cur_x,cur_y);
                    cur_x += fw;
                }
            }

            (int min_str,int max_str)? cur_bar_extent = null;
            PatternNote last_barre_note = null;

            for(int vis_fret_num = 0; vis_fret_num < vfc; vis_fret_num++) {
                int fret_num = min_vis_fret + vis_fret_num;
                curx = min_fret_x;
                for(int str_num = 0; str_num < str_count; str_num++) {
                    double cx = curx;
                    double cy = cury + (fh / 2d); //(fh + cury) - (fh / 2d);
                    double bar_y = cy - (BarHeight / 2d);
                    double dot_r = DotRadius;
                    PatternNote fret_note = notes.Where(x => x.ColNum > 0)
                        .FirstOrDefault(x => x.RowNum == str_num && x.ColNum == fret_num);
                    string fret_bg = null;
                    string fret_fg = null;

                    PatternNote primary_note = fret_note ?? last_barre_note;
                    if(primary_note != null) {
                        // fret_bg = flags.HasFlag(SvgOptionType.Colors) ? FingerBg[primary_note.FingerNum] : Fg;
                        // fret_fg = flags.HasFlag(SvgOptionType.Colors) ? FingerFg[primary_note.FingerNum] : Bg;
                        fret_bg = FingerBg[primary_note.FingerNum];
                        fret_fg = FingerFg[primary_note.FingerNum];
                    }

                    if(str_count == 1 || str_num < str_count - 1) {
                        // fret/string cell
                        AddRect(fret_grid_g,Transparent,Fg,curx,cury,fw,fh,lw);

                        if(fret_num == 1 && show_nut) {
                            // nut line
                            AddRect(fret_grid_g,Fg,Transparent,curx,cury,fw,nh,lw);
                        }
                    }

                    bool is_barred_fret = false;
                    if(barred_fret_lookup.TryGetValue(fret_num,out (int min_str,int max_str) bar_extent) &&
                       str_num >= bar_extent.min_str &&
                       str_num < bar_extent.max_str) {
                        if(fret_note != null) {
                            last_barre_note = fret_note;
                        }

                        cur_bar_extent = bar_extent;
                        is_barred_fret = true;
                    }

                    if(!is_barred_fret &&
                       shadow_barre_fret_num.HasValue &&
                       shadow_barre_fret_num.Value == fret_num) {
                        if(str_num < str_count - 1) {
                            // shadow rect
                            AddRect(
                                bg_g,BarShadow,Transparent,curx,bar_y,fw,BarHeight,0,
                                fillOpacity: ShadowOpacity,
                                classes: "barre-elm shadow-elm");
                        }

                        if(fret_note == null &&
                           (str_num == 0 || str_num == str_count - 1)) {
                            // shadow edge
                            HtmlNode shadow_g = CurrentDoc.CreateElement("g");
                            shadow_g.Attributes.Add("class","barre-elm shadow-elm");
                            shadow_g.Attributes.Add("transform",$"translate({cx},{cy})");
                            bg_g.AppendChild(shadow_g);
                            HtmlNode shadow_path = CurrentDoc.CreateElement("path");
                            shadow_path.Attributes.Add("fill",BarShadow);
                            shadow_path.Attributes.Add("fill-opacity",ShadowOpacity);
                            double angle = str_num == 0 ? -90 : 90;
                            shadow_path.Attributes.Add("transform",$"rotate({angle})");
                            double sh = BarHeight;
                            double shh = BarHeight / 2d;
                            shadow_path.Attributes.Add("d",$"M 0, 0 m -{shh}, 0 a {shh},{shh} 0 1,1 {sh},0");
                            shadow_g.AppendChild(shadow_path);
                        }
                    }

                    if(last_barre_note != null) {
                        bool is_tail = cur_bar_extent.Value.max_str == str_num;
                        if(is_tail) {
                            last_barre_note = null;
                            cur_bar_extent = null;
                        } else {
                            // bar rect (adding extra 1 to left/right to cover bg grid)
                            double pad = str_num < str_count - 1 ? 0.25 : 0;
                            AddRect(
                                bg_g,fret_bg,Transparent,curx - pad,bar_y,fw + (pad * 2),BarHeight,0,
                                "barre-elm fingers-fill");
                            last_barre_note = fret_note ?? last_barre_note;
                        }
                    }

                    if(fret_note != null) {
                        bool is_root = fret_note.IsRoot;
                        bool is_user = IsUserNote(fret_note);
                        int finger_num = fret_note.FingerNum;

                        void AddFingerShape(bool is_box,string classes) {
                            double shape_r = dot_r;
                            HtmlNode shape_cntr = CreateG(bg_g,classes);
                            if(is_box) {
                                // root outer box
                                AddMarkerShape(shape_cntr,true,RootBg,Transparent,cx,cy,shape_r,0);
                                shape_r -= DotStrokeWidth;
                            } else if(classes == "root-circle") {
                                AddMarkerShape(shape_cntr,false,RootBg,Transparent,cx,cy,shape_r,0);
                                shape_r -= DotStrokeWidth;
                            }

                            if(is_user) {
                                // user outer circle
                                AddMarkerShape(
                                    shape_cntr,is_box,UserBg,Transparent,cx,cy,shape_r,0,
                                    shadow: false, //!is_root && !is_barred_fret,
                                    classes: "user-fill");
                                shape_r -= DotStrokeWidth;
                            }

                            // finger circle
                            AddMarkerShape(
                                shape_cntr,is_box,fret_bg,Transparent,cx,cy,shape_r,0,
                                shadow: false, //!is_root && !is_user && !is_barred_fret,
                                classes: "fingers-fill");
                        }


                        AddFingerShape(false,is_root ? "root-circle" : "note-circle");
                        if(is_root) {
                            AddFingerShape(true,"root-box");
                        }

                        // finger num text
                        double tx = cx - dot_r;
                        double ty = cy - dot_r;
                        double d = dot_r * 2d;
                        double oy = -0.5d;
                        HtmlNode finger_g = CreateG(bg_g,"fingers-text");
                        AddCenteredText(
                            finger_g,finger_num.ToString(),BodyFontSize,fret_fg,tx,ty,d,d,shadow: true,oy: oy);

                        HtmlNode notes_g = CreateG(bg_g,"notes-text");
                        AddCenteredText(notes_g,fret_note.Name,BodyFontSize,fret_fg,tx,ty,d,d,shadow: true,oy: oy);
                    }

                    curx += fw;
                }

                cury += fh;
            }

            double tox = 0;
            double toy = 0;
            th = th - fh;
            toy = -(fh / 2d);

            cntr_g.Attributes.Add("transform",$"translate({tox},{toy})");

            svg.Attributes.Add("width",tw);
            svg.Attributes.Add("height",th);

            //bg_g.Attributes.Add("transform",$"translate({tox},{toy})");

            return svg;
        }
    }
}