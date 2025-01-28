using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class SuffixDisplayValueConverter : IValueConverter {
        public static readonly SuffixDisplayValueConverter Instance = new SuffixDisplayValueConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(value is not string suffix ||
               MainViewModel.Instance is not { } mvm) {
                return value;
            }

            return mvm.SelectedPatternType.ToDisplayValue(suffix);
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }
}