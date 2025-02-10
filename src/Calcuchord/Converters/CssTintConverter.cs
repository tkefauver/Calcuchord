using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class CssTintConverter : IValueConverter {
        public static readonly CssTintConverter Instance = new CssTintConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(parameter is not string palleteKey ||
               !palleteKey.TryToEnum(out PaletteColorType pct) ||
               !ThemeViewModel.Instance.P.TryGetValue(pct,out string hex)) {
                return string.Empty;
            }

            return $"* {{ fill: {hex}; stroke: {hex}; }}";
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }
}