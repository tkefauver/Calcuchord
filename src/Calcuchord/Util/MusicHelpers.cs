using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public static class MusicHelpers {
        public static string ToDisplayValue(this NoteType nt,int? register = null) {
            string result = nt.ToString();
            if(result.EndsWith("b")) {
                result = (NoteType)((int)nt - 1) + "#";
            }

            if(register is { } reg_val) {
                return result + reg_val;
            }

            return result;
        }

        public static (NoteType nt,int? register)? ParseNote(string text) {
            int sharp_idx = text.IndexOf('#');

            int? register = null;

            if(text.ToCharArray().FirstOrDefault(x => char.IsNumber(x)) is { } first_reg_char &&
               first_reg_char != '\0') {
                int first_reg_idx = text.IndexOf(first_reg_char);
                string reg_str = text.Substring(first_reg_idx,text.Length - first_reg_idx);
                if(int.TryParse(reg_str,out int reg_str_val)) {
                    register = reg_str_val;
                }
            }

            string nt_text = text;
            if(sharp_idx >= 0) {
                nt_text = text.Substring(0,sharp_idx);
            } else if(register.HasValue) {
                nt_text = text.Substring(0,text.Length - register.Value.ToString().Length);
            }

            if(nt_text.TryToEnum(out NoteType nt)) {
                int nt_val = (int)nt;
                if(sharp_idx >= 0) {
                    nt_val++;
                }

                return ((NoteType)nt_val,register);
            }

            return null;
        }

        static Dictionary<string,string> SuffixTranslations { get; } = new Dictionary<string,string>
        {
            { "_","/" },
            { "Num",string.Empty },
            { "sharp","#" }
        };

        public static string ToDisplayValue(this MusicPatternType mpt,string suffixKey) {
            string dv = suffixKey;
            foreach(var cst_kvp in SuffixTranslations) {
                dv = dv.Replace(cst_kvp.Key,cst_kvp.Value);
            }

            return dv.ToProperCase();
        }

        public static string ToIconName(this InstrumentType it) {
            switch(it) {
                case InstrumentType.Guitar:
                    return "GuitarElectric";
                case InstrumentType.Ukulele:
                    return "GuitarAcoustic";
                case InstrumentType.Piano:
                    return "Piano";
                default:
                    return "Music";
            }
        }
    }
}