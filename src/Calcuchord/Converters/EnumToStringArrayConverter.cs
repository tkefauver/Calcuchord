using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class EnumToStringArrayConverter : IValueConverter {
        public static readonly EnumToStringArrayConverter Instance = new EnumToStringArrayConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is not Type enumType) {
                return new string[] { };
            }

            return Enum.GetNames(enumType);
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }
}