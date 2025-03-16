using System;
using System.Collections.Generic;
using System.Linq;
using MonkeyPaste.Common;

namespace Calcuchord {
    public static class MusicHelpers {
        public static double Distance(this InstrumentNote a,InstrumentNote b,MatchScoreMethodType scoring) {
            if(a.Key != b.Key ||
               a.IsMute != b.IsMute) {
                return 0;
            }

            switch(scoring) {
                default:
                case MatchScoreMethodType.Exact:
                    return
                        a.ColNum == b.ColNum && a.RowNum == b.RowNum ?
                            1 :
                            0;
                case MatchScoreMethodType.Voicing:
                    return 1d / (1 + Math.Abs(a.RowNum - b.RowNum) + Math.Abs(a.ColNum - b.ColNum));
                case MatchScoreMethodType.Translation:
                    return 1 / (1d + Math.Abs(a.Register - b.Register));
            }

        }

        public static Note ToNote(this (NoteType,int) nt_tup) {
            return Note.GetNote(nt_tup.Item1.Id(nt_tup.Item2));
        }

        public static int Id(this NoteType nt,int register) {
            return (int)nt + (register * 12);
        }

        public static string ToIconName(this InstrumentType it) {
            switch(it) {
                default:
                    return "MusicClefBass";
                case InstrumentType.Guitar:
                    return "GuitarElectric";
                case InstrumentType.Ukulele:
                case InstrumentType.Banjo:
                    return "GuitarAcoustic";
                case InstrumentType.Piano:
                    return "Piano";
                case InstrumentType.Cello:
                case InstrumentType.Viola:
                case InstrumentType.Violin:
                    return "Violin";
            }
        }

        public static bool IsNylon(this InstrumentType it) {
            switch(it) {
                default:
                    return false;
                case InstrumentType.Ukulele:
                case InstrumentType.Cello:
                case InstrumentType.Viola:
                case InstrumentType.Violin:
                    return true;
            }
        }

        public static bool IsDoubledStrings(this InstrumentType it) {
            switch(it) {
                default:
                    return false;
                case InstrumentType.Lute:
                case InstrumentType.Mandolin:
                    return true;
            }
        }

        public static bool IsFretless(this InstrumentType it) {
            switch(it) {
                default:
                    return false;
                case InstrumentType.Viola:
                case InstrumentType.Violin:
                    return true;
            }
        }

        public static bool IsWoundSteel(this NoteType nt,int register) {
            // anything at/below guitar G3 string...
            return nt.Id(register) <= 43;
        }

        public static (NoteType key,int register) GetDoubledString(this InstrumentType it,NoteType nt,int register,
            int strNum) {
            /*
             The lowest four strings are tuned an octave higher,
             and the highest two strings are tuned in uniso
             */
            int dbl_register = register;
            if((it == InstrumentType.Guitar ||
                it == InstrumentType.Lute) &&
               strNum < 4) {
                dbl_register--;
            }

            return (nt,dbl_register);
        }

        public static NoteType ToDegree(this NoteType nt,ChordKeyDegreeType ckdt) {
            return new Note(nt,2).Offset(ckdt.GetOffset()).Key;
        }

        public static int GetOffset(this ChordKeyDegreeType ckdt) {
            // E, F♯, G♯, A, B, C♯, and D♯
            // 0, 2,  4,  5, 7, 9,      11
            switch(ckdt) {
                default:
                case ChordKeyDegreeType.I:
                    return 0;
                case ChordKeyDegreeType.ii:
                    return 2;
                case ChordKeyDegreeType.iii:
                    return 4;
                case ChordKeyDegreeType.IV:
                    return 5;
                case ChordKeyDegreeType.V:
                    return 7;
                case ChordKeyDegreeType.vi:
                    return 9;
                case ChordKeyDegreeType.vii:
                    return 11;
            }
        }

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
            { "sharp","#" },
        };

        public static string ToDisplayValue(this MusicPatternType mpt,string suffixKey) {
            string dv = suffixKey;
            foreach(var cst_kvp in SuffixTranslations) {
                dv = dv.Replace(cst_kvp.Key,cst_kvp.Value);
            }

            return dv.ToProperCase();
        }

    }
}