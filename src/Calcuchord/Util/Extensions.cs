using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Calcuchord {
    public static class Extensions {
        public static bool IsFlagEnabled(this InstrumentType it,SvgFlags flag) {
            if(it != InstrumentType.Piano) {
                return true;
            }
            if(flag == SvgFlags.Colors ||
               flag == SvgFlags.Fingers ||
               flag == SvgFlags.Frets ||
               flag == SvgFlags.Tuning) {
                return false;
            }
            return true;
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