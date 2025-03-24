using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public class HexToBrushOrColorConverter : IValueConverter {
        public static readonly HexToBrushOrColorConverter Instance = new HexToBrushOrColorConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is not string hex) {
                return null;
            }

            if(targetType == typeof(Color)) {
                return hex.ToAvColor();
            }

            return hex.ToAvBrush();
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }

}