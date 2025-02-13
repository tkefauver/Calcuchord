using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public class ScaleSvgBuilder : SvgBuilderBase {
        public override HtmlNode Build(NoteGroup ng,object args) {
            HtmlNode svg = InitBuild(args);

            HtmlNode bg_g = CurrentDoc.CreateElement("g");
            svg.AppendChild(bg_g);

            //SvgOptionType flags = MainViewModel.Instance.SelectedSvgOptionType;

            int vfc = 5;

            double lw = FretLineFixedAxisSize;
            double fw = StringFixedAxisLength;
            double fhw = fw * 0.5;
            double sh = FretLength;
            double shh = sh * 0.5;
            double nw = NutFixedAxisSize;
            double bfs = BodyFontSize;
            double bfhs = bfs * 0.5;
            double hfs = HeaderFontSize;
            double hfhs = hfs * 0.5;

            var notes = ng.Notes.ToList();

            int str_count = ng.Parent.Parent.Parent.RowCount;
            int min_fret = 0;
            int max_fret = 0;
            int min_vis_fret = 0;
            // get min/max visual frets
            if(notes.Where(x => x.NoteNum >= 0) is { } real_frets &&
               real_frets.Any()) {
                min_fret = real_frets.Min(x => x.NoteNum);
                max_fret = real_frets.Max(x => x.NoteNum);
                if(real_frets.Where(x => x.NoteNum > 0) is { } vis_frets &&
                   vis_frets.Any()) {
                    min_vis_fret = vis_frets.Min(x => x.NoteNum);
                }
            }

            bool show_nut = max_fret < vfc;
            bool show_fret_marker = !show_nut;
            if(show_nut) {
                min_fret = 0;
                min_vis_fret = 1;
            }

            double min_fret_x = fhw;
            double min_fret_y = DotRadius;

            // +1 fret label
            int rows = str_count + 1;

            // +1 strings label
            int cols = vfc + 1;

            double tw = fw * cols;
            double th = sh * rows;

            // tuning
            {
                var open_notes = ng.Parent.Parent.OpenNotes;
                double fs = bfs;
                double tuning_text_x = 0;
                double tuning_text_y = (min_fret_y + bfhs) - 1;

                for(int i = 0; i < str_count; i++) {
                    string label_text = open_notes[i].FullName;
                    AddText(bg_g,label_text,fs,Fg,tuning_text_x,tuning_text_y,classes: "string-tuning");
                    tuning_text_y += sh;
                }
            }

            // frets
            {
                double fret_marker_fs = bfs;
                double fret_marker_x = min_fret_x;
                double fret_marker_y = th - (sh * 1.5);
                for(int i = 0; i < vfc; i++) {
                    // fret num label
                    int fret_num = min_fret + i;
                    string marker_text = fret_num.ToString();
                    AddCenteredText(
                        bg_g,marker_text,fret_marker_fs,Fg,fret_marker_x,fret_marker_y,fw,sh,classes: "fret-labels");
                    fret_marker_x += fw;
                }
            }

            if(show_fret_marker) {
                double fret_marker_fs = hfs;
                double fret_marker_x = min_fret_x;
                double fret_marker_y = (th - (sh * 1.5)) + 1;
                string marker_text = $"{min_vis_fret}fr";
                AddCenteredText(
                    bg_g,marker_text,fret_marker_fs,Fg,fret_marker_x,fret_marker_y,fw,sh,classes: "fret-marker");
            }

            for(int str_num = 0; str_num < str_count; str_num++) {
                for(int vis_fret_num = 0; vis_fret_num < vfc; vis_fret_num++) {
                    int fret_num = min_fret + vis_fret_num;
                    double curx = min_fret_x + (fw * vis_fret_num);
                    double cury = min_fret_y + (str_num * sh);

                    if(fret_num > 0 && str_num < str_count - 1) {
                        // fret/string cell
                        AddRect(bg_g,Transparent,Fg,curx,cury,fw,sh,lw);

                        if(fret_num == 1 && show_nut) {
                            // nut line
                            AddRect(bg_g,Fg,Transparent,curx,cury,nw,sh,lw);
                        }
                    }

                    if(notes.FirstOrDefault(x => x.RowNum == str_num && x.NoteNum == fret_num) is not
                       { } fret_note) {
                        continue;
                    }

                    double cx = curx + fhw;
                    double cy = cury;
                    double dot_r = DotRadius;

                    bool is_root = fret_note.IsRoot;
                    bool is_user = IsUserNote(fret_note);
                    int finger_num = fret_note.FingerNum;
                    string fret_bg = FingerBg[finger_num];
                    string fret_fg = FingerFg[finger_num];

                    void AddFingerShape(bool is_box,string classes) {
                        double shape_r = dot_r;
                        HtmlNode shape_cntr = CreateG(bg_g,classes);
                        if(is_box) {
                            // root outer box
                            AddShape(shape_cntr,true,RootBg,Transparent,cx,cy,shape_r,0,shadow: true);
                            shape_r -= DotStrokeWidth;
                        }

                        if(is_user) {
                            // user outer circle
                            AddShape(
                                shape_cntr,is_box,UserBg,Transparent,cx,cy,shape_r,0,
                                shadow: !is_root,
                                classes: "user-fill");
                            shape_r -= DotStrokeWidth;
                        }

                        // finger circle
                        AddShape(
                            shape_cntr,is_box,fret_bg,Transparent,cx,cy,shape_r,0,
                            shadow: !is_root && !is_user,
                            classes: "fingers-fill");
                    }

                    AddFingerShape(false,is_root ? "root-circle" : "note-circle");
                    if(is_root) {
                        AddFingerShape(true,"root-box");
                    }

                    double tx = cx - dot_r;
                    double ty = cy - dot_r;
                    double d = dot_r * 2d;
                    double oy = -0.5d;
                    // finger num text
                    HtmlNode finger_g = CreateG(bg_g,"fingers-text");
                    AddCenteredText(
                        finger_g,finger_num.ToString(),BodyFontSize,fret_fg,tx,ty,d,d,shadow: true,oy: oy);

                    // fret note text
                    HtmlNode notes_g = CreateG(bg_g,"notes-text");
                    AddCenteredText(notes_g,fret_note.Name,BodyFontSize,fret_fg,tx,ty,d,d,shadow: true,oy: oy);
                }
            }

            // if(show_fret_marker) {
            //     th -= 5;
            // } else {
            //     th -= sh;
            // }

            svg.Attributes.Add("width",tw);
            svg.Attributes.Add("height",th);

            return svg;
        }
    }
}