using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class MatchToSvgConverter : IValueConverter {
        public static readonly MatchToSvgConverter Instance = new();

        ChordSvgBuilder ChordBuilder { get; } = new();
        ScaleSvgBuilder ScaleBuilder { get; } = new();
        PianoSvgBuilder PianoBuilder { get; } = new();


        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is not MatchViewModelBase mvmb) {
                return "<svg></svg>";
            }

            return null;
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }
}