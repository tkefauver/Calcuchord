using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace Calcuchord {
    [DoNotNotify]
    public partial class MainDrawerView : UserControl {
        public MainDrawerView() {
            InitializeComponent();
        }

        void InstrumentListBox_OnPointerReleased(object sender,PointerReleasedEventArgs e) {
            if(e.Source is not Control source ||
               !e.IsLeftRelease(e.Source as Visual) ||
               MainViewModel.Instance is not { } mvm) {
                return;
            }

            if(source.GetVisualAncestor<Button>() is not null) {
                // inst edit tap
            }

            // if(source.GetVisualAncestor<ListBoxItem>() is not { } lbi ||
            //    lbi.DataContext is not InstrumentViewModel vm ||
            //    !vm.IsSelected ||
            //    mvm.LastSelectedTuning != vm.SelectedTuning) {
            //     // new lbi click
            //     return;
            // }

            mvm.ForwardCommand.Execute(null);

        }
    }
}