using System;
using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public static class MusicHelpers {
        public static string ToDisplayValue(this NoteType nt,int? register = null) {
            var result = nt.ToString();
            if(result.EndsWith("b")) {
                result = (NoteType)((int)nt - 1) + "#";
            }

            if(register is { } reg_val) {
                return result + reg_val;
            }

            return result;
        }

        public static (NoteType nt,int? register)? ParseNote(string text) {
            if(text == "Eb") {

            }

            var sharp_idx = text.IndexOf('#');

            int? register = null;

            if(text.ToCharArray().FirstOrDefault(x => char.IsNumber(x)) is { } first_reg_char &&
               first_reg_char != '\0') {
                var first_reg_idx = text.IndexOf(first_reg_char);
                var reg_str = text.Substring(first_reg_idx,text.Length - first_reg_idx);
                if(int.TryParse(reg_str,out var reg_str_val)) {
                    register = reg_str_val;
                }
            }

            var nt_text = text;
            if(sharp_idx >= 0) {
                nt_text = text.Substring(startIndex: 0,sharp_idx);
            } else if(register.HasValue) {
                nt_text = text.Substring(startIndex: 0,text.Length - register.Value.ToString().Length);
            }

            if(nt_text.TryToEnum(out NoteType nt)) {
                var nt_val = (int)nt;
                if(sharp_idx >= 0) {
                    nt_val--;
                    if(nt_val < 0) {
                        nt_val = Enum.GetNames(typeof(NoteType)).Length - 1;
                    }
                }

                return ((NoteType)nt_val,register);
            }

            return null;

        }

        static Dictionary<string,string> ChordSuffixTranslations { get; } = new() {
            { "_","/" },
            { "Num",string.Empty },
            { "sharp","#" }
        };

        public static string ToDisplayValue(this ChordSuffixType cst) {
            var dv = cst.ToString();
            foreach(var cst_kvp in ChordSuffixTranslations) {
                dv = dv.Replace(cst_kvp.Key,cst_kvp.Value);
            }

            return dv;
        }

        public static string ToChordEnumName(this string chord_suffix_disp_str) {
            var en = chord_suffix_disp_str;

            foreach(var cst_kvp in ChordSuffixTranslations) {
                en = en.Replace(cst_kvp.Value,cst_kvp.Key);
            }

            return en;
        }
    }
}