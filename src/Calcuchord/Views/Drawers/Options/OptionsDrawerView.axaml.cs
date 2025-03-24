using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class OptionsDrawerView : UserControl {
        public OptionsDrawerView() {
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

                OptionViewModel so4 =
                    mvm.SortOptions.FirstOrDefault(x => (int)x.OptionValue.ToEnum<MatchSortType>() == 3);
                so1.IsChecked = false;
                so2.IsChecked = false;
                so3.IsChecked = false;
                so4.IsChecked = false;

                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so1),0);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so2),1);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so3),2);
                mvm.SortOptions.Move(mvm.SortOptions.IndexOf(so4),3);

                mvm.RaisePropertyChanged(nameof(mvm.SortOption1));
                mvm.RaisePropertyChanged(nameof(mvm.SortOption2));
                mvm.RaisePropertyChanged(nameof(mvm.SortOption3));
                mvm.RaisePropertyChanged(nameof(mvm.SortOption4));

                mvm.UpdateMatchesAsync(MatchUpdateSource.SortToggle).FireAndForgetSafeAsync();
            }
        }

        void BookmarkItemView_PointerPressed(object sender,PointerPressedEventArgs e) {
            if(e.ClickCount <= 1 ||
               sender is not Control ctrl ||
               ctrl.DataContext is not BookmarkGroupViewModel bmgvm) {
                return;
            }

            bmgvm.DoubleTapCommand.Execute(null);

        }

        DateTime? LastDoubleTappedDt { get; set; }
        object LastDoubleTappedSender { get; set; }

        void InputElement_OnDoubleTapped(object sender,TappedEventArgs e) {
            if(sender is not Control ctrl ||
               ctrl.DataContext is not BookmarkGroupViewModel bmgvm) {
                return;
            }

            LastDoubleTappedDt = DateTime.Now;
            LastDoubleTappedSender = sender;

            bmgvm.DoubleTapCommand.Execute(null);
        }

        void InputElement_OnTapped(object sender,TappedEventArgs e) {
            if(sender is not Control ctrl ||
               ctrl.DataContext is not BookmarkGroupViewModel bmgvm) {
                return;
            }

            Dispatcher.UIThread.Post(
                async () => {
                    await Task.Delay(1_000);

                    if(LastDoubleTappedDt is { } ldtdt &&
                       DateTime.Now - ldtdt < TimeSpan.FromSeconds(2) &&
                       LastDoubleTappedSender == sender) {
                        // cancel select toggle
                        return;
                    }

                    if(bmgvm.Parent.SelectedBookmarkGroups.Contains(bmgvm)) {
                        bmgvm.Parent.SelectedBookmarkGroups.Remove(bmgvm);
                    } else {
                        bmgvm.Parent.SelectedBookmarkGroups.Add(bmgvm);
                    }
                });
        }
    }
}