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
               source.GetVisualAncestor<ListBoxItem>() is not { } lbi ||
               lbi.DataContext is not InstrumentViewModel ivm ||
               !e.IsLeftRelease(e.Source as Visual) ||
               MainViewModel.Instance is not { } mvm) {
                return;
            }

            mvm.ForwardCommand.Execute(mvm.Instruments.IndexOf(ivm));

        }
    }
}