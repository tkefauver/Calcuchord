using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class EnumToIntConverter : IValueConverter {
        public static readonly EnumToIntConverter Instance = new EnumToIntConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value == null) {
                return null;
            }


            if(targetType.IsEnum) {
                // convert int to enum
                return Enum.ToObject(targetType,value);
            }

            if(value.GetType().IsEnum) {
                // convert enum to int
                return System.Convert.ChangeType(
                    value,
                    Enum.GetUnderlyingType(value.GetType()));
            }

            return null;
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {

            // perform the same conversion in both directions
            return Convert(value,targetType,parameter,culture);
        }
    }
}