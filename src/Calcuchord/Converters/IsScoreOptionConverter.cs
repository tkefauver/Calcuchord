using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;

namespace Calcuchord {
    public class IsScoreOptionConverter : IValueConverter {
        public static readonly IsScoreOptionConverter Instance = new IsScoreOptionConverter();

        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
            if(MainViewModel.Instance is not { } mvm ||
               !int.TryParse(parameter.ToStringOrEmpty(),out int idx) ||
               idx >= mvm.SortOptions.Count) {
                return false;
            }

            return mvm.SortOptions[idx].OptionValue == MatchSortType.Score.ToString();
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
            return null;
        }
    }
}