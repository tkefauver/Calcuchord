using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using SkiaSharp;
using Svg.Skia;

namespace Calcuchord {
    public static class Extensions {
        public static double Distance(this Point p1,Point p2) {
            return Math.Sqrt(Math.Pow(p2.X - p1.X,2) + Math.Pow(p2.Y - p1.Y,2));
        }

        public static Point GetChildScale(this Viewbox vb) {
            if(vb.Child is not { } c) {
                return new Point(1,1);
            }

            if(c.Bounds.Width == 0 ||
               c.Bounds.Height == 0) {
                return new Point();
            }

            return new Point(vb.Bounds.Width / c.Bounds.Width,vb.Bounds.Height / c.Bounds.Height);
        }

        public static string GetTempFile(string ext) {
            return Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName().SplitNoEmpty(".")[0] + "." + ext);
        }

        public static MemoryStream ToPdfStream(string svgXml,SKColor bg,float scale) {
            using SKSvg svg = new SKSvg();
            if(svg.FromSvg(svgXml) is null) {
                return null;
            }

            MemoryStream ms = new MemoryStream();
            svg.Picture.ToPdf(ms,bg,scale,scale);
            return ms;
        }

        public static byte[] ToPdfBytes(string svgXml,SKColor bg,float scale) {
            using MemoryStream ms = ToPdfStream(svgXml,bg,scale);
            return ms.ToArray();
        }

        public static async void FireAndForgetSafeAsync(this Task task) {
            try {
                await task;
            } catch(Exception ex) {
                ex.Dump();
            }
        }

        public static async void FireAndForgetSafeAsync(this Task task,DispatcherPriority dp) {
            await Dispatcher.UIThread.InvokeAsync(
                async () => {
                    try {
                        await task;
                    } catch(Exception ex) {
                        ex.Dump();
                    }
                },dp);
        }

        public static async void FireAndForgetSafeAsync(this Task task,DispatcherPriority dp,CancellationToken ct) {
            try {
                await Dispatcher.UIThread.InvokeAsync(
                    async () => {
                        try {
                            await task;
                        } catch(Exception ex) {
                            ex.Dump();
                        }
                    },dp,ct);
            } catch(Exception ex) {
                if(ex is not TaskCanceledException) {
                    ex.Dump();
                }
            }
        }

        public static async void FireAndForgetSafeAsync(this DispatcherOperation task) {
            try {
                await task;
            } catch(Exception ex) {
                ex.Dump();
            }
        }

        public static async void FireAndForgetSafeAsync(this DispatcherOperation task,DispatcherPriority dp) {
            await Dispatcher.UIThread.InvokeAsync(
                async () => {
                    try {
                        await task;
                    } catch(Exception ex) {
                        ex.Dump();
                    }
                },dp);
        }

        public static async void FireAndForgetSafeAsync(this DispatcherOperation task,DispatcherPriority dp,
            CancellationToken ct) {
            try {
                await Dispatcher.UIThread.InvokeAsync(
                    async () => {
                        try {
                            await task;
                        } catch(Exception ex) {
                            ex.Dump();
                        }
                    },dp,ct);
            } catch(Exception ex) {
                if(ex is not TaskCanceledException) {
                    ex.Dump();
                }
            }
        }

        public static string RemoveInvalidPathChars(this string originalString) {
            // from https://stackoverflow.com/a/66053014/105028
            string finalString = string.Empty;
            if(!string.IsNullOrEmpty(originalString)) {
                return string.Concat(originalString.Split(Path.GetInvalidFileNameChars()));
            }

            return finalString;
        }

        public static void CloseFlyout(object args) {
            if(args is not Button b ||
               b.Flyout is not { } fo) {
                return;
            }

            // BUG context menu blocks popup and doesn't close 
            // so manually closing
            fo.Hide();
        }

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

        public static bool RequiresReset(this SvgOptionType optionType,InstrumentType it,MusicPatternType mpt) {
            if(it == InstrumentType.Piano) {
                return false;
            }

            if(mpt == MusicPatternType.Chords) {
                if(optionType == SvgOptionType.Tuning) {
                    return true;
                }
            } else {
                if(optionType is SvgOptionType.Frets or SvgOptionType.Tuning) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFlagEnabled(
            this SvgOptionType optionType,
            InstrumentType it,
            MusicPatternType pt,
            DisplayModeType dmt) {
            if(dmt != DisplayModeType.Search) {
                if(optionType == SvgOptionType.Matches) {
                    return false;
                }
            }

            if(it != InstrumentType.Piano) {
                if(pt != MusicPatternType.Chords) {
                    if(optionType is SvgOptionType.Barres or SvgOptionType.Frets) {
                        return false;
                    }

                }

                return true;
            }

            // piano only below here
            return optionType is SvgOptionType.Notes or SvgOptionType.Roots or SvgOptionType.Matches;
        }

        public static double CentimetersToInches(this double cms) {
            return cms / 2.54d;
        }

        public static double InchesToCentimeters(this double inches) {
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

        public static void Dump(this Exception ex) {
            if(PlatformWrapper.Services is not { } ps ||
               ps.Logger is not { } logger) {
#if DEBUG
                Debug.WriteLine($"[{DateTime.Now}]{ex}");
#else
                Console.WriteLine($"[{DateTime.Now}]{ex}");
#endif
                return;
            }

            logger.WriteLine(ex.ToString());
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