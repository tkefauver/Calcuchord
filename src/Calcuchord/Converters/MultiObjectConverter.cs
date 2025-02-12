using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class MultiObjectConverter : IMultiValueConverter {
        public static readonly MultiObjectConverter Instance = new MultiObjectConverter();

        public object Convert(IList<object> values,Type targetType,object parameter,CultureInfo culture) {
            return values?.ToArray();
        }
    }
}