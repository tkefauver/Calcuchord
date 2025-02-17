using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public static class Extensions {
        public static void OpenInBrowser(this Uri uri) {
            string url = uri.AbsoluteUri;
            if(OperatingSystem.IsWindows()) {
                //Process.Start(new ProcessStartInfo("cmd",$"/c start {url}") {UseShellExecute = true});
                Process.Start(new ProcessStartInfo { FileName = url,UseShellExecute = true });
                return;
            }

            if(OperatingSystem.IsLinux()) {
                Process.Start("xdg-open",url);
                return;
            }

            if(OperatingSystem.IsMacOS()) {
                Process.Start("open",url);
            }

        }

        public static bool None<TSource>(this IEnumerable<TSource> source) {
            return !source.Any();
        }

        public static bool None<TSource>(this IEnumerable<TSource> source,Func<TSource,bool> predicate) {
            return !source.Any(predicate);
        }

        public static string ToIconName(this InstrumentType it) {
            //return $"avares://Calcuchord/Assets/Svg/Instruments/{it.ToString().ToLower()}.svg";
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

        public static bool IsFlagEnabled(this SvgOptionType optionType,InstrumentType it,MusicPatternType pt,
            DisplayModeType dmt) {
            if(dmt != DisplayModeType.Search) {
                if(optionType == SvgOptionType.Matches) {
                    return false;
                }
            }

            if(it != InstrumentType.Piano) {
                if(pt != MusicPatternType.Chords && optionType == SvgOptionType.Barres) {
                    return false;
                }

                return true;
            }

            // piano only below here
            return optionType is SvgOptionType.Notes or SvgOptionType.Roots or SvgOptionType.Matches;
        }

        public static double CentimetersToInches(this double cms) {
            return cms / 2.54d;
        }

        public static double nchesToCentimeters(this double inches) {
            return inches * 2.54d;
        }

        public static void Add(this HtmlAttributeCollection hac,string key,double val) {
            hac.Add(key,val.ToString());
        }

        public static IEnumerable<T[]> PowerSet<T>(this IEnumerable<T> source) {
            // from https://stackoverflow.com/a/57058345/105028
            var data = source.ToArray();

            return
                // from 0 to 2^N...
                Enumerable.Range(0,1 << data.Length)
                    .Select(
                        x => data
                            .Where((v,i) => (x & (1 << i)) != 0)
                            .ToArray()
                    );
        }
        
        public static IEnumerable<IEnumerable<T>> PowerSet4<T>(this IEnumerable<T> source) {
            // from https://stackoverflow.com/a/57058345/105028
            var data = source.ToArray();
            return
                // from 0 to 2^N...
                Enumerable.Range(0,1 << data.Length)
                    .Select(
                        x => source
                            .Where((v,i) => (x & (1 << i)) != 0)
                            //.ToArray()
                    );
        }

        public static List<List<T>> PowerSet3<T>(this List<T> list) {
            var result = new List<List<T>>();
            // head
            result.Add(new List<T>());
            result.Last().Add(list[0]);
            if(list.Count == 1) {
                return result;
            }

            // tail
            var tailCombos = PowerSet3(list.Skip(1).ToList());
            tailCombos.ForEach(
                combo => {
                    result.Add(new List<T>(combo));
                    combo.Add(list[0]);
                    result.Add(new List<T>(combo));
                });
            return result;
        }

        public static List<T[]> PowerSet2<T>(this IEnumerable<T> s) {
            var data = s.ToArray();
            int n = data.Length;
            var result = new List<T[]>();

            // Iterate through all subsets (represented by 0 to 2^n - 1)
            for(int i = 0; i < 1 << n; i++) {
                var subset = new List<T>();
                for(int j = 0; j < n; j++) {
                    // Check if the j-th bit is set in i
                    if((i & (1 << j)) != 0) {
                        //subset += s[j];
                        subset.Add(data[j]);
                    }
                }

                result.Add(subset.ToArray());
            }

            return result;
        }

    }
}