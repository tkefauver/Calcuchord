using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class RightDrawerView : UserControl {
        public RightDrawerView() {
            InitializeComponent();
        }

        void InputElement_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(MainViewModel.Instance is not { } mvm ||
               sender is not TextBlock tb) {
                return;
            }

            if(tb.Text == "Suffix" &&
               mvm.SuffixOptions.Where(x => x.IsChecked) is { } sel_suff &&
               sel_suff.Any()) {
                sel_suff.ForEach(x => mvm.SelectOptionCommand.Execute(x));
            } else if(tb.Text == "Degree" &&
                      mvm.DegreeOptions.Where(x => x.IsChecked) is { } sel_deg &&
                      sel_deg.Any()) {
                sel_deg.ForEach(x => mvm.SelectOptionCommand.Execute(x));
            } else if(tb.Text == "Key" &&
                      mvm.KeyOptions.Where(x => x.IsChecked) is { } sel_key &&
                      sel_key.Any()) {
                sel_key.ForEach(x => mvm.SelectOptionCommand.Execute(x));
            } else if(tb.Text == "Sort") {
                OptionViewModel so1 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 0);
                OptionViewModel so2 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 1);
                OptionViewModel so3 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 2);
                so1.IsChecked = false;
                so2.IsChecked = false;
                so3.IsChecked = false;

                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so1),0);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so2),1);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so3),2);

                mvm.OnPropertyChanged(nameof(mvm.SortOption1));
                mvm.OnPropertyChanged(nameof(mvm.SortOption2));
                mvm.OnPropertyChanged(nameof(mvm.SortOption3));
            }
        }
    }
}