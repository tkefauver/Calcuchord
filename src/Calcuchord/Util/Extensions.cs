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
            if(null == source) {
                throw new ArgumentNullException(nameof(source));
            }

            var data = source.ToArray();

            return Enumerable
                .Range(0,1 << data.Length)
                .Select(
                    index => data
                        .Where((v,i) => (index & (1 << i)) != 0)
                        .ToArray());
        }
    }
}