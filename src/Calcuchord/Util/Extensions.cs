using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public static class Extensions {
        public static bool None<TSource>(this IEnumerable<TSource> source) {
            return !source.Any();
        }

        public static bool None<TSource>(this IEnumerable<TSource> source,Func<TSource,bool> predicate) {
            return !source.Any(predicate);
        }

        public static bool IsFlagEnabled(this SvgFlags flag,InstrumentType it,MusicPatternType pt,DisplayModeType dmt) {
            if(dmt != DisplayModeType.Search) {
                if(flag == SvgFlags.Matches) {
                    return false;
                }
            }

            if(it != InstrumentType.Piano) {
                if(pt != MusicPatternType.Chords && flag == SvgFlags.Barres) {
                    return false;
                }

                return true;
            }

            // piano only below here
            return flag is SvgFlags.Notes or SvgFlags.Roots or SvgFlags.Matches;
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

        public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source) {
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