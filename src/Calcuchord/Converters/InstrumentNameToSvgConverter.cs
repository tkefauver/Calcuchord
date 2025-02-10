using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public class InstrumentNameToSvgConverter : IValueConverter {
        public static readonly InstrumentNameToSvgConverter Instance = new InstrumentNameToSvgConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is not string val_str ||
               !val_str.TryToEnum(out InstrumentType it)) {
                return value;
            }


            return MpAvFileIo.ReadTextFromResource(it.ToIconName());
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }

}