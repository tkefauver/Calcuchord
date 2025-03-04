using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class EnumToBoolConverter : IValueConverter {
        public static readonly EnumToBoolConverter Instance = new EnumToBoolConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is null ||
               parameter is not string paramStr) {
                return false;
            }

            return value.ToString() == paramStr;
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }

}