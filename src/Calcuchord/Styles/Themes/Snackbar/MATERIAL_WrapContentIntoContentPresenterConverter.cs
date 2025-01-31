using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data.Converters;

namespace Calcuchord {
    public class MATERIAL_WrapContentIntoContentPresenterConverter : IValueConverter {
        public static MATERIAL_WrapContentIntoContentPresenterConverter Instance { get; } =
            new MATERIAL_WrapContentIntoContentPresenterConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            return value is Control ? value : new ContentPresenter { Content = value };
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}