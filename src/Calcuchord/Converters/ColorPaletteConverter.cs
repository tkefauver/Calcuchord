using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public class ColorPaletteConverter : IValueConverter {
        public static readonly ColorPaletteConverter Instance = new();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(parameter is not string paramStr ||
               !paramStr.TryToEnum(out PaletteColorType pct) ||
               !ColorPalette.Instance.P.TryGetValue(pct,out string hex)) {
                return null;
            }
            return hex.ToAvBrush();
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }
}